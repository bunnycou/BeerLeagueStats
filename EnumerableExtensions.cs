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

        private static object? GetPropertyValue(PlayerData obj, string propertyName)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj, null);
            }

            var methodInfo = obj.GetType().GetMethod(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null)
            {
                return methodInfo.Invoke(obj, null);
            }

            return null;
        }
    }
}