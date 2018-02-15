using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Frends.Community.AWS.UL.UploadTests
{
    [TestFixture]
    public class Upload_ErrorTests
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

            ActualValueDelegate<Task> testDelegate = 
                async () => await Upload.UploadAsync(
                    input, param, options, new CancellationToken());

            Assert.That(testDelegate, 
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

            ActualValueDelegate<Task> testDelegate = 
                async () => await Upload.UploadAsync(
                    input, param, options, new CancellationToken());

            Assert.That(testDelegate, 
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("No files match the filemask within supplied path."));
        }
    }
}
