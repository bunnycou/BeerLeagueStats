namespace BLStats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class EnumerableExtensions
    {
        public static IOrderedEnumerable<KeyValuePair<string, PlayerData>> OrderByDynamic(
            this IEnumerable<KeyValuePair<string, PlayerData>> source, string propertyName)
        {
            return source.OrderBy(item => GetPropertyValue(item.Value, propertyName));
        }

        public static IOrderedEnumerable<KeyValuePair<string, PlayerData>> OrderByDescendingDynamic(
            this IEnumerable<KeyValuePair<string, PlayerData>> source, string propertyName)
        {
            return source.OrderByDescending(item => GetPropertyValue(item.Value, propertyName));
        }

        private static object? GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(obj, null);
        }
    }
}