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
    }
}
