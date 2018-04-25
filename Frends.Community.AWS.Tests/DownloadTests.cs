using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using TestConfigurationHandler;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    public class DownloadErrorTestsSingleFiles
    {
        private static Parameters _param;

        [SetUp]
        public void Setup()
        {
            _param = new Parameters
            {
                AWSAccessKeyID = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.AccessKey"),
                AWSSecretAccessKey = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.SecretAccessKey"),
                BucketName = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.BucketName")
            };
        }

        [Test]
        public void Error_IfDestinationIsEmpty()
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
                ThrowErrorIfNoMatches = true
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