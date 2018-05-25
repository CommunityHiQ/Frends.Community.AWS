using System.IO;
using System.Threading;
using Amazon;
using Amazon.S3;
using Amazon.S3.IO;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Utility class.
    /// </summary>
    public class Utilities
    {
        /// <summary>
        ///     To create dropdown box for task with enum through RegionEndpoint static list from SDK.
        /// </summary>
        /// <param name="region">Region from enum list.</param>
        /// <returns>Amazon.RegionEndpoint static resource.</returns>
        public static RegionEndpoint RegionSelection(Regions region)
        {
            switch (region)
            {
                case Regions.EuWest1:
                    return RegionEndpoint.EUWest1;
                case Regions.EuWest2:
                    return RegionEndpoint.EUWest2;
                case Regions.EuCentral1:
                    return RegionEndpoint.EUCentral1;
                case Regions.ApSoutheast1:
                    return RegionEndpoint.APSoutheast1;
                case Regions.ApSoutheast2:
                    return RegionEndpoint.APSoutheast2;
                case Regions.ApNortheast1:
                    return RegionEndpoint.APNortheast1;
                case Regions.ApNortheast2:
                    return RegionEndpoint.APNortheast2;
                case Regions.ApSouth1:
                    return RegionEndpoint.APSouth1;
                case Regions.CaCentral1:
                    return RegionEndpoint.CACentral1;
                case Regions.CnNorth1:
                    return RegionEndpoint.CNNorth1;
                case Regions.SaEast1:
                    return RegionEndpoint.SAEast1;
                case Regions.UsEast1:
                    return RegionEndpoint.USEast1;
                case Regions.UsEast2:
                    return RegionEndpoint.USEast2;
                case Regions.UsWest1:
                    return RegionEndpoint.USWest1;
                case Regions.UsWest2:
                    return RegionEndpoint.USWest2;
                default:
                    return RegionEndpoint.EUWest1;
            }
        }

        /// <summary>
        ///     Appends ending slash to path if it's missing.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>string</returns>
        public static string GetFullPathWithEndingSlashes(string input)
        {
            return Path.GetFullPath(input)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                   + Path.DirectorySeparatorChar;
        }

        /// <summary>
        ///     Gets information of directory. If it does not exist, it will create it.
        /// </summary>
        /// <param name="s3Client"></param>
        /// <param name="s3Directory"></param>
        /// <param name="bucketName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>S3DirectoryInfo</returns>
        public static S3DirectoryInfo GetS3Directory(
            IAmazonS3 s3Client,
            string s3Directory,
            string bucketName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dirInfo = new S3DirectoryInfo(s3Client, bucketName, s3Directory);
            if (dirInfo.Exists) return dirInfo;
            dirInfo.Create();
            return dirInfo;
        }

        /// <summary>
        ///     Returns S3 client.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>AmazonS3Client</returns>
        public static IAmazonS3 GetS3Client(
            Parameters parameters,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // optionally we can configure client, if the need arises.
            return new AmazonS3Client(
                parameters.AwsAccessKeyId,
                parameters.AwsSecretAccessKey,
                RegionSelection(parameters.Region));
        }
    }
}