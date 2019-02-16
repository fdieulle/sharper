using System;

namespace Sharper.Converters
{
    public interface IConverter
    {
        Type[] GetClrTypes();

        object Convert(Type type);
    }
}
