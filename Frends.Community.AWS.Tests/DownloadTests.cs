using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    public class DownloadErrorTestsSingleFiles
    {
        private static readonly Parameters Param = new Parameters
        {
            AWSAccessKeyID = "foo", // fake
            AWSSecretAccessKey = "bar", // fake
            BucketName = "baz" // fake
        };

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
                return DownloadTask.DownloadFiles(i, Param, o, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(i.DestinationPath)}"));
        }
    }
}