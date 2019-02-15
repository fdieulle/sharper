using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Sharper.Converters.Resources
{
    public static class ResourcesLoader
    {
        static ResourcesLoader()
        {
            LoadOlsonWindowsMapping();
            UtcOlsonTimezone = new[] { TimeZoneInfo.Utc.GetOlsonTimezone() };
            LocalOlsonTimezone = new[] { TimeZoneInfo.Local.GetOlsonTimezone() };
        }

        #region Timezones

        private static readonly Type itSelf = MethodBase.GetCurrentMethod().DeclaringType;
        private const string EMBEDDED_RESOURCES = "Sharper.Converters.Resources";
        private static readonly Dictionary<string, TimeZoneInfo> olsonToWindows = new Dictionary<string, TimeZoneInfo>();
        private static readonly Dictionary<TimeZoneInfo, string> windowsToOlson = new Dictionary<TimeZoneInfo, string>();

        private static void LoadOlsonWindowsMapping()
        {
            var olsonWindowsMapping = LoadOlsonWindowsMappingTimeZones();
            var windowsTz = TimeZoneInfo.GetSystemTimeZones().ToDictionary(p => p.Id, p => p);

            var length = olsonWindowsMapping.Length;
            for (var i = 0; i < length; i++)
            {
                if (olsonWindowsMapping[i].Type == null) continue;

                if (!windowsTz.TryGetValue(olsonWindowsMapping[i].Other, out var timezone))
                    continue;

                var type = olsonWindowsMapping[i].Type;
                if (string.Equals("001", olsonWindowsMapping[i].Territory))
                    windowsToOlson[timezone] = type;

                olsonToWindows[type] = timezone;
            }

            olsonToWindows["CET"] = windowsTz["Central Europe Standard Time"];
            olsonToWindows["CEST"] = windowsTz["Central Europe Standard Time"];
            olsonToWindows["EET"] = windowsTz["E. Europe Standard Time"];
            olsonToWindows["EST"] = windowsTz["E. Europe Standard Time"];
            olsonToWindows["WET"] = windowsTz["W. Europe Standard Time"];
            olsonToWindows["EDT"] = windowsTz["US Eastern Standard Time"];
        }

        private static OlsonWindowsMapItem[] LoadOlsonWindowsMappingTimeZones()
        {
            string xml;
            using (var stream = itSelf.Assembly.GetManifestResourceStream($"{EMBEDDED_RESOURCES}.WindowsZones.xml"))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var reader = new StreamReader(stream))
                    xml = reader.ReadToEnd();
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var nodes = doc.SelectNodes("//*/mapTimezones/mapZone");
            var count = nodes?.Count ?? 0;
            var mapZones = new OlsonWindowsMapItem[count];
            for (var i = 0; i < count; i++)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (nodes[i].Attributes == null)
                    continue;

                var other = nodes[i].Attributes["other"].Value;
                var territory = nodes[i].Attributes["territory"].Value;

                var type = nodes[i].Attributes["type"].Value;
                var subTypes = type.Split(' ');
                foreach (var subType in subTypes)
                {
                    mapZones[i] = new OlsonWindowsMapItem
                    {
                        Other = other,
                        Type = subType,
                        Territory = territory
                    };
                }
            }

            return mapZones;
        }

        public class OlsonWindowsMapItem
        {
            public string Other { get; set; }
            public string Territory { get; set; }
            public string Type { get; set; }

            public override string ToString() 
                => $"Other: {Other}, Territory: {Territory}, Type: {Type}";
        }

        public static TimeZoneInfo GetWindowsTimezone(this string tzone)
        {
            if (string.IsNullOrEmpty(tzone))
                return TimeZoneInfo.Local;

            if (olsonToWindows.TryGetValue(tzone, out var timezone))
                return timezone;

            return TimeZoneInfo.Local;
        }

        public static string GetOlsonTimezone(this TimeZoneInfo timezone)
        {
            if (timezone == null) return "UTC";

            if (windowsToOlson.TryGetValue(timezone, out var tzone))
                return tzone ?? "UTC";

            return "UTC";
        }

        public static readonly DateTime Origin = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
        public static readonly long OriginTicks = Origin.Ticks;
        public static readonly string[] UtcOlsonTimezone;
        public static readonly string[] LocalOlsonTimezone;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime FromTicks(this double ticks, TimeZoneInfo timezone)
            => TimeZoneInfo.ConvertTime(new DateTime(OriginTicks + (long)(ticks * 1e7)), timezone);

        public static DateTime[] FromTicks(this double[] ticks, TimeZoneInfo timezone)
        {
            var result = new DateTime[ticks.Length];
            for (var i = 0; i < ticks.Length; i++)
                result[i] = FromTicks(ticks[i], timezone);
            
            return result;
        }

        public static DateTime[,] FromTicks(this double[,] ticks, TimeZoneInfo timezone)
        {
            var nRow = ticks.GetLength(0);
            var nCol = ticks.GetLength(1);

            var result = new DateTime[nRow, nCol];
            for (var i = 0; i < nRow; i++)
            {
                for (var j = 0; j < nCol; j++)
                    result[i, j] = FromTicks(ticks[i, j], timezone);
            }
            return result;
        }

        public static double ToTicks(this DateTime dateTime, out string[] tzone)
        {
            tzone = dateTime.Kind == DateTimeKind.Utc
                ? UtcOlsonTimezone
                : LocalOlsonTimezone;
            return (dateTime.ToUniversalTime() - Origin).TotalSeconds;
        }

        public static double[] ToTicks(this DateTime[] array, out string[] tzone)
        {
            var length = array.Length;
            var result = new double[length];

            tzone = length > 0 && array[0].Kind == DateTimeKind.Utc
                ? UtcOlsonTimezone
                : LocalOlsonTimezone;

            for (var i = 0; i < length; i++)
                result[i] = (array[i].ToUniversalTime() - Origin).TotalSeconds;

            return result;
        }

        public static double[,] ToTicks(this DateTime[,] matrix, out string[] tzone)
        {
            var nRow = matrix.GetLength(0);
            var nCol = matrix.GetLength(1);
            var result = new double[nRow, nCol];

            tzone = nRow > 0 && nCol > 0 && matrix[0, 0].Kind == DateTimeKind.Utc
                ? UtcOlsonTimezone
                : LocalOlsonTimezone;

            for (var i = 0; i < nRow; i++)
            {
                for (var j = 0; j < nCol; j++)
                    result[i, j] = (matrix[i, j] - Origin).TotalSeconds;
            }

            return result;
        }

        #endregion
    }
}
