using System;
using RDotNet;

namespace Sharper.Converters.RDotNet
{
    public class MatrixConverter<TIn, TOut> : IConverter
    {
        private static readonly Type[] types = new[] { typeof(TOut[,]) };

        private readonly Matrix<TIn> _matrix;

        public MatrixConverter(Matrix<TIn> matrix) => _matrix = matrix;

        #region Implementation of IConverter

        public Type[] GetClrTypes() => types;

        public object Convert(Type type) => ConvertToMatrix(_matrix.ToArray());

        #endregion

        protected virtual object ConvertToMatrix(TIn[,] matrix) => matrix;
    }

    public class MatrixConverter<T> : MatrixConverter<T, T>
    {
        public MatrixConverter(Matrix<T> matrix) 
            : base(matrix) { }
    }
}
