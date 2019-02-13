using System;

namespace Sharper.Converters
{
    public interface IDataConverter
    {
        bool IsDefined(Type type);

        IConverter GetConverter(ulong address);

        IntPtr ConvertBack(Type type, object data);
    }
}