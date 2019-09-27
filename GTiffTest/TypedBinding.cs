using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;


namespace RanOpt.iBuilding.Common.UI
{
    /// <summary>
    /// Provides a new way to define the property name by using lambda expressions.
    /// </summary>
    public static class TypedBinding
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, string propertyName, params string[] propertyNames)
        {
            Raise(handler, sender, new[] {propertyName}.Concat(propertyNames));
        }

        private static void Raise(PropertyChangedEventHandler handler, object sender, IEnumerable<string> propertyNames)
        {
            if (handler == null)
            {
                return;
            }

            var setOfPropertyNames = propertyNames.ToArray();

            if (setOfPropertyNames.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentNullException(nameof(propertyNames), "One of the specified property names is null or empty.");
            }

            var args = setOfPropertyNames
                .Select(name => new PropertyChangedEventArgs(name));

            args.ToList().ForEach(arg => handler(sender, arg));
        }

        public static bool RaiseIfChanged(this PropertyChangedEventHandler handler, object sender, ref string current, string value, string propertyName, params string[] propertyNames)
        {
            return RaiseIfChanged(handler, sender, ref current, value, (r, l) => r == l, new[] {propertyName}.Concat(propertyNames));
        }

        public static bool RaiseIfChanged<T>(this PropertyChangedEventHandler handler, object sender, ref T current, T value, string propertyName, params string[] propertyNames) where T : struct
        {
            return RaiseIfChanged(handler, sender, ref current, value, (r, l) => Equals(r, l), new[] {propertyName}.Concat(propertyNames));
        }

        public static bool RaiseIfChanged<T>(this PropertyChangedEventHandler handler, object sender, ref T current, T value, Func<T, T, bool> equal, string propertyName, params string[] propertyNames)
        {
            return RaiseIfChanged(handler, sender, ref current, value, equal, new[] {propertyName}.Concat(propertyNames));
        }

        private static bool RaiseIfChanged<T>(PropertyChangedEventHandler handler, object sender, ref T current, T value, Func<T, T, bool> equal, IEnumerable<string> propertyNames)
        {
            if (equal(current, value))
            {
                return false;
            }

            current = value;

            Raise(handler, sender, propertyNames);

            return true;
        }
    }
}