using System;
using System.Diagnostics;

namespace LateCat.PoseidonEngine.Utilities
{
    internal static class LinkHandler
    {
        public static void OpenBrowser(Uri uri)
        {
            try
            {
                var ps = new ProcessStartInfo(uri.AbsoluteUri)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);

            }
            catch
            {

            }
        }

        public static void OpenBrowser(string address)
        {
            try
            {
                OpenBrowser(new Uri(address));
            }
            catch
            {

            }
        }

        public static Uri SanitizeUrl(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException(null, nameof(address));
            }

            Uri uri;

            try
            {
                uri = new Uri(address);
            }
            catch (UriFormatException)
            {
                uri = new UriBuilder(address)
                {
                    Scheme = "https",
                    Port = -1,
                }.Uri;
            }
            return uri;
        }
    }
}
