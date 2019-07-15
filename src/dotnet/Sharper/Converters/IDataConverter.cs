using System;

namespace Sharper.Converters
{
    public interface IDataConverter
    {
        bool IsDefined(Type type);

        IConverter GetConverter(long pointer);

        long ConvertBack(Type type, object data);

        void Release(long pointer);
    }
}