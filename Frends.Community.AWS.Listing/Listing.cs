using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.Tasks.Attributes;
using Frends.Community.AWS.Helpers;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Amazon.S3.Transfer;

namespace Frends.Community.AWS.LI
{
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
        /// Default is EUWest1.
        /// </summary>
        public Regions Region { get; set; }

        /// <summary>
        /// Object prefix ( folder path ).
        /// Use this to set prefix for each file.
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }

        /// <summary>
        /// Delimiter.
        /// Use in conjuction with prefix to limit results to specific level of the flat namespace hierarchy.
        /// See: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
        /// </summary>
        [DefaultValue("/")]
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
        /// </summary>
        [DefaultValue("")]
        [DefaultDisplayType(DisplayType.Text)]
        public string StartAfter { get; set; }
        
        /// <summary>
        /// If previous response is truncated, use the ContinuationToken from that response here, to continue listing.
        /// </summary>
        [DefaultValue("")]
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
            var response = new ListObjectsV2Response();
            using (var client = new AmazonS3Client(
                param.AWSAccessKeyID, 
                param.AWSSecretAccessKey, 
                Helpers.Helpers.RegionSelection(param.Region)))
            {
                var request = new ListObjectsV2Request()
                {
                    BucketName = param.BucketName,
                    ContinuationToken = param.ContinuationToken,
                    Delimiter = param.Delimiter,
                    Encoding = EncodingType.Url,
                    FetchOwner = false,
                    MaxKeys = param.MaxKeys,
                    Prefix = param.Prefix,
                    //RequestPayer = "",
                    StartAfter = param.StartAfter                    
                };

                response = await client.ListObjectsV2Async(request, cToken);
            }

            var resp = opt.FullResponse ? JObject.FromObject(response) : new JObject(new JProperty("S3Objects", JObject.FromObject(response)["S3Objects"]));

            return resp;
        }
    }
}
