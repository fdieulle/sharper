using System;
using System.Linq;
using RDotNet;

namespace Sharper.Converters.RDotNet
{
    public class IntegerDiffTimeVectorConverter : VectorConverter<int, TimeSpan>
    {
        private readonly string _units;

        public IntegerDiffTimeVectorConverter(Vector<int> vector)
            : base(vector) => _units = vector.GetUnits();

        #region Overrides of VectorConverter<int,TimeSpan>

        protected override object ConvertToSingle(int value) => value.ToTimeSpan(_units);

        protected override object ConvertToArray(int[] array) => array.ToTimeSpan(_units);

        protected override object ConvertToList(int[] array) => array.ToTimeSpan(_units).ToList();

        #endregion
    }

    public class IntegerDiffTimeMatrixConverter : MatrixConverter<int, TimeSpan>
    {
        private readonly string _units;

        public IntegerDiffTimeMatrixConverter(Matrix<int> matrix) 
            : base(matrix) => _units = matrix.GetUnits();

        #region Overrides of MatrixConverter<int,TimeSpan>

        protected override object ConvertToMatrix(int[,] matrix) => matrix.ToTimeSpan(_units);

        #endregion
    }

    public class NumericDiffTimeVectorConverter : VectorConverter<double, TimeSpan>
    {
        private readonly string _units;

        public NumericDiffTimeVectorConverter(Vector<double> vector)
            : base(vector)
        {
            _units = vector.GetUnits();
        }

        #region Overrides of VectorConverter<int,TimeSpan>

        protected override object ConvertToSingle(double value) => value.ToTimeSpan(_units);

        protected override object ConvertToArray(double[] array) => array.ToTimeSpan(_units);

        protected override object ConvertToList(double[] array) => array.ToTimeSpan(_units).ToList();

        #endregion
    }

    public class NumericDiffTimeMatrixConverter : MatrixConverter<double, TimeSpan>
    {
        private readonly string _units;

        public NumericDiffTimeMatrixConverter(Matrix<double> matrix)
            : base(matrix) => _units = matrix.GetUnits();

        #region Overrides of MatrixConverter<int,TimeSpan>

        protected override object ConvertToMatrix(double[,] matrix) => matrix.ToTimeSpan(_units);

        #endregion
    }
}
