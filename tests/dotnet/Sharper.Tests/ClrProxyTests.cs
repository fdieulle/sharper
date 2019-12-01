using System;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using RDotNet;
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
            ClrProxy.LoadAssembly(PATH);

            const string typeName = "AssemblyForTests.StaticClass";
            const string methodName = "SameMethodName";
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

            ClrProxy.CallStaticMethod(typeName, methodName, null, 0, out var results, out var resultsSize);
            
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

        [Test]
        public void TestCreateObject()
        {
            var engine = REngine.GetInstance();
            ClrProxy.LoadAssembly(PATH);

            var externalPtr = ClrProxy.CreateObject("AssemblyForTests.DefaultCtorData", null, 0);

            ClrProxy.CallMethod(externalPtr, "ToString", null, 0, out var results, out var resultsSize);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, resultsSize);
            Assert.AreEqual(1, results.Length);

            var sexp = engine.CreateFromNativeSexp(new IntPtr(results[0]));
            Assert.AreEqual("AssemblyForTests.DefaultCtorData", sexp.AsCharacter().ToArray()[0]);
        }

        [Test]
        public void TestGetAndSetProperty()
        {
            var engine = REngine.GetInstance();
            ClrProxy.LoadAssembly(PATH);

            var externalPtr = ClrProxy.CreateObject("AssemblyForTests.DefaultCtorData", null, 0);

            var sexp = engine.CreateCharacter("Test");
            ClrProxy.SetProperty(externalPtr, "Name", sexp.DangerousGetHandle().ToInt64());

            var result = ClrProxy.GetProperty(externalPtr, "Name");

            Assert.AreEqual("Test", engine.CreateFromNativeSexp(new IntPtr(result)).AsCharacter()[0]);
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
