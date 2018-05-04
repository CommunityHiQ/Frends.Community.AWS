using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    [Order(2)]
    [Description("Listing error tests.")]
    public class ListingErrorTests
    {
        [Test]
        public void Error_IfAccessKeyIsEmpty()
        {
            var linput = new ListInput();
            var param = new Parameters
            {
                AWSAccessKeyID = string.Empty,
                AWSSecretAccessKey = "foo", // fake
                BucketName = "bar" // fake
            };
            var opt = new ListOptions {FullResponse = true};

            async Task TestDelegate()
            {
                await ListTask.ListObjectsAsync(linput, param, opt, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.StartsWith("Cannot be empty. "));
        }

        [Test]
        public void Error_IfBucketNameIsEmpty()
        {
            var linput = new ListInput();
            var param = new Parameters
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = "bar", // fake
                BucketName = string.Empty
            };
            var opt = new ListOptions {FullResponse = true};

            async Task TestDelegate()
            {
                await ListTask.ListObjectsAsync(linput, param, opt, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.StartsWith("Cannot be empty. "));
        }

        [Test]
        public void Error_IfSecretKeyIsEmpty()
        {
            var linput = new ListInput();
            var param = new Parameters
            {
                AWSAccessKeyID = "foo", // fake
                AWSSecretAccessKey = string.Empty,
                BucketName = "bar" // fake
            };
            var opt = new ListOptions {FullResponse = true};

            async Task TestDelegate()
            {
                await ListTask.ListObjectsAsync(linput, param, opt, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.StartsWith("Cannot be empty. "));
        }
    }
}