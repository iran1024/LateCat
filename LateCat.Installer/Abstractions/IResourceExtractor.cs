using System.IO;

namespace LateCat.Installer.Abstractions
{
    internal interface IResourceExtractor
    {
        Stream? GetResource(string name);
    }
}