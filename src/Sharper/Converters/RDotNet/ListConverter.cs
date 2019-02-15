using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RDotNet;

namespace Sharper.Converters.RDotNet
{
    public class ListConverter : IConverter
    {
        private static readonly Type[] keyTypes = new[] { typeof(string) };
        private static readonly Type[] defaultItemType = new[] { typeof(object) };
        private static readonly HashSet<Type> listTypeDefinitions;
        private static readonly HashSet<Type> dictionaryTypeDefinitions;

        static ListConverter()
        {
            var list = typeof(List<object>)
                .GetFullHierarchy()
                .Where(p => p.IsGenericType)
                .Select(p => p.GetGenericTypeDefinition())
                .ToArray();

            var dictionary = typeof(Dictionary<string, object>).GetFullHierarchy()
                .Where(p => p.IsGenericType)
                .Select(p => p.GetGenericTypeDefinition())
                .ToArray();

            listTypeDefinitions = new HashSet<Type>(list);
            dictionaryTypeDefinitions = new HashSet<Type>(dictionary);

            var intersect = dictionary.Intersect(list);
            foreach (var type in intersect)
                dictionaryTypeDefinitions.Remove(type);
        }

        private readonly int _length;
        private readonly IConverter[] _converters;
        private readonly string[] _names;
        private readonly Type[] _types;
        private readonly Type[] _intersectedItemType;

        public ListConverter(GenericVector sexp, IDataConverter converter)
        {
            var array = sexp.ToArray();
            _length = sexp.Length;

            _intersectedItemType = null;
            _converters = new IConverter[_length];
            for (var i = 0; i < _length; i++)
            {
                _converters[i] = converter.GetConverter(array[i].DangerousGetHandle().ToInt64());
                if (_converters[i] == null)
                    throw new InvalidDataException("Unable to get convert for data at index: " + i + " in List");

                var itemTypes = _converters[i].GetClrTypes();
                _intersectedItemType = _intersectedItemType == null
                    ? itemTypes
                    : _intersectedItemType.Intersect(itemTypes);
            }

            if (_intersectedItemType == null)
                _intersectedItemType = new[] { typeof(object) };

            var fullTypes = new List<Type>();
            _names = sexp.Names;
            if (_names != null)
            {
                fullTypes.AddRange(keyTypes.GetDictionaryTypes(_intersectedItemType));
                if (_names.Length != _length)
                {
                    var swap = new string[_length];
                    for (var i = 0; i < _length; i++)
                    {
                        swap[i] = i < _names.Length
                            ? _names[i]
                            : "Column " + (i + 1);
                    }
                    _names = swap;
                }
            }

            fullTypes.AddRange(_intersectedItemType.GetListOrArrayTypes());

            var count = fullTypes.Count;
            _types = fullTypes[0].GetFullHierarchy();
            for (var i = 1; i < count; i++)
                _types = _types.Union(fullTypes[i].GetFullHierarchy());
        }

        #region Implementation of IConverter

        public Type[] GetClrTypes()
        {
            return _types;
        }

        public object Convert(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (listTypeDefinitions.Contains(genericTypeDefinition))
                {
                    var genericType = type.GetGenericArguments()[0];
                    var list = (IList)Activator.CreateInstance(type.IsInterface
                        ? typeof(List<>).MakeGenericType(genericType)
                        : type);
                    for (var i = 0; i < _length; i++)
                        list.Add(_converters[i].Convert(genericType));
                    return list;
                }

                if (dictionaryTypeDefinitions.Contains(genericTypeDefinition))
                {
                    var genericType = type.GetGenericArguments()[1];
                    var dico = (IDictionary)Activator.CreateInstance(type.IsInterface
                        ? typeof(Dictionary<,>).MakeGenericType(typeof(string), genericType)
                        : type);
                    for (var i = 0; i < _length; i++)
                        dico.Add(_names[i], _converters[i].Convert(genericType));
                    return dico;
                }
            }

            if (type.IsArray || type == typeof(Array))
            {
                var genericType = type.GetElementType() ?? _intersectedItemType[0];
                var array = Array.CreateInstance(genericType, _length);
                for (var i = 0; i < _length; i++)
                    array.SetValue(_converters[i].Convert(genericType), i);
                return array;
            }

            var defaultList = new List<object>(_length);
            for (var i = 0; i < _length; i++)
            {
                var itemTypes = _converters[i].GetClrTypes() ?? defaultItemType;
                defaultList.Add(_converters[i].Convert(itemTypes.Length == 0 ? defaultItemType[0] : itemTypes[0]));
            }
            return defaultList;
        }

        #endregion

        public static bool TryConvertBack(REngine engine, RDotNetConverter dataConverter, object data, out SymbolicExpression result)
        {
            var type = data.GetType();
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (dictionaryTypeDefinitions.Contains(genericTypeDefinition))
                {
                    var genericArguments = type.GetGenericArguments();
                    if (genericArguments[0] == typeof(string))
                    {
                        if (dicoToSexpMethod.MakeGenericMethod(new[] { genericArguments[1] })
                            .Invoke(null, new[] { engine, dataConverter, data }) is SymbolicExpression sexp)
                        {
                            result = sexp;
                            return true;
                        }
                    }
                }

                if (listTypeDefinitions.Contains(genericTypeDefinition))
                {
                    if (data is IEnumerable enumerable)
                    {
                        result = new GenericVector(engine, ListToSexp(engine, dataConverter, enumerable));
                        return true;
                    }
                }
            }

            if (type.IsArray)
            {
                if (data is IEnumerable array)
                {
                    result = new GenericVector(engine, ListToSexp(engine, dataConverter, array));
                    return true;
                }
            }

            result = engine.NilValue;
            return false;
        }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable once PossibleNullReferenceException
        private static readonly MethodInfo dicoToSexpMethod = MethodBase.GetCurrentMethod().DeclaringType
            .GetMethod("DicoToSexp", BindingFlags.NonPublic | BindingFlags.Static);
        private static SymbolicExpression DicoToSexp<T>(REngine engine, RDotNetConverter dataConverter, IEnumerable<KeyValuePair<string, T>> enumerable)
        // ReSharper restore UnusedMember.Local
        {
            var type = typeof(T);
            var array = enumerable.ToArray();
            var length = array.Length;

            var values = new SymbolicExpression[length];
            var names = new string[length];
            for (var i = 0; i < length; i++)
            {
                names[i] = array[i].Key;
                var sexp = dataConverter.ConvertToSexp(type, array[i].Value);
                values[i] = sexp ?? engine.NilValue;
            }

            var vector = new GenericVector(engine, values);
            vector.SetNames(names);
            return vector;
        }

        private static IEnumerable<SymbolicExpression> ListToSexp(REngine engine, RDotNetConverter dataConverter, IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item == null)
                    yield return engine.NilValue;
                else
                {
                    var sexp = dataConverter.ConvertToSexp(item.GetType(), item);
                    if (sexp == null)
                        yield return engine.NilValue;
                    else yield return sexp;
                }
            }
        }
    }
}
