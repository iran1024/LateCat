using System.IO;

namespace LateCat.Installer.Models
{
    public class ResourceMap
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string DestinationDirectory { get; set; }

        public Stream? ResourceStream { get; set; }
    }
}
