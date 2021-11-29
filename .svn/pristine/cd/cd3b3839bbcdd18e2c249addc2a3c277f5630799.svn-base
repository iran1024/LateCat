using CefSharp;
using Newtonsoft.Json;
using System.Text;

namespace LateCat.CefPlayer
{
    public static class DevToolsExtensions
    {
        public enum CaptureFormat
        {
            JPEG,
            Webp,
            Png
        }

        private static int LastMessageId = 600000;

        public static async Task<byte[]> CaptureScreenShotAsPng(this IWebBrowser chromiumWebBrowser, CaptureFormat format)
        {
            var host = chromiumWebBrowser.GetBrowserHost();

            if (host == null || host.IsDisposed)
            {
                throw new Exception("BrowserHost is Null or Disposed");
            }

            var msgId = Interlocked.Increment(ref LastMessageId);

            var observer = new TaskMethodDevToolsMessageObserver(msgId);

            using var observerRegistration = host.AddDevToolsMessageObserver(observer);

            int id = 0;
            const string methodName = "Page.captureScreenshot";

            Dictionary<string, object> param = null;
            switch (format)
            {
                case CaptureFormat.JPEG:
                    param = new Dictionary<string, object> { { "format", "jpeg" } };
                    break;
                case CaptureFormat.Webp:
                    param = new Dictionary<string, object> { { "format", "webp" } };
                    break;
                case CaptureFormat.Png:
                    param = null;
                    break;
            }

            if (Cef.CurrentlyOnThread(CefThreadIds.TID_UI))
            {
                id = host.ExecuteDevToolsMethod(msgId, methodName, param);
            }
            else
            {
                id = await Cef.UIThreadTaskFactory.StartNew(() =>
                {
                    return host.ExecuteDevToolsMethod(msgId, methodName, param);
                });
            }

            if (id != msgId)
            {
                throw new Exception("Message Id doesn't match the provided Id");
            }

            var result = await observer.Task;

            var success = result.Item1;

            dynamic response = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(result.Item2));

            if (success)
            {
                return Convert.FromBase64String((string)response.data);
            }

            var code = (string)response.code;
            var message = (string)response.message;

            throw new Exception(code + ":" + message);
        }
    }
}
