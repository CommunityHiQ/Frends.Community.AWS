using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Frends.Community.AWS.LI.ListingTests
{
    [TestFixture]
    public class Listing_ErrorTests
    {
        [Test]
        public void Error_IfAccessKeyIsEmpty()
        {
            var param = new Parameters() {
                AWSAccessKeyID = String.Empty,
                AWSSecretAccessKey = "foo",
                BucketName = "bar",
            };
            var opt = new Options() { FullResponse = true };

            ActualValueDelegate<Task> testDelegate =
                async () => await Listing.ListObjectsAsync(
                    param, opt, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.StartsWith("Cannot be empty. "));
        }

        [Test]
        public void Error_IfSecretKeyIsEmpty()
        {
            var param = new Parameters()
            {
                AWSAccessKeyID = "foo",
                AWSSecretAccessKey = String.Empty,
                BucketName = "bar",
            };
            var opt = new Options() { FullResponse = true };

            ActualValueDelegate<Task> testDelegate =
                async () => await Listing.ListObjectsAsync(
                    param, opt, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.StartsWith("Cannot be empty. "));
        }

        [Test]
        public void Error_IfBucketNameIsEmpty()
        {
            var param = new Parameters()
            {
                AWSAccessKeyID = "foo",
                AWSSecretAccessKey = "bar",
                BucketName = String.Empty,
            };
            var opt = new Options() { FullResponse = true };

            ActualValueDelegate<Task> testDelegate =
                async () => await Listing.ListObjectsAsync(
                    param, opt, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.StartsWith("Cannot be empty. "));
        }
    }
}
