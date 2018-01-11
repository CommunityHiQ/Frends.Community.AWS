﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.Tasks.Attributes;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Lists objects in between prefix and delimiter.
    /// </summary>
    public class Listing
    {
        public static async Task<JObject> ListObjectsAsync(
            [CustomDisplay(DisplayOption.Tab)] ListInput input,
            [CustomDisplay(DisplayOption.Tab)] Parameters parameters,
            [CustomDisplay(DisplayOption.Tab)] ListOptions options,
            CancellationToken cToken
            )
        {
            #region Error tests
            if (string.IsNullOrWhiteSpace(parameters.AWSAccessKeyID))
                throw new ArgumentNullException(nameof(parameters.AWSAccessKeyID), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.AWSSecretAccessKey))
                throw new ArgumentNullException(nameof(parameters.AWSSecretAccessKey), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.BucketName))
                throw new ArgumentNullException(nameof(parameters.BucketName), "Cannot be empty. ");
            #endregion

            var response = new ListObjectsV2Response();

            using (var client = new AmazonS3Client(
                parameters.AWSAccessKeyID, 
                parameters.AWSSecretAccessKey, 
                Utilities.RegionSelection(parameters.Region)))
            {
                var request = new ListObjectsV2Request()
                {     
                    BucketName = parameters.BucketName,
                    Delimiter = string.IsNullOrWhiteSpace(input.Delimiter) ? null : input.Delimiter,
                    Encoding = EncodingType.Url,
                    FetchOwner = false,
                    MaxKeys = input.MaxKeys,
                    // added ternary to account for frends not including null as parameter by default...
                    Prefix = string.IsNullOrWhiteSpace(input.Prefix) ? null : input.Prefix,
                    ContinuationToken = string.IsNullOrWhiteSpace(input.ContinuationToken) ? null : input.ContinuationToken,
                    StartAfter = string.IsNullOrWhiteSpace(input.StartAfter) ? null : input.StartAfter
                };

                response = await client.ListObjectsV2Async(request, cToken);
            }

            var resp = options.FullResponse ? 
                JObject.FromObject(response) : 
                new JObject(
                    new JProperty("S3Objects", JObject.FromObject(response)["S3Objects"]));

            return resp;
        }
    }
}
