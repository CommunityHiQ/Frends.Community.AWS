using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Frends.Community.AWS.UploadTests
{
    [TestFixture]
    public class UploadTests
    {
        [Test]
        public void ShouldReturnErrorIfFileDoesNotExist()
        {
            var input = new Input()
            {
                FileMask = "",
                FilePath = ""
            };
            var param = new Parameters() { };
            var opt = new Options()
            {
                ReturnListOfObjectKeys = true,
                StorageClass = StorageClasses.Standard,
                ThrowErrorIfNoMatch = true
            };

            ActualValueDelegate<object> testDelegate = () => Upload.UploadFiles(
                input, param, opt, new CancellationToken());

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ShouldReturnErrorIfSwitchIsOnAndNothingMatches()
        {
            var input = new Input()
            {
                FileMask = "there_is_no_spoon.text",
                FilePath = Path.GetTempPath()
            };
            var param = new Parameters() { };
            var opt = new Options()
            {
                ReturnListOfObjectKeys = true,
                StorageClass = StorageClasses.Standard,
                ThrowErrorIfNoMatch = true
            };

            ActualValueDelegate<object> testDelegate = () => Upload.UploadFiles(
                input, param, opt, new CancellationToken());

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }
    }
}
