using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security;

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
                return response;
            }

            Assert.That(UploadThatThrows,
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("Source path not found."));
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
                return response;
            }

            Assert.That(UploadThatThrows,
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("No files match the filemask within supplied path."));
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
                return response;
            }

            Assert.That(UploadThatThrows,
                Throws.TypeOf<SecurityException>()
                    .With.Message.StartsWith("Invalid Amazon S3 Credentials - data was not uploaded."));
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
                return response;
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
    }
}