using System;
using NUnit.Framework;
using System.Threading;
using NUnit.Framework.Constraints;

namespace Frends.Community.AWS.DownloadTests
{
    [TestFixture]
    public class DownloadTests
    {
        [Test]
        public void ShouldReturnErrorIfNotADirectory()
        {
            var param = new Download.Parameters();
            var input = new Download.Input()
            {
                SourceDirectory = @"folder/prefix",
                DownloadWholeDirectory = true
            };

            ActualValueDelegate<object> testDelegate = () => Download.DownloadFiles(
                input, param, new CancellationToken());

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ShouldReturnErrorIfSourceIsDirectory()
        {
            var param = new Download.Parameters();
            var input = new Download.Input()
            {
                DownloadWholeDirectory = false,
                SourcePrefixAndFilename = @"folder/file/"
            };

            ActualValueDelegate<object> testDelegate = () => Download.DownloadFiles(
                input, param, new CancellationToken());

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ShouldReturnErrorIfDestinationIsDirectory()
        {
            var param = new Download.Parameters();
            var input = new Download.Input()
            {
                DownloadWholeDirectory = false,
                SourcePrefixAndFilename = @"folder/file",
                DestinationPathAndFilename = @"c:\folder\"
            };
            
            ActualValueDelegate<object> testDelegate = () => Download.DownloadFiles(
                input, param, new CancellationToken());
            
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());            
        }
        
    }
}
