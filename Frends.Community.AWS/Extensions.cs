using System;
using System.IO;
using System.Linq;
using Amazon.S3.IO;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Extension methods.
    /// </summary>
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

        /// <summary>
        ///     Move feature with source delete and option for overwrite.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="path"></param>
        /// <param name="overwrite"></param>
        /// <returns>FileInfo</returns>
        public static FileInfo MoveToLocal(this S3FileInfo file, string path, bool overwrite)
        {
            var localFile = file.CopyToLocal(path, overwrite);
            file.Delete();
            return localFile;
        }
    }
}