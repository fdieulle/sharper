using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RDotNet;

namespace Sharper.Converters.RDotNet
{
    public class ExternalPtrConverter : IConverter, IDisposable
    {
        public const string NET_OBJ_TAG = ".NetObj";

        private readonly SymbolicExpression _sexp;
        private readonly Type[] _types;

        public ExternalPtrConverter(SymbolicExpression sexp)
        {
            _sexp = sexp;
            _types = ExtractTypes(sexp);
        }

        #region Implementation of IConverter

        public Type[] GetClrTypes() => _types;

        public object Convert(Type type)
        {
            var pointer = _sexp.Engine.GetFunction<R_ExternalPtrAddr>()(_sexp.DangerousGetHandle());
            return Marshal.GetObjectForIUnknown(pointer);
        }

        #endregion

        #region IDisposable

        public void Dispose() => Disposing(_sexp);

        private static void Disposing(SymbolicExpression sexp)
        {
            var objPtr = sexp.Engine.GetFunction<R_ExternalPtrAddr>()(sexp.DangerousGetHandle());
            Marshal.Release(objPtr);
        }

        #endregion

        public static SymbolicExpression ToExternalPointer(REngine engine, object instance)
        {
            if (instance == null) return engine.NilValue;

            var tag = engine.CreateCharacterVector(new[] { NET_OBJ_TAG, instance.GetType().FullName });
            var ptr = engine.GetFunction<R_MakeExternalPtr>()(
                Marshal.GetIUnknownForObject(instance),
                tag.DangerousGetHandle(),
                engine.NilValue.DangerousGetHandle());

            var sexp = engine.CreateFromNativeSexp(ptr);
            engine.GetFunction<R_RegisterCFinalizerEx>()(
                sexp.DangerousGetHandle(),
                p => Disposing(sexp), 
                true);

            return sexp;
        }

        public static Type[] ExtractTypes(SymbolicExpression sexp)
        {
            var tagPtr = sexp.Engine.GetFunction<R_ExternalPtrTag>()(sexp.DangerousGetHandle());
            var tag = sexp.Engine.CreateFromNativeSexp(tagPtr).AsCharacter().ToArray();

            if (tag == null || tag.Length != 2)
                throw new InvalidOperationException("This external pointer isn't supported");

            if (string.Equals(tag[0], NET_OBJ_TAG))
            {
                var typeAsString = tag[1];

                if (typeAsString.TryGetType(out var type, out var errorMessage))
                    return type.GetFullHierarchy();

                return new[] { typeof(object) };
            }

            throw new InvalidOperationException($"This external pointer isn't supported, the tag should starts with {NET_OBJ_TAG} value");
        }
    }
}
