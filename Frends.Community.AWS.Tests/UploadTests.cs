using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using TestConfigurationHandler;

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
                AwsAccessKeyId = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.AccessKey"),
                AwsSecretAccessKey = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.SecretAccessKey"),
                BucketName = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.BucketName")
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

            void UploadThatThrows()
            {
                UploadTask.UploadFiles(input, _param, options, new CancellationToken());
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

            void UploadThatThrows()
            {
                UploadTask.UploadFiles(input, _param, options, new CancellationToken());
            }

            Assert.That(UploadThatThrows,
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("No files match the filemask within supplied path."));
        }
    }
}