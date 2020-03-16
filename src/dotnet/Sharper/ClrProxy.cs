using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Sharper.Converters;
using Sharper.Converters.RDotNet;
using Sharper.Loggers;
using Sharper.R6;

namespace Sharper
{
    public static class ClrProxy
    {
        private static readonly ILogger logger = new FileLogger(Assembly.GetExecutingAssembly().Location);

        static ClrProxy()
        {
            logger.InfoFormat("BaseDirectory: {0}", AppDomain.CurrentDomain.BaseDirectory);
            logger.InfoFormat("DynamicDirectory: {0}", AppDomain.CurrentDomain.DynamicDirectory);
            logger.InfoFormat("IsFullyTrusted: {0}", AppDomain.CurrentDomain.IsFullyTrusted);
            logger.InfoFormat("IsHomogenous: {0}", AppDomain.CurrentDomain.IsHomogenous);
            logger.InfoFormat("RelativeSearchPath: {0}", AppDomain.CurrentDomain.RelativeSearchPath);
            logger.InfoFormat("ShadowCopyFiles: {0}", AppDomain.CurrentDomain.ShadowCopyFiles);
            logger.InfoFormat("APPBASE: {0}", AppDomain.CurrentDomain.GetData("APPBASE"));
            logger.InfoFormat("APP_CONFIG_FILE: {0}", AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE"));
            logger.InfoFormat("APP_LAUNCH_URL: {0}", AppDomain.CurrentDomain.GetData("APP_LAUNCH_URL"));
            logger.InfoFormat("APP_NAME: {0}", AppDomain.CurrentDomain.GetData("APP_NAME"));
            logger.InfoFormat("BINPATH_PROBE_ONLY: {0}", AppDomain.CurrentDomain.GetData("BINPATH_PROBE_ONLY"));
            logger.InfoFormat("CACHE_BASE: {0}", AppDomain.CurrentDomain.GetData("CACHE_BASE"));
            logger.InfoFormat("CODE_DOWNLOAD_DISABLED: {0}", AppDomain.CurrentDomain.GetData("CODE_DOWNLOAD_DISABLED"));
            logger.InfoFormat("DEV_PATH: {0}", AppDomain.CurrentDomain.GetData("DEV_PATH"));
            logger.InfoFormat("DISALLOW_APP: {0}", AppDomain.CurrentDomain.GetData("DISALLOW_APP"));
            logger.InfoFormat("DISALLOW_APP_BASE_PROBING: {0}", AppDomain.CurrentDomain.GetData("DISALLOW_APP_BASE_PROBING"));
            logger.InfoFormat("DISALLOW_APP_REDIRECTS: {0}", AppDomain.CurrentDomain.GetData("DISALLOW_APP_REDIRECTS"));
            logger.InfoFormat("DYNAMIC_BASE: {0}", AppDomain.CurrentDomain.GetData("DYNAMIC_BASE"));
            logger.InfoFormat("FORCE_CACHE_INSTALL: {0}", AppDomain.CurrentDomain.GetData("FORCE_CACHE_INSTALL"));
            logger.InfoFormat("LICENSE_FILE: {0}", AppDomain.CurrentDomain.GetData("LICENSE_FILE"));
            logger.InfoFormat("LOADER_OPTIMIZATION: {0}", AppDomain.CurrentDomain.GetData("LOADER_OPTIMIZATION"));
            logger.InfoFormat("LOCATION_URI: {0}", AppDomain.CurrentDomain.GetData("LOCATION_URI"));
            logger.InfoFormat("PRIVATE_BINPATH: {0}", AppDomain.CurrentDomain.GetData("PRIVATE_BINPATH"));
            logger.InfoFormat("REGEX_DEFAULT_MATCH_TIMEOUT: {0}", AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT"));
            logger.InfoFormat("SHADOW_COPY_DIRS: {0}", AppDomain.CurrentDomain.GetData("SHADOW_COPY_DIRS"));
            logger.InfoFormat("TRUSTED_PLATFORM_ASSEMBLIES: {0}", AppDomain.CurrentDomain.GetData("TRUSTED_PLATFORM_ASSEMBLIES"));

            AppDomain.CurrentDomain.UnhandledException += OnException;
	        DataConverter = new RDotNetConverter(logger);
        }

        private static void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error("Unhandled exception", e.ExceptionObject as Exception);
        }

        #region Mange Data converter

        public static IDataConverter DataConverter { get; private set; }

        #endregion

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool LoadAssembly([MarshalAs(UnmanagedType.LPStr)] string pathOrAssemblyName)
        {
            logger.InfoFormat("[LoadAssembly] Path or AssemblyName: {0}", pathOrAssemblyName);

            if (string.IsNullOrEmpty(pathOrAssemblyName))
                return true;

            try
            {
                var filePath = pathOrAssemblyName.Replace("/", "\\");
                if (File.Exists(filePath))
                {
                    var assemblyName = new FileInfo(filePath).Name;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (string.Equals(assembly.ManifestModule.Name, assemblyName))
                            return true;
                    }
                    
                    Assembly.LoadFrom(filePath);
                    return true;
                }

                if (pathOrAssemblyName.IsFullyQualifiedAssemblyName())
                {
                    Assembly.Load(pathOrAssemblyName);
                    return true;
                }

                throw new FileLoadException($"Unable to load assembly: {pathOrAssemblyName}");
            }
            catch (Exception e)
            {
                LogExceptions("[LoadAssembly]", e);
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool CallStaticMethod(
            [MarshalAs(UnmanagedType.LPStr)] string typeName,
            [MarshalAs(UnmanagedType.LPStr)] string methodName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] long[] argumentsPtr,
            int argumentsSize,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] out long[] results,
            [Out] out int resultsSize)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

            logger.DebugFormat("[CallStaticMethod] TypeName: {0}, MethodName: {1}, NbArguments: {2}", typeName, methodName, argumentsSize);

            try
            {
                if (!typeName.TryGetType(out var type, out var errorMsg))
                    throw new TypeAccessException(errorMsg);

                var converters = new IConverter[argumentsSize];
                for (var i = 0; i < argumentsSize; i++)
                    converters[i] = DataConverter.GetConverter(argumentsPtr[i]);

                if (!type.TryGetMethod(methodName, flags, converters, out var method))
                    throw new MissingMethodException($"Method not found, Type: {typeName}, Method: {methodName}");

                InternalCallMethod(method, null, converters, out results, out resultsSize);

                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[CallStaticMethod]", e);
                results = null;
                resultsSize = 0;
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool GetStaticProperty(
            [MarshalAs(UnmanagedType.LPStr)] string typeName,
            [MarshalAs(UnmanagedType.LPStr)] string propertyName,
            [Out, MarshalAs(UnmanagedType.U8)] out long value)
        {
            logger.DebugFormat("[GetStaticProperty] TypeName: {0}, PropertyName: {1}", typeName, propertyName);

            try
            {
                if (!typeName.TryGetType(out var type, out var errorMsg))
                    throw new TypeAccessException(errorMsg);

                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                if (property == null)
                    throw new MissingMemberException($"Static property {propertyName} not found for Type: {type.FullName}");
                if (!property.CanRead)
                    throw new InvalidOperationException($"Static property {propertyName} can't be get for Type: {type.FullName}");

                var result = property.GetGetMethod().Call(null, new IConverter[0])[0];
                value = DataConverter.ConvertBack(property.PropertyType, result);

                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[GetStaticProperty]", e);
                value = 0;
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool SetStaticProperty(
            [MarshalAs(UnmanagedType.LPStr)] string typeName,
            [MarshalAs(UnmanagedType.LPStr)] string propertyName,
            [MarshalAs(UnmanagedType.U8)] long argumentPtr)
        {
            logger.DebugFormat("[SetStaticProperty] TypeName: {0}, PropertyName: {1}", typeName, propertyName);

            try
            {
                if (!typeName.TryGetType(out var type, out var errorMsg))
                    throw new TypeAccessException(errorMsg);

                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                if (property == null)
                    throw new MissingMemberException($"Static property {propertyName} not found for Type: {type.FullName}");
                if (!property.CanWrite)
                    throw new InvalidOperationException($"Static property {propertyName} can't be set for Type: {type.FullName}");

                var converters = new[] { DataConverter.GetConverter(argumentPtr) };

                property.GetSetMethod().Call(null, converters);
                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[SetStaticProperty]", e);
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool CreateObject(
            [MarshalAs(UnmanagedType.LPStr)] string typeName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] long[] argumentsPtr,
            int argumentsSize,
            [Out, MarshalAs(UnmanagedType.U8)] out long objectPtr)
        {
            logger.DebugFormat("[CreateInstance] TypeName: {0}", typeName);

            try
            {
                if (!typeName.TryGetType(out var type, out var errorMsg))
                    throw new TypeAccessException(errorMsg);

                var converters = new IConverter[argumentsSize];
                for (var i = 0; i < argumentsSize; i++)
                    converters[i] = DataConverter.GetConverter(argumentsPtr[i]);

                if (!type.TryGetConstructor(converters, out var ctor))
                    throw new MissingMemberException($"Constructor not found for Type: {typeName}");

                var result = ctor.Call(converters);
                objectPtr = DataConverter.ConvertBack(type, result);
                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[CreateInstance]", e);
                objectPtr = 0;
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool ReleaseObject([MarshalAs(UnmanagedType.U8)] long objectPtr)
        {
            logger.Debug("[ReleaseObject]");
            try
            {
                DataConverter.Release(objectPtr);
                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[ReleaseObject]", e);
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool CallMethod(
            [MarshalAs(UnmanagedType.U8)]  long objectPtr,
            [MarshalAs(UnmanagedType.LPStr)] string methodName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] long[] argumentsPtr,
            int argumentsSize,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] out long[] results,
            [Out] out int resultsSize)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            logger.DebugFormat("[CallMethod] Instance: {0}, MethodName: {1}", objectPtr, methodName);

            try
            {
                var instance = DataConverter.GetConverter(objectPtr)?.Convert(typeof(object));
                if (instance == null)
                    throw new ArgumentNullException(nameof(objectPtr));

                var type = instance.GetType();

                var converters = new IConverter[argumentsSize];
                for (var i = 0; i < argumentsSize; i++)
                    converters[i] = DataConverter.GetConverter(argumentsPtr[i]);

                if (!type.TryGetMethod(methodName, flags, converters, out var method))
                    throw new MissingMethodException($"Method not found for Type: {type}, Method: {methodName}");

                InternalCallMethod(method, instance, converters, out results, out resultsSize);

                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[CallMethod]", e);
                results = null;
                resultsSize = 0;
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool GetProperty(
            [MarshalAs(UnmanagedType.U8)]  long objectPtr,
            [MarshalAs(UnmanagedType.LPStr)] string propertyName,
            [Out, MarshalAs(UnmanagedType.U8)] out long value)
        {
            logger.DebugFormat("[GetProperty] Instance: {0}, PropertyName: {1}", objectPtr, propertyName);

            try
            {
                var instance = DataConverter.GetConverter(objectPtr)?.Convert(typeof(object));
                if (instance == null)
                    throw new ArgumentNullException(nameof(objectPtr));

                var type = instance.GetType();

                var property = type.GetProperty(propertyName);
                if (property == null)
                    throw new MissingMemberException($"Property {propertyName} not found for Type: {type.FullName}");

                if (!property.CanRead)
                    throw new InvalidOperationException($"Property {propertyName} can't be get for Type: {type.FullName}");

                var result = property.GetGetMethod().Call(instance, new IConverter[0])[0];
                value = DataConverter.ConvertBack(property.PropertyType, result);
                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[GetProperty]", e);
                value = 0;
                return false;
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool SetProperty(
            [MarshalAs(UnmanagedType.U8)] long objectPtr, 
            [MarshalAs(UnmanagedType.LPStr)] string propertyName,
            [MarshalAs(UnmanagedType.U8)] long argumentPtr)
        {
            logger.DebugFormat("[SetProperty] Instance: {0}, PropertyName: {1}", objectPtr, propertyName);

            try
            {
                var instance = DataConverter.GetConverter(objectPtr)?.Convert(typeof(object));
                if (instance == null)
                    throw new ArgumentNullException(nameof(objectPtr));

                var type = instance.GetType();

                var property = type.GetProperty(propertyName);
                if (property == null)
                    throw new MissingMemberException($"Property {propertyName} not found for Type: {type.FullName}");

                if (!property.CanWrite)
                    throw new InvalidOperationException($"Property {propertyName} can't be set for Type: {type.FullName}");

                var converters = new[] { DataConverter.GetConverter(argumentPtr) };

                property.GetSetMethod().Call(instance, converters);
                return true;
            }
            catch (Exception e)
            {
                LogExceptions("[SetProperty]", e);
                return false;
            }
        }

        private static void InternalCallMethod(MethodInfo method, object instance, IConverter[] converters, out long[] results, out int resultsSize)
        {
            var objects = method.Call(instance, converters);
            resultsSize = objects.Length;
            results = new long[resultsSize];

            results[0] = DataConverter.ConvertBack(method.ReturnType, objects[0]);
            if (resultsSize > 1)
            {
                var parameters = method.GetParameters();
                for (var i = 1; i < results.Length; i++)
                    results[i] = DataConverter.ConvertBack(parameters[i - 1].ParameterType.Extract(), objects[i]);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static string[] GetHierarchyTypeNames(object instance)
        {
            if (instance == null) return new string[0];

            var type = instance.GetType();
            var typeNames = new List<string>();

            while (type != null && type != typeof(object))
            {
                typeNames.Add(type.Name);
                type = type.BaseType;
            }

            return typeNames.ToArray();
        }

        // ReSharper disable once UnusedMember.Global
        public static void GenerateR6Classes(string[] typeNames, string file, bool appendFile = false, bool withInheritedTypes = false)
        {
            try
            {
                R6Generator.GenerateR6Classes(typeNames, file, DataConverter, appendFile, withInheritedTypes);
            }
            catch (Exception e)
            {
                LogExceptions("[GenerateR6Classes]", e);
            }
        }

        #region Manage errors
        private static readonly StringBuilder lastErrors = new StringBuilder();
        private static void LogExceptions(string message, Exception e)
        {
            logger.Error(message, e);

            lastErrors.Clear();
            lastErrors.AppendLine(message);
            while (e != null)
            {
                lastErrors.Append("[Message] ");
                lastErrors.AppendLine(e.Message);
                lastErrors.Append("[Source] ");
                lastErrors.AppendLine(e.Source);
                lastErrors.Append("[StackTrace] ");
                lastErrors.AppendLine(e.StackTrace);
                e = e.InnerException;
            }
        }

        [return: MarshalAs(UnmanagedType.LPStr)]
        public static string GetLastError() => lastErrors.ToString();

        #endregion
    }
}
