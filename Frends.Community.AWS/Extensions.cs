using System;
using System.Linq;

namespace Frends.Community.AWS
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class Extensions
    {
        private const string StringSeparator = ", ";

        /// <summary>
        /// Gets all string properties,
        /// checks if there are nulls or empties as values,
        /// forms an array from property names. Ordering to enable testing.
        /// Error message contains ordered list of params.
        /// </summary>
        /// <param name="property"></param>
        public static void IsAnyNullOrWhiteSpaceThrow(this object property)
        {
            if (property == null) throw new ArgumentNullException();
            var arr =
                (from pi in property.GetType().GetProperties()
                 where pi.PropertyType == typeof(string)
                 where string.IsNullOrWhiteSpace((string)pi.GetValue(property))
                 orderby pi.Name
                 select pi.Name)
                .ToArray();

            if (arr.Length > 0) throw new ArgumentNullException(string.Join(StringSeparator, arr));
        }
    }
}