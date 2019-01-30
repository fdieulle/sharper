using System;

namespace Sharper.Converters
{
    public class NullConverter : IConverter
    {
        public static readonly Type[] Types = { typeof(void) };
        public static readonly IConverter Instance = new NullConverter();

        private NullConverter() { }

        #region Implementation of IConverter

        public Type[] GetClrTypes() => Types;

        public object Convert(Type type) => null;

        #endregion
    }
}