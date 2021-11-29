using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace LateCat.Core
{
    [MarkupExtensionReturnType(typeof(IEnumerable<Enum>))]
    public sealed class EnumValuesExtension : MarkupExtension
    {
        private EnumValuesExtension()
        {

        }

        public EnumValuesExtension(Type enumType)
            => EnumType = enumType;

        [ConstructorArgument("enumType")]
        public Type EnumType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
            => Enum.GetValues(EnumType);
    }
}
