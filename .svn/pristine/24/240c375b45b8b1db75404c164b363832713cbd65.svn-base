using Newtonsoft.Json.Linq;
using System.IO;

namespace LateCat.Core.Cef
{
    internal class PropertiesJsonHelper
    {
        public static void SaveProperties(string path, JObject rss)
        {
            try
            {
                File.WriteAllText(path, rss.ToString());
            }
            catch
            {

            }
        }

        public static JObject LoadProperties(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var data = JObject.Parse(json);
                return data;
            }
            catch
            {
                return null;
            }
        }
    }
}
