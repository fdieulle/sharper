using System;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using RDotNet;
using Sharper.Converters;
using Sharper.Converters.RDotNet;

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

            ClrProxy.CallStaticMethod(typeName, methodName, null, 0, out _, out _);
            
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
        public void TestCallStaticMethod()
        {
            var engine = REngine.GetInstance();
            ClrProxy.LoadAssembly(PATH);

            const string typeName = "AssemblyForTests.StaticClass";
            const string methodName = "SameMethodName";

            ClrProxy.CallStaticMethod(typeName, methodName, null, 0, out _, out _);

            SymbolicExpression sexp = engine.CreateInteger(1);
            var arguments = new[] { (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            sexp = engine.CreateNumeric(1.0);
            arguments = new[] { (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            sexp = REngineExtension.CreateIntegerVector(engine, new[] {1, 2, 3});
            arguments = new[] { (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            sexp = REngineExtension.CreateNumericVector(engine, new[] { 1.0, 2.0, 3.0 });
            arguments = new[] { (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            sexp = engine.CreateNumeric(1.0); 
            SymbolicExpression sexp2 = engine.CreateInteger(1);
            arguments = new[] { (long)sexp.DangerousGetHandle(), (long)sexp2.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            sexp = REngineExtension.CreateNumericVector(engine, new[] { 1.0, 2.0, 3.0 }); 
            sexp2 = REngineExtension.CreateIntegerVector(engine, new[] { 1, 2, 3 });
            arguments = new[] { (long)sexp.DangerousGetHandle(), (long)sexp2.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            sexp = engine.CreateInteger(1);
            sexp2 = REngineExtension.CreateNumericVector(engine, new[] { 1.0, 2.0, 3.0 });
            arguments = new[] { (long)sexp2.DangerousGetHandle(), (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            sexp = REngineExtension.CreateIntegerVector(engine, new[] { 1, 2, 3 });
            sexp2 = engine.CreateNumeric(1.0);
            arguments = new[] { (long)sexp2.DangerousGetHandle(), (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);

            var externalPtr = ClrProxy.CreateObject("AssemblyForTests.DefaultCtorData", null, 0);
            arguments = new[] {externalPtr};
            ClrProxy.CallStaticMethod(typeName, methodName, arguments, arguments.Length, out _, out _);
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

        [Test]
        public void TestCallMethodWithOutArgument()
        {
            const string typeName = "AssemblyForTests.StaticClass";

            var engine = REngine.GetInstance();
            ClrProxy.LoadAssembly(PATH);

            var sexp = engine.CreateNumeric(0.0);
            var arguments = new[] { (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, "TryGetValue", arguments, arguments.Length, out var results, out var resultsSize);
            Assert.AreEqual(2, resultsSize);
            Assert.IsTrue(engine.CreateFromNativeSexp(new IntPtr(results[0])).AsLogical()[0]);
            Assert.AreEqual(12.4, engine.CreateFromNativeSexp(new IntPtr(results[1])).AsNumeric()[0]);

            var ptr = ClrProxy.CreateObject("AssemblyForTests.DefaultCtorData", null, 0);
            arguments = new[] { ptr };
            ClrProxy.CallStaticMethod(typeName, "TryGetObject", arguments, arguments.Length, out results, out resultsSize);
            Assert.AreEqual(2, resultsSize);
            Assert.IsTrue(engine.CreateFromNativeSexp(new IntPtr(results[0])).AsLogical()[0]);

            var namePtr = ClrProxy.GetProperty(results[1], "Name");
            Assert.AreEqual("Out object", engine.CreateFromNativeSexp(new IntPtr(namePtr)).AsCharacter()[0]);
        }

        [Test]
        public void TestCallMethodWithRefArgument()
        {
            const string typeName = "AssemblyForTests.StaticClass";

            var engine = REngine.GetInstance();
            ClrProxy.LoadAssembly(PATH);

            var sexp = engine.CreateNumeric(1.0);
            var arguments = new[] { (long)sexp.DangerousGetHandle() };
            ClrProxy.CallStaticMethod(typeName, "UpdateValue", arguments, arguments.Length, out var results, out var resultsSize);
            Assert.AreEqual(2, resultsSize);
            Assert.AreEqual(engine.NilValue, engine.CreateFromNativeSexp(new IntPtr(results[0])));
            Assert.AreEqual(2, engine.CreateFromNativeSexp(new IntPtr(results[1])).AsNumeric()[0]);

            var ptr = ClrProxy.CreateObject("AssemblyForTests.DefaultCtorData", null, 0);
            arguments = new[] { ptr };
            ClrProxy.CallStaticMethod(typeName, "UpdateObject", arguments, arguments.Length, out results, out resultsSize);
            Assert.AreEqual(2, resultsSize);
            Assert.AreEqual(engine.NilValue, engine.CreateFromNativeSexp(new IntPtr(results[0])));
            var namePtr = ClrProxy.GetProperty(results[1], "Name");
            Assert.AreEqual("Ref object", engine.CreateFromNativeSexp(new IntPtr(namePtr)).AsCharacter()[0]);
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
