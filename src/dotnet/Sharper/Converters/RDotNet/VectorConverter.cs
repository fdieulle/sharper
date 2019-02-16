using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RDotNet;

namespace Sharper.Converters.RDotNet
{
    public class VectorConverter<TIn, TOut> : IConverter
    {
        private static readonly Type[] multiValues = { typeof(TOut[]), typeof(List<TOut>), typeof(IList<TOut>), typeof(ICollection<TOut>), typeof(IEnumerable<TOut>), typeof(Array), typeof(IEnumerable) };
        private static readonly Type[] singleValue = new[] { typeof(TOut) }.Concat(multiValues).ToArray();

        private readonly Vector<TIn> _vector;
        private readonly Type[] _types;

        public VectorConverter(Vector<TIn> vector)
        {
            _vector = vector;
            _types = vector.Length <= 1
                ? singleValue
                : multiValues;
        }

        #region Implementation of IConverter

        public Type[] GetClrTypes() => _types;

        public object Convert(Type type)
        {
            if (type == typeof(TOut))
                return ConvertToSingle(_vector.ToArray()[0]);
            if (type == typeof(TOut[]) || type == typeof(Array) || type == typeof(IEnumerable))
                return ConvertToArray(_vector.ToArray());
            if (type == typeof(List<TOut>) || type == typeof(IList<TOut>) || type == typeof(ICollection<TOut>) || type == typeof(IEnumerable<TOut>))
                return ConvertToList(_vector.ToArray());

            return ConvertToOther(type);
        }

        #endregion

        protected virtual object ConvertToSingle(TIn value) => value;

        protected virtual object ConvertToArray(TIn[] array) => array;

        protected virtual object ConvertToList(TIn[] array) => array.ToList();

        protected virtual object ConvertToOther(Type type) 
            => throw new InvalidOperationException($"Unexpected type on converter from R: {_vector.Type} to Clr: {type}");
    }

    public class VectorConverter<T> : VectorConverter<T, T>
    {
        public VectorConverter(Vector<T> vector) 
            : base(vector) { }
    }
}
