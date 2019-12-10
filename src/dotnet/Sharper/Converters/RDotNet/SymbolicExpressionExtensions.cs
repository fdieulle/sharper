using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RDotNet;
using Sharper.Converters.Resources;

namespace Sharper.Converters.RDotNet
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr R_MakeExternalPtr(IntPtr args, IntPtr tag, IntPtr prot);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr R_ExternalPtrTag(IntPtr args);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr R_ExternalPtrAddr(IntPtr args);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void R_CFinalizer_t(IntPtr s);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void R_RegisterCFinalizerEx(IntPtr s, R_CFinalizer_t fun, bool onExit);

    public static class SymbolicExpressionExtensions
    {
        public static SymbolicExpression ToExternalPointer(this REngine engine, object instance) 
            => ExternalPtrConverter.ToExternalPointer(engine, instance);

        #region Faster than RDotNet extensions

        /// <summary>
        /// Creates a new NumericVector with the specified values.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="vector">The values.</param>
        /// <returns>The new vector.</returns>
        public static SymbolicExpression CreateNumericVector(this REngine engine, double[] vector)
        {
            if (engine == null)
                throw new ArgumentNullException();

            if (!engine.IsRunning)
                throw new ArgumentException();

            return new NumericVector(engine, vector);
        }

        /// <summary>
        /// Creates a new IntegerVector with the specified values.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="vector">The values.</param>
        /// <returns>The new vector.</returns>
        public static IntegerVector CreateIntegerVector(this REngine engine, int[] vector)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (!engine.IsRunning)
                throw new ArgumentException("engine");

            return new IntegerVector(engine, vector);
        }

        #endregion

        #region DateTime

        public static bool IsPosixct(this SymbolicExpression sexp)
        {
            return sexp.GetAttributeNames().Any(p => string.Equals("class", p))
                && sexp.GetAttribute("class").AsCharacter().ToArray().Any(p => string.Equals("POSIXct", p));
        }

        public static bool IsPosixlt(this SymbolicExpression sexp)
        {
            return sexp.GetAttributeNames().Any(p => string.Equals("class", p))
                && sexp.GetAttribute("class").AsCharacter().ToArray().Any(p => string.Equals("POSIXlt", p));
        }

        public static TimeZoneInfo GetWindowsTimezone(this SymbolicExpression sexp)
        {
            if (sexp.GetAttributeNames().Any(p => string.Equals("tzone", p)))
            {
                var tzone = sexp.GetAttribute("tzone").AsCharacter().ToArray().FirstOrDefault();
                return tzone.GetWindowsTimezone();
            }

            return TimeZoneInfo.Local;
        }

        public static SymbolicExpression CreatePosixct(this REngine engine, DateTime value)
        {
            return engine.CreatePosixctVector(new[] { value });
        }

        public static SymbolicExpression CreatePosixctVector(this REngine engine, DateTime[] data)
        {
            var numeric = data.ToTicks(out var tzone);
            var sexp = engine.CreateNumericVector(numeric);
            return sexp.AddPosixctAttributes(tzone);
        }

        public static SymbolicExpression CreatePosixctVector(this REngine engine, IEnumerable<DateTime> data)
        {
            var numeric = data.ToArray().ToTicks(out var tzone);
            var sexp = engine.CreateNumericVector(numeric);
            return sexp.AddPosixctAttributes(tzone);
        }

        public static SymbolicExpression CreatePosixctMatrix(this REngine engine, DateTime[,] data)
        {
            var numeric = data.ToTicks(out var tzone);
            var sexp = engine.CreateNumericMatrix(numeric);
            return sexp.AddPosixctAttributes(tzone);
        }

        public static SymbolicExpression AddPosixctAttributes(this SymbolicExpression sexp, IEnumerable<string> tzone)
        {
            sexp.SetAttribute("class", sexp.Engine.CreateCharacterVector(new[] { "POSIXct", "POSIXt" }));
            sexp.SetAttribute("tzone", sexp.Engine.CreateCharacterVector(tzone));
            return sexp;
        }

        #endregion

        #region TimeSpan

        public static bool IsDiffTime(this SymbolicExpression sexp)
        {
            return sexp.GetAttributeNames().Any(p => string.Equals("class", p))
                && sexp.GetAttribute("class").AsCharacter().ToArray().Any(p => string.Equals("difftime", p));
        }

        private const string SECS = "secs";
        private const string MINS = "mins";
        private const string HOURS = "hours";
        private const string DAYS = "days";
        private const string WEEKS = "weeks";

        public static string GetUnits(this SymbolicExpression sexp)
        {
            string units = null;
            if (sexp.GetAttributeNames().Any(p => string.Equals("units", p)))
                units = sexp.GetAttribute("units").AsCharacter().FirstOrDefault();

            switch (units)
            {
                case WEEKS:
                    return WEEKS;
                case DAYS:
                    return DAYS;
                case HOURS:
                    return HOURS;
                case MINS:
                    return MINS;
                default:
                    return SECS;
            }
        }

        public static SymbolicExpression CreateDiffTime(this REngine engine, TimeSpan data)
        {
            return engine.CreateDiffTimeVector(new[] { data });
        }

        public static SymbolicExpression CreateDiffTimeVector(this REngine engine, TimeSpan[] data)
        {
            var numeric = data.FromTimeSpan();
            var sexp = engine.CreateNumericVector(numeric);
            return sexp.AddDiffTimeAttributes();
        }

        public static SymbolicExpression CreateDiffTimeVector(this REngine engine, IEnumerable<TimeSpan> data)
        {
            var numeric = data.ToArray().FromTimeSpan();
            var sexp = engine.CreateNumericVector(numeric);
            return sexp.AddDiffTimeAttributes();
        }

        public static SymbolicExpression CreateDiffTimeMatrix(this REngine engine, TimeSpan[,] data)
        {
            var numeric = data.FromTimeSpan();
            var sexp = engine.CreateNumericMatrix(numeric);
            return sexp.AddDiffTimeAttributes();
        }

        public static SymbolicExpression AddDiffTimeAttributes(this SymbolicExpression sexp, string units = SECS)
        {
            sexp.SetAttribute("class", sexp.Engine.CreateCharacterVector(new[] { "difftime" }));
            sexp.SetAttribute("units", sexp.Engine.CreateCharacterVector(new[] { units }));
            return sexp;
        }

        private const double TICKS_PER_MILLISECOND = 1e4;
        private const double TICKS_PER_SECOND = TICKS_PER_MILLISECOND * 1e3;
        private const double TICKS_PER_MINUTE = TICKS_PER_SECOND * 60;
        private const double TICKS_PER_HOUR = TICKS_PER_MINUTE * 60;
        private const double TICKS_PER_DAY = TICKS_PER_HOUR * 24;
        private const double TICKS_PER_WEEK = TICKS_PER_DAY * 7;

        public static TimeSpan ToTimeSpan(this double value, string units)
        {
            switch (units)
            {
                case WEEKS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_WEEK));
                case DAYS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_DAY));
                case HOURS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_HOUR));
                case MINS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_MINUTE));
                default:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_SECOND));
            }
        }

        public static TimeSpan[] ToTimeSpan(this double[] values, string units)
        {
            var result = new TimeSpan[values.Length];
            switch (units)
            {
                case WEEKS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_WEEK));
                    return result;
                case DAYS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_DAY));
                    return result;
                case HOURS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_HOUR));
                    return result;
                case MINS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_MINUTE));
                    return result;
                default:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_SECOND));
                    return result;
            }
        }

        public static TimeSpan[,] ToTimeSpan(this double[,] values, string units)
        {
            var nbRow = values.GetLength(0);
            var nbCol = values.GetLength(1);

            var result = new TimeSpan[nbRow, nbCol];
            switch (units)
            {
                case WEEKS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_WEEK));
                    return result;
                case DAYS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_DAY));
                    return result;
                case HOURS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_HOUR));
                    return result;
                case MINS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_MINUTE));
                    return result;
                default:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_SECOND));
                    return result;
            }
        }

        public static TimeSpan ToTimeSpan(this int value, string units)
        {
            switch (units)
            {
                case WEEKS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_WEEK));
                case DAYS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_DAY));
                case HOURS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_HOUR));
                case MINS:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_MINUTE));
                default:
                    return TimeSpan.FromTicks((long)(value * TICKS_PER_SECOND));
            }
        }

        public static TimeSpan[] ToTimeSpan(this int[] values, string units)
        {
            var result = new TimeSpan[values.Length];
            switch (units)
            {
                case WEEKS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_WEEK));
                    return result;
                case DAYS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_DAY));
                    return result;
                case HOURS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_HOUR));
                    return result;
                case MINS:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_MINUTE));
                    return result;
                default:
                    for (var i = 0; i < values.Length; i++)
                        result[i] = TimeSpan.FromTicks((long)(values[i] * TICKS_PER_SECOND));
                    return result;
            }
        }

        public static TimeSpan[,] ToTimeSpan(this int[,] values, string units)
        {
            var nbRow = values.GetLength(0);
            var nbCol = values.GetLength(1);

            var result = new TimeSpan[nbRow, nbCol];
            switch (units)
            {
                case WEEKS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_WEEK));
                    return result;
                case DAYS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_DAY));
                    return result;
                case HOURS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_HOUR));
                    return result;
                case MINS:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_MINUTE));
                    return result;
                default:
                    for (var i = 0; i < nbRow; i++)
                        for (var j = 0; j < nbCol; j++)
                            result[i, j] = TimeSpan.FromTicks((long)(values[i, j] * TICKS_PER_SECOND));
                    return result;
            }
        }

        public static double[] FromTimeSpan(this TimeSpan[] timespans)
        {
            var result = new double[timespans.Length];
            for (var i = 0; i < timespans.Length; i++)
                result[i] = timespans[i].TotalSeconds;
            return result;
        }

        public static double[,] FromTimeSpan(this TimeSpan[,] timespans)
        {
            var nbRow = timespans.GetLength(0);
            var nbCol = timespans.GetLength(1);

            var result = new double[nbRow, nbCol];
            for (var i = 0; i < nbRow; i++)
                for (var j = 0; j < nbCol; j++)
                    result[i, j] = timespans[i, j].TotalSeconds;
            return result;
        }

        #endregion
    }
}
