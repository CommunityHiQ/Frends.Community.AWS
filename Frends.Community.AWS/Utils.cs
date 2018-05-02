using Amazon;
using Amazon.S3;

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
                case Regions.EUWest1:
                    return RegionEndpoint.EUWest1;
                case Regions.EUWest2:
                    return RegionEndpoint.EUWest2;
                case Regions.EUCentral1:
                    return RegionEndpoint.EUCentral1;
                case Regions.APSoutheast1:
                    return RegionEndpoint.APSoutheast1;
                case Regions.APSoutheast2:
                    return RegionEndpoint.APSoutheast2;
                case Regions.APNortheast1:
                    return RegionEndpoint.APNortheast1;
                case Regions.APNortheast2:
                    return RegionEndpoint.APNortheast2;
                case Regions.APSouth1:
                    return RegionEndpoint.APSouth1;
                case Regions.CACentral1:
                    return RegionEndpoint.CACentral1;
                case Regions.CNNorth1:
                    return RegionEndpoint.CNNorth1;
                case Regions.SAEast1:
                    return RegionEndpoint.SAEast1;
                case Regions.USEast1:
                    return RegionEndpoint.USEast1;
                case Regions.USEast2:
                    return RegionEndpoint.USEast2;
                case Regions.USWest1:
                    return RegionEndpoint.USWest1;
                case Regions.USWest2:
                    return RegionEndpoint.USWest2;
                default:
                    return RegionEndpoint.EUWest1;
            }
        }

        /// <summary>
        ///     You can select different type of storage redundancy option.
        ///     Standard being the default with high redundancy and accessed often, but is the most expensive.
        ///     Defaults to Standard.
        /// </summary>
        /// <param name="s3StorageClass"></param>
        /// <returns>S3StorageClass-object for UploadRequest-parameter.</returns>
        public static S3StorageClass StorageClassSelection(StorageClasses s3StorageClass)
        {
            switch (s3StorageClass)
            {
                case StorageClasses.Standard:
                    return S3StorageClass.Standard;
                case StorageClasses.StandardInfrequent:
                    return S3StorageClass.StandardInfrequentAccess;
                case StorageClasses.Reduced:
                    return S3StorageClass.ReducedRedundancy;
                case StorageClasses.Glacier:
                    return S3StorageClass.Glacier;
                default:
                    return S3StorageClass.Standard;
            }
        }
    }
}