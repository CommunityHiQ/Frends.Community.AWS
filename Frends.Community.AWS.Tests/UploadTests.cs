using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    public class UploadErrorTests
    {
        [Test]
        public void Error_IfSourcePathIsInvalid()
        {

            var input = new UploadInput()
            {
                FileMask = @"*.test",
                FilePath = @"c:\there_is_no_folder_like_this\"
            };
            var param = new Parameters()
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = "baz" // fake
            };
            var options = new UploadOptions()
            {
                ReturnListOfObjectKeys = true,
                StorageClass = StorageClasses.Standard,
                ThrowErrorIfNoMatch = true
            };

            async Task TestDelegate() => await Upload.UploadAsync(input, param, options, new CancellationToken());

            Assert.That(TestDelegate, 
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("Source path not found."));
        }

        [Test]
        public void Error_IfSwitchIsOnAndNothingMatches()
        {
            var input = new UploadInput()
            {
                FileMask = "there_is_no_spoon.text",
                FilePath = Path.GetTempPath(),
                Prefix = @"\"
            };
            var param = new Parameters()
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = "baz" // fake
            };
            var options = new UploadOptions()
            {
                ReturnListOfObjectKeys = true,
                StorageClass = StorageClasses.Standard,
                ThrowErrorIfNoMatch = true
            };

            async Task TestDelegate() => await Upload.UploadAsync(input, param, options, new CancellationToken());

            Assert.That(TestDelegate, 
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("No files match the filemask within supplied path."));
        }
    }
}
