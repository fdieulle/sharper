using System;
using System.Linq;
using RDotNet;
using Sharper.Converters.Resources;

namespace Sharper.Converters.RDotNet
{
    public class PosixVectorConverter : VectorConverter<double, DateTime>
    {
        private readonly TimeZoneInfo _timeZone;

        public PosixVectorConverter(Vector<double> vector) 
            : base(vector) => _timeZone = vector.GetWindowsTimezone();

        #region Overrides of VectorConverter<double,DateTime>

        protected override object ConvertToSingle(double value) => value.FromTicks(_timeZone);

        protected override object ConvertToArray(double[] array) => array.FromTicks(_timeZone);

        protected override object ConvertToList(double[] array) => array.FromTicks(_timeZone).ToList();

        #endregion
    }

    public class PosixMatrixConverter : MatrixConverter<double, DateTime>
    {
        private readonly TimeZoneInfo _timeZone;

        public PosixMatrixConverter(Matrix<double> matrix) 
            : base(matrix) => _timeZone = matrix.GetWindowsTimezone();

        #region Overrides of MatrixConverter<double,DateTime>

        protected override object ConvertToMatrix(double[,] matrix) => matrix.FromTicks(_timeZone);

        #endregion
    }
}
