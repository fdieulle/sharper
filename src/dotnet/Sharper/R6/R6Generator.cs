using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sharper.Converters;

namespace Sharper.R6
{
    public static class R6Generator
    {
        private static readonly string location =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "R6", "Templates");

        private static readonly string r6ClassTemplate = File.ReadAllText(Path.Combine(location, "R6Class-Template.R"));
        private static readonly string r6PropertyTemplate = File.ReadAllText(Path.Combine(location, "R6Property-Template.R"));
        private static readonly string r6CtorTemplate = File.ReadAllText(Path.Combine(location, "R6Ctor-Template.R"));
        private static readonly string r6MethodTemplate = File.ReadAllText(Path.Combine(location, "R6Method-Template.R"));

        public static void GenerateR6Classes(
            string[] typeNames, 
            string file,
            IDataConverter converter,
            bool appendFile = false,
            bool withInheritedTypes = false)
        {
            if (typeNames == null || string.IsNullOrEmpty(file)) return;

            var types = typeNames.Select(t => t.AsType())
                .Where(t => t != null);

            if (withInheritedTypes)
                types = types.GetInheritedTypes();

            types = types.AddReferencedTypes(converter);

            var stack = new Stack<Type>();
            var allTypes = new HashSet<Type>();
            foreach (var type in types)
            {
                stack.Push(type);
                allTypes.Add(type);
            }

            var mode = appendFile ? FileMode.Append : FileMode.Create;
            var isNewFile = !File.Exists(file);
            using (var writer = new StreamWriter(new FileStream(file, mode, FileAccess.Write, FileShare.Read)))
            {
                if (isNewFile)
                {
                    writer.WriteLine("require(R6)");
                    writer.WriteLine("require(sharper)");
                    writer.WriteLine();
                }
                while (stack.Count > 0)
                {
                    var type = stack.Pop();
                    writer.WriteLine(type.GenerateR6Class(allTypes));
                }
            }
        }

        private static string GenerateR6Class(this Type type, HashSet<Type> allTypes)
        {
            var properties = new List<string>();
            foreach (var property in type.GetPublicProperties())
                properties.Add(r6PropertyTemplate
                    .Replace("{{PropertyName}}", property.Name)
                    .Replace("{{Description}}", property.GetDescription()));

            var ctorParameters = type.GetCtorParameters();
            var ctor = r6CtorTemplate
                .Replace("{{Parameters}}", ctorParameters.GenerateR6Parameters())
                .Replace("{{Comma}}", ctorParameters.GenerateComma())
                .Replace("{{FullTypeName}}", type.FullName)
                .Replace("{{CanBeInstantiated}}", (!type.IsAbstract && !type.IsInterface).ToString().ToUpper())
                .Replace("{{Description}}", type.GetDescription())
                .Replace("{{Parameters_doc}}", ctorParameters.GenerateR6ParametersDoc())
                .Replace("{{Return_doc}}", $"A new instance of {type.Name}");

            var methods = new List<string>();
            foreach (var method in type.GetPublicMethods())
            {
                var methodParameters = method.GetParameters();
                methods.Add(r6MethodTemplate
                    .Replace("{{MethodName}}", method.Name)
                    .Replace("{{Parameters}}", methodParameters.GenerateR6Parameters())
                    .Replace("{{Comma}}", methodParameters.GenerateComma())
                    .Replace("{{Return}}", method.ReturnType == typeof(void) ? "invisible" : "return")
                    .Replace("{{Description}}", type.GetDescription())
                    .Replace("{{Parameters_doc}}", ctorParameters.GenerateR6ParametersDoc())
                    .Replace("{{Return_doc}}", method.ReturnType == typeof(void) ? "nothing" : "the result"));
            }

            var r6Class = r6ClassTemplate
                .Replace("{{TypeName}}", type.Name)
                .Replace("{{InheritTypeName}}", allTypes.Contains(type.BaseType) ? type.BaseType.Name : "NetObject")
                .Replace("{{Properties}}", string.Join($",{Environment.NewLine}", properties))
                .Replace("{{Ctor}}", ctor)
                .Replace("{{Comma}}", methods.GenerateComma())
                .Replace("{{Methods}}", string.Join($",{Environment.NewLine}", methods))
                .Replace("{{Description}}", type.GetDescription());
            return r6Class;
        }

        private static string GenerateR6Parameters(this ParameterInfo[] parameters) 
            => parameters == null || parameters.Length == 0 
                ? string.Empty 
                : string.Join(", ", parameters.Select(p => p.Name));

        private static string GenerateComma<T>(this IEnumerable<T> source) 
            => source.Any() ? "," : string.Empty;

        private static string GenerateR6ParametersDoc(this ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var parameter in parameters)
            {
                sb.AppendLine();
                sb.AppendFormat("#' @param {0} ", parameter.Name);
            }

            return sb.ToString();
        }

        #region Search and Collect types

        private static IEnumerable<Type> AddReferencedTypes(this IEnumerable<Type> types, IDataConverter converter)
        {
            var keepOrder = types.ToArray();
            var visited = new HashSet<Type>(keepOrder);
            foreach (var type in keepOrder)
            {
                yield return type;

                foreach (var referencedType in type.GetReferencedTypes(converter)
                    .Where(p => !visited.Contains(p)))
                {
                    visited.Add(referencedType);
                    yield return referencedType;
                }
            }
        }

        private static IEnumerable<Type> GetInheritedTypes(this IEnumerable<Type> types)
        {
            var visited = new HashSet<Type>();
            foreach (var type in types
                .Where(p => !visited.Contains(p)))
            {
                visited.Add(type);
                yield return type;
                
                foreach (var inheritedType in type.GetInheritedTypes()
                    .Where(p => !visited.Contains(p)))
                {
                    visited.Add(inheritedType);
                    yield return inheritedType;
                }
            }
        }

        private static IEnumerable<Type> GetInheritedTypes(this Type type)
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from candidate in assembly.GetTypes().Where(p => p.InheritsFrom(type))
                from underlyingType in candidate.GetHierarchyUntil(type)
                select underlyingType;
        }

        private static bool InheritsFrom(this Type type, Type fromType)
        {
            if (type.IsInterface) return false;

            if (fromType.IsInterface)
                return type.GetInterfaces().Any(p => p == fromType);
            
            while (type != null)
            {
                if (type == fromType) return true;
                type = type.BaseType;
            }

            return false;
        }

        private static IEnumerable<Type> GetHierarchyUntil(this Type type, Type untilType)
        {
            while (type != null)
            {
                if(type == untilType) yield break;
                if(type == typeof(object)) yield break;
                yield return type;

                type = type.BaseType;
            }
        }

        private static IEnumerable<Type> GetReferencedTypes(this Type type, IDataConverter converter)
        {
            foreach (var property in type.GetPublicProperties())
            {
                if (property.PropertyType.TryGetTypeToGenerate(converter, out var propertyType))
                    yield return propertyType;
            }

            foreach (var method in type.GetPublicMethods())
            {
                if (method.ReturnType.TryGetTypeToGenerate(converter, out var returnType))
                    yield return returnType;
                foreach (var parameter in method.GetParameters())
                {
                    if (parameter.ParameterType.TryGetTypeToGenerate(converter, out var parameterType))
                        yield return parameterType;
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.IsBrowsable() && !p.IsSpecialName);
        }

        private static IEnumerable<MethodInfo> GetPublicMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.IsSpecialName && p.IsBrowsable() && p.DeclaringType != typeof(object));
        }

        private static bool IsBrowsable(this MemberInfo member)
        {
            var attribute = member.GetCustomAttribute<BrowsableAttribute>(true);
            return attribute == null || attribute.Browsable;
        }

        private static bool TryGetTypeToGenerate(this Type type, IDataConverter converter, out Type t)
        {
            t = type;
            if (type.IsValueType
                || type == typeof(void)
                || type.IsEnum
                || type == typeof(object)
                || type == typeof(string)
                || !type.IsBrowsable()
                || (converter?.IsDefined(type) ?? false))
                return false;

            // Manage array
            if (type.IsArray)
            {
                t = type.GetElementType();
                return t.ShouldGenerateR6(converter);
            }

            // Manage IEnumerable<>
            if (type.GetInterfaces().Any(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                t = type.GetGenericArguments()[0];
                return t.ShouldGenerateR6(converter);
            }

            return !type.IsGenericType // Not yet supported
                   && !type.InheritsFrom(typeof(IEnumerable));
        }

        private static bool ShouldGenerateR6(this Type type, IDataConverter converter) 
            => type.TryGetTypeToGenerate(converter, out _);

        private static ParameterInfo[] GetCtorParameters(this Type type)
        {
            var ctors = type.GetConstructors();

            var candidate = (ctors.FirstOrDefault(p => p.GetCustomAttribute<R6CtorAttribute>(false) != null)
                ?? ctors.FirstOrDefault(p => p.GetParameters().Length == 0))
                ?? ctors.FirstOrDefault();

            return candidate != null 
                ? candidate.GetParameters() 
                : new ParameterInfo[0];
        }

        private static string GetDescription(this MemberInfo member)
        {
            var attribute = member.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null || string.IsNullOrEmpty(attribute.Description)
                ? member.Name : attribute.Description;
        }

        #endregion
    }
}
