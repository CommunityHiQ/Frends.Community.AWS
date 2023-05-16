using NUnit.Framework;
using System;
using System.Threading;

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

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName"))) throw new ArgumentException("Some secrets/environment variables are null/empty.");

            _param = new Parameters
            {
                AwsAccessKeyId = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey"),
                AwsSecretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey"),
                BucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName"),
                Region = (Regions)int.Parse(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_Region") ?? throw new ArgumentException("Some secrets/environment variables are null/empty."))
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

            void TestDelegate()
            {
                DownloadTask.DownloadFiles(i, _param, o, new CancellationToken());
            }

            Assert.Throws<ArgumentNullException>(TestDelegate);
            Assert.That(TestDelegate, Throws.TypeOf<ArgumentNullException>().With.Message.EndsWith($"{nameof(i.DestinationPath)}"));
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

            void TestDelegate()
            {
                DownloadTask.DownloadFiles(i, _param, o, new CancellationToken());
            }

            Assert.That(TestDelegate, Throws.TypeOf<ArgumentNullException>().With.Message.EndsWith($"{nameof(i.DestinationPath)}"));
        }
    }
}
