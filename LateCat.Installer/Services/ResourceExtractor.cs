using LateCat.Installer.Abstractions;
using System.IO;
using System.Reflection;

namespace LateCat.Installer.Services
{
    class ResourceExtractor : IResourceExtractor
    {
        private readonly Assembly _assembly;

        public ResourceExtractor()
        {
            _assembly = Assembly.GetExecutingAssembly();
        }

        public Stream? GetResource(string name)
        {
            return _assembly.GetManifestResourceStream(name);
        }
    }
}