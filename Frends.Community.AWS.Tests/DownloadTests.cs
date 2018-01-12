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
        [Test]
        public void Error_IfDestinationIsDirectory()
        {
            var param = new Parameters()
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = "baz" // fake
            }; 
            var input = new DownloadInput()
            {
                DownloadWholeDirectory = false,
                SourcePrefixAndKey = @"folder/file",
                DestinationPathAndFilename = @"c:\folder\"
            };

            ActualValueDelegate<Task> testDelegate =
                async () => await Download.DownloadAsync(
                    input, param, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("No filename supplied. "));
        }

        [Test]
        public void Error_IfSourceIsEmpty()
        {

            var param = new Parameters()
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = "baz" // fake
            };
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

            var param = new Parameters()
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = "baz" // fake
            };
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
        [Test]
        public void Error_IfSourceDirIsEmpty()
        {
            var param = new Parameters()
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = "baz" // fake
            };
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
            var param = new Parameters()
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = "baz" // fake
            };
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

        [Test]
        public void Error_IfFileDoesNotExistsAndAddingToReturnList()
        {
            ActualValueDelegate<DownloadResultToken> testDelegate = () => new DownloadResultToken()
            {
                ObjectKey = "zz.foo",
                Size = 987654321,
                FilePath = @"X:\yy\zz.foo"
            };

            Assert.That(testDelegate,
                Throws.Exception.With.Message.StartsWith(@"AWS Download File Error;"));
        }
        
        [Test]
        public void Download_CreatesProperDownloadResultToken()
        {
            var path = Path.GetTempFileName();
            var token = new DownloadResultToken()
            {
                FilePath = path
            };

            Assert.AreEqual(token.FilePath, path);
        }
    }
}
