using System;
using RDotNet;

namespace Sharper.Converters.RDotNet
{
    public class RDotNetConverter : IDataConverter
    {
        private static readonly REngine engine = REngine.GetInstance(initialize: false);

        #region Implementation of IDataConverter

        public bool IsDefined(Type type)
        {
            return true;
        }

        public IConverter GetConverter(ulong address)
        {
            return NullConverter.Instance;
        }

        public IntPtr ConvertBack(Type type, object data)
        {
            return new NumericVector(engine, new []{1.0, 2.0, 3.0}).DangerousGetHandle();
        }

        #endregion
    }
}
