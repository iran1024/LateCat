using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IWebView2Provider
    {
        WebView2 GetWebView2();

        Task EnsureCoreWebView2Async();
    }
}