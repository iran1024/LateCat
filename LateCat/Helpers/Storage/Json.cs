using Newtonsoft.Json;
using System;
using System.IO;

namespace LateCat.Helpers
{
    public static class Json<T>
    {
        public static T LoadData(string filePath)
        {
            using var file = File.OpenText(filePath);
            var serializer = new JsonSerializer();

            var tmp = (T)serializer.Deserialize(file, typeof(T))!;

            return tmp != null ? tmp : throw new ArgumentNullException("json null/corrupt");
        }

        public static void StoreData(string filePath, T data)
        {
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };

            using var sw = new StreamWriter(filePath);
            using var writer = new JsonTextWriter(sw);

            serializer.Serialize(writer, data);
        }
    }
}
