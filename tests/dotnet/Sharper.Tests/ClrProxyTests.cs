using System;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using Sharper.Converters;

namespace Sharper.Tests
{
    [TestFixture]
    public class ClrProxyTests
    {
#if DEBUG
        private const string PATH = @"..\..\..\..\AssemblyForTests\bin\Debug\netstandard2.0\AssemblyForTests.dll";
#elif RELEASE
        private const string PATH = @"..\..\..\..\AssemblyForTests\bin\Release\netstandard2.0\AssemblyForTests.dll";
#endif

        [Test]
        public void TestGetMethod()
        {
            ClrProxy.DataConverter = Substitute.For<IDataConverter>();
            Assert.IsNotNull(ClrProxy.LoadAssembly(PATH));

            var typeName = "AssemblyForTests.StaticClass";
            var methodName = "SameMethodName";
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

            ClrProxy.CallStaticMethod(typeName, methodName, null, 0);
            
            Assert.IsTrue(typeName.TryGetType(out var type, out var errorMsg));
            
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<int>()), out var method1));
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<double>()), out var method2));
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<int[]>()), out var method3));
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<double[]>()), out var method4));
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<double>(), C<int>()), out var method5));
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<double[]>(), C<int[]>()), out var method6));
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<double[]>(), C<int>()), out var method7));
            Assert.IsTrue(type.TryGetMethod(methodName, flags, A(C<double>(), C<int[]>()), out var method8));
        }

        private static IConverter C(Type type)
        {
            var converter = Substitute.For<IConverter>();
            converter.GetClrTypes().Returns(new[] {type});
            return converter;
        }

        private static IConverter C<T>() => C(typeof(T));

        private static T[] A<T>(params T[] array) => array;
    }
}
