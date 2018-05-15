using System;
using System.Linq;

namespace Frends.Community.AWS
{
    public static class Extensions
    {
        private const string StringSeparator = ", ";

        /// <summary>
        ///     Gets all string properties,
        ///     checks if there are nulls or empties as values,
        ///     forms an array from property names. Ordering to enable testing.
        ///     Error message contains ordered list of params.
        /// </summary>
        /// <param name="parameter"></param>
        public static void IsAnyNullOrWhiteSpaceThrow(this Parameters parameter)
        {
            if (parameter == null) throw new ArgumentNullException();
            var arr =
                (from pi in parameter.GetType().GetProperties()
                    where pi.PropertyType == typeof(string)
                    where string.IsNullOrWhiteSpace((string) pi.GetValue(parameter))
                    orderby pi.Name
                    select pi.Name)
                .ToArray();

            if (arr.Length > 0) throw new ArgumentNullException(string.Join(StringSeparator, arr));
        }
    }
}