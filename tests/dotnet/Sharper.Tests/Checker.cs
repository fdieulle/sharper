using NUnit.Framework;

namespace Sharper.Tests
{
    public static class Checker
    {
        public static void CheckIsTrue(this bool value, string message = null)
            => Assert.IsTrue(value, message);

        public static T CheckIsNull<T>(this T value, string message = null)
        {
            Assert.IsNull(value, message);
            return default(T);
        }

        public static T CheckIsNotNull<T>(this T value, string message = null)
        {
            Assert.IsNotNull(value, message);
            return value;
        }
    }
}
