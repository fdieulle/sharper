using System;

namespace Sharper.Converters
{
    public interface IDataConverter
    {
        bool IsDefined(Type type);

        IConverter GetConverter(long address);

        object ConvertBack(Type type, object data);
    }
}