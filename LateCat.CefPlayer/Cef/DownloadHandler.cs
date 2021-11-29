using CefSharp;
using System.IO;

namespace LateCat.CefPlayer
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            OnBeforeDownloadFired?.Invoke(this, downloadItem);

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    string lateCatDir = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).ToString()).ToString()).ToString(), "tmpdata");
                    Directory.CreateDirectory(Path.Combine(lateCatDir, "downloads"));
                    lateCatDir = Path.Combine(lateCatDir, "downloads");

                    callback.Continue(Path.Combine(lateCatDir, downloadItem.SuggestedFileName), showDialog: false);
                }
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadUpdatedFired?.Invoke(this, downloadItem);

            if (downloadItem.IsCancelled)
            {

            }
            else if (downloadItem.IsComplete)
            {

            }
        }
    }
}
