using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Ebook_Reader.Views
{
    public partial class ReaderView : UserControl
    {
        private bool _isWebViewInitialized;
        private readonly SemaphoreSlim _webViewInitLock = new(1, 1);

        public ReaderView()
        {
            InitializeComponent();
            DataContextChanged += ReaderView_DataContextChanged;
        }

        private void ReaderView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModels.ReaderViewModel oldVm)
            {
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            }

            if (e.NewValue is ViewModels.ReaderViewModel newVm)
            {
                newVm.PropertyChanged += ViewModel_PropertyChanged;
                _ = Dispatcher.InvokeAsync(() => UpdateWebView(newVm));
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ViewModels.ReaderViewModel vm)
                return;

            if (e.PropertyName == nameof(ViewModels.ReaderViewModel.Content) || 
                e.PropertyName == nameof(ViewModels.ReaderViewModel.CurrentTheme))
            {
                _ = Dispatcher.InvokeAsync(() => UpdateWebView(vm));
            }
        }

        private async Task EnsureWebViewAsync()
        {
            if (_isWebViewInitialized)
                return;

            await _webViewInitLock.WaitAsync();
            try
            {
                if (!_isWebViewInitialized)
                {
                    await webView.EnsureCoreWebView2Async(null);
                    _isWebViewInitialized = true;
                }
            }
            finally
            {
                _webViewInitLock.Release();
            }
        }

        private async Task UpdateWebView(ViewModels.ReaderViewModel viewModel)
        {
            try
            {
                await EnsureWebViewAsync();

                if (string.IsNullOrWhiteSpace(viewModel.Content))
                {
                    webView.CoreWebView2.NavigateToString(LoadingHtml());
                    return;
                }

                if (viewModel.IsPdf)
                {
                    // Handle PDF path as URI
                    try
                    {
                        var uri = new Uri(viewModel.Content);
                        webView.CoreWebView2.Navigate(uri.AbsoluteUri);
                    }
                    catch
                    {
                        webView.CoreWebView2.Navigate(viewModel.Content);
                    }
                    return;
                }

                RenderHtml(viewModel.Content, viewModel.CurrentTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView error: {ex}");
            }
        }

        private void RenderHtml(string rawHtml, ViewModels.ReadingTheme theme)
        {
            // Extract body and head content with better robustness
            string body = ExtractTag(rawHtml, "body") ?? ScrubTopLevelTags(rawHtml);
            string head = ExtractTag(rawHtml, "head") ?? "";

            string themeCss = string.Empty;
            string forcedVisibility = "";

            switch (theme)
            {
                case ViewModels.ReadingTheme.Paper:
                    themeCss = "background-color: #F4ECD8 !important; color: #3C3C3C !important;";
                    forcedVisibility = "* { color: #3C3C3C !important; background-color: transparent !important; }";
                    break;
                case ViewModels.ReadingTheme.Sepia:
                    themeCss = "background-color: #704214 !important; color: #EEE !important;";
                    forcedVisibility = "* { color: #EEE !important; background-color: transparent !important; }";
                    break;
                case ViewModels.ReadingTheme.Night:
                    themeCss = "background-color: #1A1A1A !important; color: #CCC !important;";
                    forcedVisibility = "* { color: #CCC !important; background-color: transparent !important; }";
                    break;
                default:
                    // Default theme: we still force visibility and basic contrast to avoid blank screens
                    themeCss = "background-color: #FDFDFD !important; color: #1A1A1A !important;";
                    forcedVisibility = "* { color: inherit !important; background-color: transparent !important; }";
                    break;
            }

            string finalHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    {head}
    <style>
        /* Extreme Reset and forced visibility */
        html, body {{
            {themeCss}
            margin: 0 !important;
            padding: 0 !important;
            width: 100% !important;
            min-height: 100% !important;
        }}

        body {{
            font-family: 'Segoe UI', Tahoma, Arial, sans-serif !important;
            font-size: 20px !important;
            line-height: 1.8 !important;
            max-width: 900px !important;
            margin: 0 auto !important;
            padding: 50px 10% !important;
            word-wrap: break-word !important;
            overflow-x: hidden !important;
        }}

        /* Force visibility and disable destructive styling */
        {forcedVisibility}
        * {{
            visibility: visible !important;
            opacity: 1 !important;
            position: static !important;
            max-width: 100% !important;
            box-sizing: border-box !important;
        }}

        h1, h2, h3, h4 {{ 
            color: #E76E6E !important; 
            margin-top: 1.5em !important; 
            display: block !important;
        }}

        p, div, section {{ 
            display: block !important; 
            margin-bottom: 1em !important; 
        }}

        img {{
            max-width: 100% !important;
            height: auto !important;
            display: block !important;
            margin: 20px auto !important;
            border-radius: 8px;
        }}

        pre {{
            white-space: pre-wrap !important;
        }}
    </style>
</head>
<body>
    {body}
</body>
</html>";

            System.Diagnostics.Debug.WriteLine($"[WebView] Rendering HTML. Body length: {body.Length}. Theme: {theme}");
            webView.CoreWebView2.NavigateToString(finalHtml);
        }

        private string ScrubTopLevelTags(string html)
        {
            string scrubbed = html;
            string[] tagsToRemove = { "<html>", "</html>", "<body", "</body>", "<head>", "</head>", "<?xml", "<!DOCTYPE" };
            
            foreach (var tag in tagsToRemove)
            {
                int index;
                while ((index = scrubbed.IndexOf(tag, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if (tag.EndsWith(">"))
                    {
                        scrubbed = scrubbed.Remove(index, tag.Length);
                    }
                    else
                    {
                        // Handle open tags with attributes like <body xmlns="...">
                        int end = scrubbed.IndexOf(">", index);
                        if (end != -1) scrubbed = scrubbed.Remove(index, end - index + 1);
                        else break;
                    }
                }
            }
            return scrubbed;
        }

        private static string? ExtractTag(string html, string tag)
        {
            int start = html.IndexOf($"<{tag}", StringComparison.OrdinalIgnoreCase);
            if (start == -1) return null;

            int openEnd = html.IndexOf(">", start);
            int end = html.LastIndexOf($"</{tag}>", StringComparison.OrdinalIgnoreCase);

            if (openEnd == -1 || end == -1 || end <= openEnd)
                return null;

            return html.Substring(openEnd + 1, end - openEnd - 1);
        }

        private static string LoadingHtml() =>
            @"<html>
            <body style='background:#FDFDFD; display:flex; justify-content:center; align-items:center; height:100vh; margin:0; font-family:Segoe UI;'>
                <h2 style='color:#E76E6E;'>Loading your book...</h2>
            </body>
            </html>";
    }
}
