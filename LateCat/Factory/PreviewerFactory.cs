using LateCat.PoseidonEngine.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace LateCat.Factory
{
    internal class PreviewerFactory : IPreviewerFactory
    {
        private readonly IDictionary<string, IPreviewer> _maps = new Dictionary<string, IPreviewer>();

        public IPreviewer Get<TPreviewer>(IWallpaperMetadata metadata)
            where TPreviewer : IPreviewer
        {
            var typeName = typeof(TPreviewer).Name;

            if (_maps.ContainsKey(typeName))
            {
                return _maps[typeName].GetPreviewer(metadata);
            }
            else
            {
                var instance = (TPreviewer)App.Services.GetServices<IPreviewer>()
                    .First(o => o.GetType() == typeof(TPreviewer));

                _maps.Add(typeName, instance);

                return instance.GetPreviewer(metadata);
            }
        }
    }
}