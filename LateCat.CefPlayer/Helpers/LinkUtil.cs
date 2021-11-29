﻿using System.Text.RegularExpressions;
using System.Web;

namespace LateCat.CefPlayer.Helpers
{
    public static class LinkUtil
    {
        public static string GetYouTubeVideoIdFromUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                try
                {
                    uri = new UriBuilder("http", url).Uri;
                }
                catch
                {
                    return "";
                }
            }

            string host = uri.Host;
            string[] youTubeHosts = { "www.youtube.com", "youtube.com", "youtu.be", "www.youtu.be" };
            if (!youTubeHosts.Contains(host))
                return "";

            var query = HttpUtility.ParseQueryString(uri.Query);
            if (query.AllKeys.Contains("v"))
            {
                return Regex.Match(query["v"], @"^[a-zA-Z0-9_-]{11}$").Value;
            }
            else if (query.AllKeys.Contains("u"))
            {
                return Regex.Match(query["u"], @"/watch\?v=([a-zA-Z0-9_-]{11})").Groups[1].Value;
            }
            else
            {
                var last = uri.Segments.Last().Replace("/", "");
                if (Regex.IsMatch(last, @"^v=[a-zA-Z0-9_-]{11}$"))
                    return last.Replace("v=", "");

                var segments = uri.Segments;
                if (segments.Length > 2 && segments[^2] != "v/" && segments[^2] != "watch/")
                    return "";

                return Regex.Match(last, @"^[a-zA-Z0-9_-]{11}$").Value;
            }
        }

        public static bool TryParseShadertoy(string url, ref string html)
        {
            if (!url.Contains("shadertoy.com/view"))
            {
                return false;
            }

            try
            {
                _ = SanitizeUrl(url);
            }
            catch
            {
                return false;
            }

            url = url.Replace("view/", "embed/");
            html = @"<!DOCTYPE html><html lang=""en"" dir=""ltr""> <head> <meta charset=""utf - 8""> 
                    <title>Digital Brain</title> <style media=""screen""> iframe { position: fixed; width: 100%; height: 100%; top: 0; right: 0; bottom: 0;
                    left: 0; z-index; -1; pointer-events: none;  } </style> </head> <body> <iframe width=""640"" height=""360"" frameborder=""0"" 
                    src=" + url + @"?gui=false&t=10&paused=false&muted=true""></iframe> </body></html>";
            return true;
        }

        public static Uri SanitizeUrl(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException(nameof(address));
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
