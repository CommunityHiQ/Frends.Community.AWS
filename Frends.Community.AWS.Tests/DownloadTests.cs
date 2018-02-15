using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    public class Download_ErrorTests_SingleFiles
    {
        private static Parameters param = new Parameters()
        {
            AWSAccessKeyID = "foo", // fake
            AWSSecretAccessKey = "bar", // fake
            BucketName = "baz" // fake
        };

        [Test]
        public void Error_IfDestinationIsEmpty()
        {
            var i = new DownloadInput()
            {
                DestinationPath = null,
                SearchPattern = "*",
                S3Directory = ""
            };

            var o = new DownloadOptions()
            {
                DownloadFromCurrentDirectoryOnly = true,
                Overwrite = true,
                ThrowErrorIfNoMatches = true
            };

            ActualValueDelegate<List<string>> testDelegate =
                () => Download.DownloadFiles(
                    i, param, o, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(i.DestinationPath)}"));
        }
    }
}
