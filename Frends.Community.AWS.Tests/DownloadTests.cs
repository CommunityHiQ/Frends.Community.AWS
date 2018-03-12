using System;
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
        public void Error_IfSourceIsEmpty()
        {
            var input = new DownloadInput()
            {
                DownloadWholeDirectory = false,
                SourcePrefixAndKey = @" ",
                DestinationPathAndFilename = @"c:\folder\"
            };

            ActualValueDelegate<Task> testDelegate =
                async () => await Download.DownloadAsync(
                    input, param, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(input.SourcePrefixAndKey)}"));
        }

        [Test]
        public void Error_IfDestinationIsEmpty()
        {
            var input = new DownloadInput()
            {
                DownloadWholeDirectory = false,
                SourcePrefixAndKey = @"foo/bar",
                DestinationPathAndFilename = @" "
            };

            ActualValueDelegate<Task> testDelegate =
                async () => await Download.DownloadAsync(
                    input, param, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(input.DestinationPathAndFilename)}"));
        }
    }

    [TestFixture]
    public class Download_ErrorTest_Directory
    {
        private static Parameters param = new Parameters()
        {
            AWSAccessKeyID = "foo", // fake
            AWSSecretAccessKey = "bar", // fake
            BucketName = "baz" // fake
        };

        [Test]
        public void Error_IfSourceDirIsEmpty()
        {
            var input = new DownloadInput()
            {
                DownloadWholeDirectory = true,
                DestinationPath = "c:\foo\bar",
                SourcePrefix = " "
            };

            ActualValueDelegate<Task> testDelegate =
                async () => await Download.DownloadAsync(
                    input, param, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(input.SourcePrefix)}"));
        }

        [Test]
        public void Error_IfDestinationPathIsEmpty()
        {
            var input = new DownloadInput()
            {
                DownloadWholeDirectory = true,
                DestinationPath = " ",
                SourcePrefix = @"\"
            };

            ActualValueDelegate<Task> testDelegate =
                async () => await Download.DownloadAsync(
                    input, param, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(input.DestinationPath)}"));
        }        
    }
}
