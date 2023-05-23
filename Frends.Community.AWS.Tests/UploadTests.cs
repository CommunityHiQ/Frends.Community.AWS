using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    [Order(1)]
    [Description("Upload error tests.")]
    public class UploadErrorTests
    {
        private static Parameters _param;

        [OneTimeSetUp]
        public void Setup()
        {
            _param = new Parameters
            {
                AwsAccessKeyId = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey"),
                AwsSecretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey"),
                BucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName"),
                Region = (Regions)int.Parse(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_Region")),
                ThrowExceptionOnErrorResponse = true
            };
        }

        [Test]
        public void Error_IfSourcePathIsInvalid()
        {
            var input = new UploadInput
            {
                FileMask = @"*.test",
                FilePath = @"c:\there_is_no_folder_like_this\"
            };
            var options = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true
            };

            async Task<List<string>> UploadThatThrows()
            {
                var response = await UploadTask.UploadFiles(input, _param, options, new CancellationToken());
                return response.UploadedFiles;
            }

            Assert.That(UploadThatThrows, Throws.TypeOf<ArgumentException>().With.Message.StartsWith("Source path not found."));
        }

        [Test]
        public void Error_IfSwitchIsOnAndNothingMatches()
        {
            var input = new UploadInput
            {
                FileMask = "there_is_no_spoon.text",
                FilePath = Path.GetTempPath(),
                S3Directory = @"\"
            };

            var options = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true
            };

            async Task<List<string>> UploadThatThrows()
            {
                var response = await UploadTask.UploadFiles(input, _param, options, new CancellationToken());
                return response.UploadedFiles;
            }

            Assert.That(UploadThatThrows, Throws.TypeOf<ArgumentException>().With.Message.StartsWith("No files match the filemask within supplied path."));
        }

        [Test]
        public void Error_IfCredentialsAreInvalidAndThrowExceptionOnErrorResponseIsTrue()
        {
            var input = new UploadInput
            {
                FileMask = "TestFile1.csv",
                FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../TestData/"),
                S3Directory = @"\",
                CannedACL = S3CannedACLs.Private
            };

            var options = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true
            };

            var param = new Parameters
            {
                // Invalid AwsAccesKeyId.
                AwsAccessKeyId = "fnvfdvfkdjvn",

                // Invalid AwsSecretAccessKey.
                AwsSecretAccessKey = "bvfjhbvdjhvbjdhf",
                BucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName"),
                Region = (Regions)int.Parse(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_Region")),
                ThrowExceptionOnErrorResponse = true
            };

            async Task<List<string>> UploadThatThrows()
            {
                var response = await UploadTask.UploadFiles(input, param, options, new CancellationToken());
                return response.UploadedFiles;
            }

            Assert.That(UploadThatThrows, Throws.TypeOf<UploadException>());
        }

        [Test]
        public async Task Error_IfCredentialsAreInvalidAndThrowExceptionOnErrorResponseIsFalse()
        {
            var input = new UploadInput
            {
                FileMask = "TestFile1.csv",
                FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../TestData/"),
                S3Directory = @"\"
            };

            var options = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true
            };

            var param = new Parameters
            {
                // Invalid AwsAccesKeyId.
                AwsAccessKeyId = "fnvfdvfkdjvn",

                // Invalid AwsSecretAccessKey.
                AwsSecretAccessKey = "bvfjhbvdjhvbjdhf",
                BucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName"),
                Region = (Regions)int.Parse(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_Region")),
                ThrowExceptionOnErrorResponse = false
            };

            async Task<List<string>> UploadThatThrows()
            {
                var response = await UploadTask.UploadFiles(input, param, options, new CancellationToken());
                return response.UploadedFiles;
            }

            try
            {
                await UploadThatThrows();

            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }

        [Test]
        public async Task SendFileToS3Bucket()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../UploadTestData");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, "test.txt");
            File.WriteAllText(filePath, "This is a Test file");

            var input = new UploadInput
            {
                FileMask = "test.txt",
                FilePath = dir,
                S3Directory = "CommunityUploadTest/"
            };

            var options = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true
            };

            var response = await UploadTask.UploadFiles(input, _param, options, new CancellationToken());
            foreach (var key in response.UploadedFiles)
            {
                Assert.AreEqual("CommunityUploadTest/test.txt", key);
                await DeleteFileFromBucket(key, _param.BucketName);
            }
            Directory.Delete(dir, true);
        }

        [Test]
        public async Task SendFileToS3WritesDebugLog()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../debugLog");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, "debugLog.txt");
            File.WriteAllText(filePath, "This is a Test file");

            var input = new UploadInput
            {
                FileMask = "debugLog.txt",
                FilePath = dir,
                S3Directory = ""
            };

            var options = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true
            };

            var response = await UploadTask.UploadFiles(input, _param, options, new CancellationToken());
            Assert.IsFalse(string.IsNullOrEmpty(response.DebugLog));
            Directory.Delete(dir, true);
            await DeleteFileFromBucket("debugLog.txt", _param.BucketName);
        }

        [Test]
        public void FailedUploadProvidesDebugLog()
        {
            var input = new UploadInput
            {
                FileMask = "TestFile1.csv",
                FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../TestData/"),
                S3Directory = @"\",
                CannedACL = S3CannedACLs.Private
            };

            var options = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true
            };

            var param = new Parameters
            {
                // Invalid AwsAccesKeyId.
                AwsAccessKeyId = "fnvfdvfkdjvn",

                // Invalid AwsSecretAccessKey.
                AwsSecretAccessKey = "bvfjhbvdjhvbjdhf",
                BucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName"),
                Region = (Regions)int.Parse(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_Region")),
                ThrowExceptionOnErrorResponse = true
            };

            var response = Assert.ThrowsAsync<UploadException>(async () => await UploadTask.UploadFiles(input, param, options, new CancellationToken()));
            Assert.IsFalse(string.IsNullOrEmpty(response.DebugLog));
        }

        private static async Task DeleteFileFromBucket(string key, string bucketName)
        {
            using (var client = new AmazonS3Client(_param.AwsAccessKeyId, _param.AwsSecretAccessKey, RegionEndpoint.EUCentral1))
            {
                await client.DeleteObjectAsync(new DeleteObjectRequest() { BucketName = bucketName, Key = key });
            }
        }
    }
}
