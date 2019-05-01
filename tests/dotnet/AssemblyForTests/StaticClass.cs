using System;

namespace AssemblyForTests
{
    public static class StaticClass
    {
        #region Same method name

        public static void SameMethodName()
        {
            Console.WriteLine("SameMethodName() Call");
        }

        public static void SameMethodName(int intValue)
        {
            Console.WriteLine("SameMethodName(int) arg: {0}", intValue);
        }

        public static void SameMethodName(double doubleValue)
        {
            Console.WriteLine("SameMethodName(double) arg: {0}", doubleValue);
        }

        public static void SameMethodName(int[] intValue)
        {
            Console.WriteLine("SameMethodName(int[]) arg: {0}", string.Join(";", intValue));
        }

        public static void SameMethodName(double[] doubleValue)
        {
            Console.WriteLine("SameMethodName(double[]) arg: {0}", string.Join(";", doubleValue));
        }

        public static void SameMethodName(double doubleValue, int intValue)
        {
            Console.WriteLine("SameMethodName(double, int) arg1: {0}, arg2: {1}", doubleValue, intValue);
        }

        public static void SameMethodName(double[] doubleValue, int[] intValue)
        {
            Console.WriteLine("SameMethodName(double[], int[]) arg1: {0}, arg2: {1}", string.Join(";", doubleValue), string.Join(";", intValue));
        }

        public static void SameMethodName(double[] doubleValue, int intValue)
        {
            Console.WriteLine("SameMethodName(double[], int) arg1: {0}, arg2: {1}", string.Join(";", doubleValue), intValue);
        }

        public static void SameMethodName(double doubleValue, int[] intValue)
        {
            Console.WriteLine("SameMethodName(double, int[]) arg1: {0}, arg2: {1}", doubleValue, string.Join(";", intValue));
        }

        #endregion

        #region Returns Native types

        public static int ReturnsNativeType(int x) => x;
        public static int[] ReturnsNativeType(int[] x) => x;
        public static int[,] ReturnsNativeType(int[,] x) => x;

        public static double ReturnsNativeType(double x) => x;
        public static double[] ReturnsNativeType(double[] x) => x;
        public static double[,] ReturnsNativeType(double[,] x) => x;

        public static bool ReturnsNativeType(bool x) => x;
        public static bool[] ReturnsNativeType(bool[] x) => x;
        public static bool[,] ReturnsNativeType(bool[,] x) => x;

        public static string ReturnsNativeType(string x) => x;
        public static string[] ReturnsNativeType(string[] x) => x;
        public static string[,] ReturnsNativeType(string[,] x) => x;

        public static DateTime ReturnsNativeType(DateTime x) => x;
        public static DateTime[] ReturnsNativeType(DateTime[] x) => x;
        public static DateTime[,] ReturnsNativeType(DateTime[,] x) => x;

        public static TimeSpan ReturnsNativeType(TimeSpan x) => x;
        public static TimeSpan[] ReturnsNativeType(TimeSpan[] x) => x;
        public static TimeSpan[,] ReturnsNativeType(TimeSpan[,] x) => x;

        #endregion

        #region Properties

        public static double DoubleProperty { get; set; } = 12.3;

        public static double[] DoubleArrayProperty { get; set; } = { 12.3, 13.4, 14.5 };

        public static int Int32Property { get; set; } = 13;

        public static int[] Int32ArrayProperty { get; set; } = { 12, 13, 14 };

        #endregion
    }
}
