using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using TestConfigurationHandler;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    [Order(3)]
    [Description("Download error tests.")]
    public class DownloadErrorTestsSingleFiles
    {
        private static Parameters _param;

        [OneTimeSetUp]
        public void Setup()
        {
            _param = new Parameters
            {
                AwsAccessKeyId = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.AccessKey"),
                AwsSecretAccessKey = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.SecretAccessKey"),
                BucketName = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.BucketName"),
                Region = (Regions) int.Parse(ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.Region"))
            };
        }


        [Test]
        public void Error_IfDestinationIsEmpty()
        {
            var i = new DownloadInput
            {
                DestinationPath = string.Empty,
                SearchPattern = "*",
                S3Directory = ""
            };

            var o = new DownloadOptions
            {
                DownloadFromCurrentDirectoryOnly = true,
                Overwrite = true,
                ThrowErrorIfNoMatches = true,
                DeleteSourceFile = false
            };

            List<string> TestDelegate()
            {
                return DownloadTask.DownloadFiles(i, _param, o, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(i.DestinationPath)}"));
        }

        [Test]
        public void Error_IfDestinationIsNull()
        {
            var i = new DownloadInput
            {
                DestinationPath = null,
                SearchPattern = "*",
                S3Directory = ""
            };

            var o = new DownloadOptions
            {
                DownloadFromCurrentDirectoryOnly = true,
                Overwrite = true,
                ThrowErrorIfNoMatches = true,
                DeleteSourceFile = false
            };

            List<string> TestDelegate()
            {
                return DownloadTask.DownloadFiles(i, _param, o, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(i.DestinationPath)}"));
        }
    }
}