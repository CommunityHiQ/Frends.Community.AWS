using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Frends.Tasks.Attributes;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;

namespace Frends.Community.AWS.LI
{
#pragma warning disable 1591
    public enum Regions
    {
        EUWest1, EUWest2, EUCentral1,
        APNortheast1, APNortheast2, APSouth1, APSoutheast1, APSoutheast2,
        CACentral1, CNNorth1, SAEast1,
        USEast1, USEast2, USGovCloudWest1, USWest1, USWest2
    }
#pragma warning restore 1591

    /// <summary>
    /// Parameter class.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// AWS Bucketname, you can add folder path here.
        /// Prefix will be added after this.
        /// With Expression-mode, you can add prefixes ( #env.bucket + @"/prefix").
        /// </summary>
        [DefaultValue("*REQUIRED*")]
        [DefaultDisplayType(DisplayType.Expression)]
        public string BucketName { get; set; }


        /// <summary>
        /// Access Key.
        /// Use #env-variable with secret field for security.
        /// </summary>
        [DefaultValue("*REQUIRED*")]
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSAccessKeyID { get; set; }

        /// <summary>
        /// Secret Access Key.
        /// Use #env-variable with secret field for security.         
        /// </summary>
        [DefaultValue("*REQUIRED*")]
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSSecretAccessKey { get; set; }

        /// <summary>
        /// Region selection, choose nearest.
        /// Default: EUWest1.
        /// </summary>
        public Regions Region { get; set; }

        /// <summary>
        /// Object prefix ( folder path ).
        /// Use this to set prefix for each file.
        /// Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }

        /// <summary>
        /// Delimiter.
        /// Use in conjuction with prefix to limit results to specific level of the flat namespace hierarchy.
        /// See: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Delimiter { get; set; }

        /// <summary>
        /// Max number of keys returned.
        /// </summary>
        [DefaultValue(100)]
        [DefaultDisplayType(DisplayType.Expression)]
        public int MaxKeys { get; set; }

        /// <summary>
        /// A key to start the listing from.
        /// Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string StartAfter { get; set; }
        
        /// <summary>
        /// If previous response is truncated, use the ContinuationToken from that response here, to continue listing.
        /// Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string ContinuationToken { get; set; }
    }

    public class Options
    {
        /// <summary>
        /// True will return full response, false will return object keys only.
        /// </summary>
        [DefaultValue(false)]
        public bool FullResponse { get; set; }
    }

    /// <summary>
    /// Lists objects in between prefix and delimiter.
    /// </summary>
    public class Listing
    {
        public static async Task<JObject> ListObjectsAsync(Parameters param, Options opt, CancellationToken cToken)
        {
            #region Error tests
            if (string.IsNullOrWhiteSpace(param.AWSAccessKeyID))
                throw new ArgumentNullException(nameof(param.AWSAccessKeyID), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(param.AWSSecretAccessKey))
                throw new ArgumentNullException(nameof(param.AWSSecretAccessKey), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(param.BucketName))
                throw new ArgumentNullException(nameof(param.BucketName), "Cannot be empty. ");
            #endregion

            var response = new ListObjectsV2Response();

            using (var client = new AmazonS3Client(
                param.AWSAccessKeyID, 
                param.AWSSecretAccessKey, 
                RegionSelection(param.Region)))
            {
                var request = new ListObjectsV2Request()
                {
                    //RequestPayer = "",            
                    BucketName = param.BucketName,
                    Delimiter = param.Delimiter,
                    Encoding = EncodingType.Url,
                    FetchOwner = false,
                    MaxKeys = param.MaxKeys,
                    // added ternary to account for frends not including null as parameter by default...
                    Prefix = string.IsNullOrWhiteSpace(param.Prefix) ? null : param.Prefix,
                    ContinuationToken = string.IsNullOrWhiteSpace(param.ContinuationToken) ? null : param.ContinuationToken,
                    StartAfter = string.IsNullOrWhiteSpace(param.StartAfter) ? null : param.StartAfter
                };

                response = await client.ListObjectsV2Async(request, cToken);
            }

            var resp = opt.FullResponse ? 
                JObject.FromObject(response) : 
                new JObject(
                    new JProperty("S3Objects", JObject.FromObject(response)["S3Objects"]));

            return resp;
        }



        /// <summary>
        /// To create dropdown box for task with enum through RegionEndpoint static list from SDK.
        /// </summary>
        /// <param name="region">Region from enum list.</param>
        /// <returns>Amazon.RegionEndpoint static resource.</returns>
        private static RegionEndpoint RegionSelection(Regions region)
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
                //case Regions.USGovCloudWest1:
                //return RegionEndpoint.USGovCloudWest1;
                case Regions.USWest1:
                    return RegionEndpoint.USWest1;
                case Regions.USWest2:
                    return RegionEndpoint.USWest2;
                default:
                    return RegionEndpoint.EUWest1;
            }
        }
    }
}
