using System.IO;
using System.Threading;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace Frends.Community.AWS
{
    /// <summary>
    /// Utility class.
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// To create dropdown box for task with enum through RegionEndpoint static list from SDK.
        /// </summary>
        /// <param name="region">Region from enum list.</param>
        /// <returns>Amazon.RegionEndpoint static resource.</returns>
        public static RegionEndpoint RegionSelection(Regions region)
        {
            switch (region)
            {
                case Regions.AfSouth1:
                    return RegionEndpoint.AFSouth1;
                case Regions.ApEast1:
                    return RegionEndpoint.APEast1;
                case Regions.ApNortheast1:
                    return RegionEndpoint.APNortheast1;
                case Regions.ApNortheast2:
                    return RegionEndpoint.APNortheast2;
                case Regions.ApNortheast3:
                    return RegionEndpoint.APNortheast3;
                case Regions.ApSouth1:
                    return RegionEndpoint.APSouth1;
                case Regions.ApSoutheast1:
                    return RegionEndpoint.APSoutheast1;
                case Regions.ApSoutheast2:
                    return RegionEndpoint.APSoutheast2;
                case Regions.CaCentral1:
                    return RegionEndpoint.CACentral1;
                case Regions.CnNorth1:
                    return RegionEndpoint.CNNorth1;
                case Regions.CnNorthWest1:
                    return RegionEndpoint.CNNorthWest1;
                case Regions.EuCentral1:
                    return RegionEndpoint.EUCentral1;
                case Regions.EuNorth1:
                    return RegionEndpoint.EUNorth1;
                case Regions.EuSouth1:
                    return RegionEndpoint.EUSouth1;
                case Regions.EuWest1:
                    return RegionEndpoint.EUWest1;
                case Regions.EuWest2:
                    return RegionEndpoint.EUWest2;
                case Regions.EuWest3:
                    return RegionEndpoint.EUWest3;
                case Regions.MeSouth1:
                    return RegionEndpoint.MESouth1;
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
        /// Appends ending slash to path if it's missing.
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
        /// Returns S3 client.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>AmazonS3Client</returns>
        public static IAmazonS3 GetS3Client(
            Parameters parameters)
        {
            var region = RegionSelection(parameters.Region);

            // Use the application's default configuration.
            if(parameters.UseDefaultCredentials) return new AmazonS3Client(region);

            // Optionally we can configure client, if the need arises.
            return parameters.AwsCredentials == null
                ? new AmazonS3Client(
                    parameters.AwsAccessKeyId,
                    parameters.AwsSecretAccessKey,
                    region)
                : new AmazonS3Client(parameters.AwsCredentials, region);
        }

        /// <summary>
        /// Delete source file from S3 or agent.
        /// </summary>
        /// <param name="s3Client"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="bucketName"></param>
        /// <param name="filePath"></param>
        /// <param name="sourceIsS3"></param>
        /// <returns></returns>
        public static void DeleteSourceFile(
            AmazonS3Client s3Client,
            CancellationToken cancellationToken,
            string bucketName,
            string filePath,
            bool sourceIsS3
        )
        {
            if (sourceIsS3)
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = filePath
                };

                s3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);
            }
            else
            {
                var file = new FileInfo(filePath);
                while (IsFileLocked(file)) Thread.Sleep(1000);
                File.Delete(filePath);
            }
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // The file is unavailable because it is:
                // 1. Still being written to.
                // 2. Being processed by another thread.
                // 3. Does not exist (has already been processed).
                return true;
            }
            finally
            {
                stream?.Close();
            }

            // File is not locked.
            return false;
        }
    }
}