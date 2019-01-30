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

            const string typeName = "AssemblyForTests.StaticClass";
            const string methodName = "SameMethodName";
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

            ClrProxy.CallStaticMethod(typeName, methodName, null, 0);
            
            typeName.TryGetType(out var type, out var errorMessage).CheckIsTrue();
            errorMessage.CheckIsNull();
            
            type.TryGetMethod(methodName, flags, A(C<int>()), out var method).CheckIsTrue();
            method.CheckIsNotNull();

            type.TryGetMethod(methodName, flags, A(C<double>()), out method).CheckIsTrue();
            method.CheckIsNotNull();

            type.TryGetMethod(methodName, flags, A(C<int[]>()), out method).CheckIsTrue();
            method.CheckIsNotNull();

            type.TryGetMethod(methodName, flags, A(C<double[]>()), out method).CheckIsTrue();
            method.CheckIsNotNull();

            type.TryGetMethod(methodName, flags, A(C<double>(), C<int>()), out method).CheckIsTrue();
            method.CheckIsNotNull();

            type.TryGetMethod(methodName, flags, A(C<double[]>(), C<int[]>()), out method).CheckIsTrue();
            method.CheckIsNotNull();

            type.TryGetMethod(methodName, flags, A(C<double[]>(), C<int>()), out method).CheckIsTrue();
            method.CheckIsNotNull();

            type.TryGetMethod(methodName, flags, A(C<double>(), C<int[]>()), out method).CheckIsTrue();
            method.CheckIsNotNull();
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
