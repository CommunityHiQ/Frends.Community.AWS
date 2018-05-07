using Amazon;

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
    }
}