using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    [Order(2)]
    [Description("Listing error tests.")]
    public class ListingErrorTests
    {
        [Test]
        public void Error_IfBucketNameIsEmpty()
        {
            var linput = new ListInput();

            var param = new Parameters
            {
                AwsAccessKeyId = "foo",
                AwsSecretAccessKey = "bar",
                BucketName = null
            };
            var opt = new ListOptions { FullResponse = true };

            async Task TestDelegate()
            {
                await ListTask.ListObjectsAsync(linput, param, opt, new CancellationToken());
            }

            Assert.That(TestDelegate, Throws.TypeOf<ArgumentNullException>().With.Message.StartWith("Value cannot be null."));
        }

        [Test]
        public void Error_IfParametersAreNull()
        {
            var linput = new ListInput();
            var param = new Parameters
            {
                AwsAccessKeyId = null,
                AwsSecretAccessKey = " ",
                BucketName = string.Empty
            };
            var opt = new ListOptions { FullResponse = true };

            async Task TestDelegate()
            {
                await ListTask.ListObjectsAsync(linput, param, opt, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith(
                        string.Join(", ",
                            nameof(param.AwsAccessKeyId),
                            nameof(param.AwsSecretAccessKey),
                            nameof(param.BucketName))));
        }
    }
}
