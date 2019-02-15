using System;
using System.Collections.Generic;
using RDotNet;
using RDotNet.Internals;
using Sharper.Loggers;

namespace Sharper.Converters.RDotNet
{
    public class RDotNetConverter : IDataConverter
    {
        private readonly ILogger _logger;
        private static readonly REngine engine = REngine.GetInstance(initialize: false);

        private readonly Dictionary<SymbolicExpressionType, Func<SymbolicExpression, IConverter>> _converters = new Dictionary<SymbolicExpressionType, Func<SymbolicExpression, IConverter>>();
        private readonly Dictionary<Type, Func<object, SymbolicExpression>> _convertersBack = new Dictionary<Type, Func<object, SymbolicExpression>>();

        public RDotNetConverter(ILogger logger)
        {
            _logger = logger;

            SetupRToDotNetConverters();
            SetupDotNetToRConverters();
        }

        #region Implementation of IDataConverter

        public bool IsDefined(Type type)
            => _convertersBack.ContainsKey(type);

        public IConverter GetConverter(long pointer)
        {
            var sexp = engine.CreateFromNativeSexp(new IntPtr(pointer));

            _logger.DebugFormat("SEXP type: {0}", sexp.Type);

            if (_converters.TryGetValue(sexp.Type, out var factory))
                return factory(sexp);

            throw new InvalidCastException($"Unable to find a converter from R type: {sexp.Type}");
        }

        public long ConvertBack(Type type, object data)
        {
            var sexp = ConvertToSexp(type, data);
            return (long)(sexp?.DangerousGetHandle() ?? engine.NilValue.DangerousGetHandle());
        }

        #endregion

        public SymbolicExpression ConvertToSexp(Type type, object data)
        {
            if (data == null) return engine.NilValue;

            if (_convertersBack.TryGetValue(type, out var factory))
                return factory(data) ?? engine.NilValue;

            if (data is SymbolicExpression sexp) return sexp;

            if (data.GetType().IsEnum)
                return ConvertToSexp(typeof(string), data.ToString());

            // Try to convert a generic list or dictionary first
            if (ListConverter.TryConvertBack(engine, this, data, out var result))
                return result;

            // Otherwise convert to an external pointer
            return engine.ToExternalPointer(data);
        }

        #region Setup Converter R -> .Net

        private void SetupRToDotNetConverters()
        {
            SetupRToDotNetConverter(SymbolicExpressionType.Null, p => NullConverter.Instance);
            SetupRToDotNetConverter(SymbolicExpressionType.CharacterVector, ConvertFromCharacterVector);
            SetupRToDotNetConverter(SymbolicExpressionType.IntegerVector, ConvertFromIntegerVector);
            SetupRToDotNetConverter(SymbolicExpressionType.NumericVector, ConvertFromNumericalVector);
            SetupRToDotNetConverter(SymbolicExpressionType.LogicalVector, ConvertFromLogicalVector);
            SetupRToDotNetConverter(SymbolicExpressionType.ExternalPointer, p => new ExternalPtrConverter(p));
            SetupRToDotNetConverter(SymbolicExpressionType.List, p => new ListConverter(p.AsList(), this));
        }

        public void SetupRToDotNetConverter(SymbolicExpressionType type, Func<SymbolicExpression, IConverter> factory)
        {
            if(_logger.IsDebugEnabled)
                _logger.DebugFormat(
                    _converters.ContainsKey(type)
                        ? "Override converter R -> .Net, Type: {0}"
                        : "Setup converter R -> .Net, Type: {0}", type);

            _converters[type] = factory;
        }

        public bool RemoveRToDotNetConverter(SymbolicExpressionType type)
        {
            if (_logger.IsDebugEnabled && _converters.ContainsKey(type))
                _logger.DebugFormat("Remove converter R -> .Net, Type: {0}", type);

            return _converters.Remove(type);
        }

        public Func<SymbolicExpression, IConverter> GetRToDotNetConverter(SymbolicExpressionType type)
        {
            _converters.TryGetValue(type, out var factory);
            return factory;
        }

        private static IConverter ConvertFromCharacterVector(SymbolicExpression sexp)
        {
            return sexp.IsMatrix()
                ? (IConverter)new MatrixConverter<string>(sexp.AsCharacterMatrix())
                : new VectorConverter<string>(sexp.AsCharacter());
        }

        private static IConverter ConvertFromIntegerVector(SymbolicExpression sexp)
        {
            if (sexp.IsMatrix())
            {
                if (sexp.IsDiffTime())
                    return new IntegerDiffTimeMatrixConverter(sexp.AsIntegerMatrix());

                return new MatrixConverter<int>(sexp.AsIntegerMatrix());
            }

            if (sexp.IsDiffTime())
                return new IntegerDiffTimeVectorConverter(sexp.AsInteger());

            return new VectorConverter<int>(sexp.AsInteger());
        }

        private static IConverter ConvertFromLogicalVector(SymbolicExpression sexp)
        {
            return sexp.IsMatrix()
                ? (IConverter)new MatrixConverter<bool>(sexp.AsLogicalMatrix())
                : new VectorConverter<bool>(sexp.AsLogical());
        }

        private static IConverter ConvertFromNumericalVector(SymbolicExpression sexp)
        {
            var isPosixct = sexp.IsPosixct();
            var isDiffTime = !isPosixct && sexp.IsDiffTime();

            if (sexp.IsMatrix())
            {
                if (isPosixct)
                    return new PosixMatrixConverter(sexp.AsNumericMatrix());
                if (isDiffTime)
                    return new NumericDiffTimeMatrixConverter(sexp.AsNumericMatrix());

                return new MatrixConverter<double>(sexp.AsNumericMatrix());
            }

            if (isPosixct)
                return new PosixVectorConverter(sexp.AsNumeric());
            if (isDiffTime)
                return new NumericDiffTimeVectorConverter(sexp.AsNumeric());

            return new VectorConverter<double>(sexp.AsNumeric());
        }

        #endregion

        #region Setup Converter .Net -> R

        private void SetupDotNetToRConverters()
        {
            SetupDotNetToRConverter(typeof(void), p => null);

            SetupDotNetToRConverter(typeof(string), p => engine.CreateCharacter((string)p));
            SetupDotNetToRConverter(typeof(string[]), p => engine.CreateCharacterVector((string[])p));
            SetupDotNetToRConverter(typeof(List<string>), p => engine.CreateCharacterVector((IEnumerable<string>)p));
            SetupDotNetToRConverter(typeof(IList<string>), p => engine.CreateCharacterVector((IEnumerable<string>)p));
            SetupDotNetToRConverter(typeof(ICollection<string>), p => engine.CreateCharacterVector((IEnumerable<string>)p));
            SetupDotNetToRConverter(typeof(IEnumerable<string>), p => engine.CreateCharacterVector((IEnumerable<string>)p));
            SetupDotNetToRConverter(typeof(string[,]), p => engine.CreateCharacterMatrix((string[,])p));

            SetupDotNetToRConverter(typeof(int), p => engine.CreateInteger((int)p));
            SetupDotNetToRConverter(typeof(int[]), p => engine.CreateIntegerVector((int[])p));
            SetupDotNetToRConverter(typeof(List<int>), p => engine.CreateIntegerVector((IEnumerable<int>)p));
            SetupDotNetToRConverter(typeof(IList<int>), p => engine.CreateIntegerVector((IEnumerable<int>)p));
            SetupDotNetToRConverter(typeof(ICollection<int>), p => engine.CreateIntegerVector((IEnumerable<int>)p));
            SetupDotNetToRConverter(typeof(IEnumerable<int>), p => engine.CreateIntegerVector((IEnumerable<int>)p));
            SetupDotNetToRConverter(typeof(int[,]), p => engine.CreateIntegerMatrix((int[,])p));

            SetupDotNetToRConverter(typeof(bool), p => engine.CreateLogical((bool)p));
            SetupDotNetToRConverter(typeof(bool[]), p => engine.CreateLogicalVector((bool[])p));
            SetupDotNetToRConverter(typeof(List<bool>), p => engine.CreateLogicalVector((IEnumerable<bool>)p));
            SetupDotNetToRConverter(typeof(IList<bool>), p => engine.CreateLogicalVector((IEnumerable<bool>)p));
            SetupDotNetToRConverter(typeof(ICollection<bool>), p => engine.CreateLogicalVector((IEnumerable<bool>)p));
            SetupDotNetToRConverter(typeof(IEnumerable<bool>), p => engine.CreateLogicalVector((IEnumerable<bool>)p));
            SetupDotNetToRConverter(typeof(bool[,]), p => engine.CreateLogicalMatrix((bool[,])p));

            SetupDotNetToRConverter(typeof(double), p => engine.CreateNumeric((double)p));
            SetupDotNetToRConverter(typeof(double[]), p => engine.CreateNumericVector((double[])p));
            SetupDotNetToRConverter(typeof(List<double>), p => engine.CreateNumericVector((IEnumerable<double>)p));
            SetupDotNetToRConverter(typeof(IList<double>), p => engine.CreateNumericVector((IEnumerable<double>)p));
            SetupDotNetToRConverter(typeof(ICollection<double>), p => engine.CreateNumericVector((IEnumerable<double>)p));
            SetupDotNetToRConverter(typeof(IEnumerable<double>), p => engine.CreateNumericVector((IEnumerable<double>)p));
            SetupDotNetToRConverter(typeof(double[,]), p => engine.CreateNumericMatrix((double[,])p));

            SetupDotNetToRConverter(typeof(DateTime), p => engine.CreatePosixct((DateTime)p));
            SetupDotNetToRConverter(typeof(DateTime[]), p => engine.CreatePosixctVector((DateTime[])p));
            SetupDotNetToRConverter(typeof(List<DateTime>), p => engine.CreatePosixctVector((IEnumerable<DateTime>)p));
            SetupDotNetToRConverter(typeof(IList<DateTime>), p => engine.CreatePosixctVector((IEnumerable<DateTime>)p));
            SetupDotNetToRConverter(typeof(ICollection<DateTime>), p => engine.CreatePosixctVector((IEnumerable<DateTime>)p));
            SetupDotNetToRConverter(typeof(IEnumerable<DateTime>), p => engine.CreatePosixctVector((IEnumerable<DateTime>)p));
            SetupDotNetToRConverter(typeof(DateTime[,]), p => engine.CreatePosixctMatrix((DateTime[,])p));

            SetupDotNetToRConverter(typeof(TimeSpan), p => engine.CreateDiffTime((TimeSpan)p));
            SetupDotNetToRConverter(typeof(TimeSpan[]), p => engine.CreateDiffTimeVector((TimeSpan[])p));
            SetupDotNetToRConverter(typeof(List<TimeSpan>), p => engine.CreateDiffTimeVector((IEnumerable<TimeSpan>)p));
            SetupDotNetToRConverter(typeof(IList<TimeSpan>), p => engine.CreateDiffTimeVector((IEnumerable<TimeSpan>)p));
            SetupDotNetToRConverter(typeof(ICollection<TimeSpan>), p => engine.CreateDiffTimeVector((IEnumerable<TimeSpan>)p));
            SetupDotNetToRConverter(typeof(IEnumerable<TimeSpan>), p => engine.CreateDiffTimeVector((IEnumerable<TimeSpan>)p));
            SetupDotNetToRConverter(typeof(TimeSpan[,]), p => engine.CreateDiffTimeMatrix((TimeSpan[,])p));
        }

        public void SetupDotNetToRConverter(Type type, Func<object, SymbolicExpression> converter)
        {
            if (type == null) return;

            if (_logger.IsDebugEnabled)
                _logger.DebugFormat(
                    _convertersBack.ContainsKey(type)
                        ? "Override converter C# -> R, Type: {0}"
                        : "Setup converter C# -> R, Type: {0}", type);

            _convertersBack[type] = converter;
        }

        public bool RemoveToRConverter(Type type)
        {
            if (type == null) return false;

            if (_logger.IsDebugEnabled && _convertersBack.ContainsKey(type))
                _logger.DebugFormat("Remove converter C# -> R, Type: {0}", type);

            return _convertersBack.Remove(type);
        }

        public Func<object, SymbolicExpression> GetToRConverter(Type type)
        {
            if (type == null) return null;

            _convertersBack.TryGetValue(type, out var converter);
            return converter;
        }

        #endregion
    }
}
