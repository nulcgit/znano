using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;

namespace Znano

{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeBrowser();
        }

        private async void InitializeBrowser()
        {
            await InitialWebView.EnsureCoreWebView2Async(null);
            InitialWebView.Source = new Uri("https://www.google.com");
            InitialWebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            AddNewTab(e.Uri);
        }

        private void AddNewTab(string url = "https://www.google.com")
        {
            var webView = new Microsoft.Web.WebView2.Wpf.WebView2();
            var tabItem = new TabItem
            {
                Header = "New Tab",
                Content = new Grid { Children = { webView } }
            };

            TabControl.Items.Add(tabItem);
            TabControl.SelectedItem = tabItem;

            webView.NavigationStarting += WebView_NavigationStarting;
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                {
                    webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                    webView.Source = new Uri(url);
                }
            };
            NavigateToUrl();
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (sender is Microsoft.Web.WebView2.Wpf.WebView2 webView)
            {
                UrlTextBox.Text = e.Uri;
                UpdateNavigationButtons(webView);
            }
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (sender is Microsoft.Web.WebView2.Wpf.WebView2 webView)
            {
                UpdateNavigationButtons(webView);
                var tabItem = TabControl.SelectedItem as TabItem;
                if (tabItem != null && webView.CoreWebView2 != null)
                {
                    tabItem.Header = webView.CoreWebView2.DocumentTitle.Length > 20
                        ? webView.CoreWebView2.DocumentTitle.Substring(0, 20) + "..."
                        : webView.CoreWebView2.DocumentTitle;
                }
            }
        }

        private void UpdateNavigationButtons(Microsoft.Web.WebView2.Wpf.WebView2 webView)
        {
            BackButton.IsEnabled = webView.CoreWebView2?.CanGoBack == true;
            ForwardButton.IsEnabled = webView.CoreWebView2?.CanGoForward == true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            if (webView?.CoreWebView2?.CanGoBack == true)
            {
                webView.CoreWebView2.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            if (webView?.CoreWebView2?.CanGoForward == true)
            {
                webView.CoreWebView2.GoForward();
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            webView?.CoreWebView2?.Reload();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            webView?.CoreWebView2?.Stop();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToUrl();
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NavigateToUrl();
            }
        }

        private void NavigateToUrl()
        {
            var webView = GetCurrentWebView();
            if (webView == null) return;

            string url = UrlTextBox.Text;
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            try
            {
                webView.Source = new Uri(url);
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Invalid URL format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && TabControl.Items.Count > 1)
            {
                // Find the parent TabItem
                var tabItem = FindParent<TabItem>(button);
                if (tabItem != null)
                {
                    // Dispose of WebView2 resources
                    var webView = (tabItem.Content as Grid)?.Children.OfType<Microsoft.Web.WebView2.Wpf.WebView2>().FirstOrDefault();
                    if (webView != null)
                    {
                        webView.Dispose();
                    }

                    // Remove the tab
                    TabControl.Items.Remove(tabItem);
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var webView = GetCurrentWebView();
            if (webView?.CoreWebView2 != null)
            {
                UrlTextBox.Text = webView.CoreWebView2.Source.ToString();
                UpdateNavigationButtons(webView);
            }
        }

        private Microsoft.Web.WebView2.Wpf.WebView2 GetCurrentWebView()
        {
            if (TabControl.SelectedItem is TabItem tabItem)
            {
                return (tabItem.Content as Grid)?.Children.OfType<Microsoft.Web.WebView2.Wpf.WebView2>().FirstOrDefault();
            }
            return null;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}