using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TestConfigurationHandler;

namespace Frends.Community.AWS.Tests
{
    [TestFixture]
    public class ListingErrorTests
    {
        [SetUp]
        public void Setup()
        {
            _param = new Parameters
            {
                AWSAccessKeyID = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.AccessKey"),
                AWSSecretAccessKey = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.SecretAccessKey"),
                BucketName = ConfigHandler.ReadConfigValue("HiQ.AWSS3Test.BucketName")
            };
        }

        private static Parameters _param;

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

        [Test]
        public void Error_ListShouldThrowIfDoesNotFindObjects()
        {
            var linput = new ListInput
            {
                ContinuationToken = string.Empty,
                Delimiter = "/",
                MaxKeys = 100,
                Prefix = "/this_should_not_exist",
                StartAfter = null
            };

            var opt = new ListOptions {FullResponse = false, ThrowErrorIfNoFilesFound = true};

            async Task TestDelegate()
            {
                await ListTask.ListObjectsAsync(linput, _param, opt, new CancellationToken());
            }

            Assert.That(TestDelegate,
                Throws.TypeOf<ArgumentException>()
                    .With.Message.StartsWith("No objects found with supplied parameters:"));
        }

        [Test]
        public async Task Test_ListShouldReturn()
        {
            var linput = new ListInput
            {
                ContinuationToken = string.Empty,
                Delimiter = "/",
                MaxKeys = 100,
                Prefix = "/",
                StartAfter = null
            };

            var opt = new ListOptions {FullResponse = true, ThrowErrorIfNoFilesFound = false};

            var result = await ListTask.ListObjectsAsync(linput, _param, opt, new CancellationToken());

            Assert.True(result.HasValues);
            Assert.AreEqual(result.Value<int>("HttpStatusCode"), 200); // should be full respons and proper request.
        }

        [Test]
        public async Task Test_ListShouldReturnArrayOnly()
        {
            var linput = new ListInput
            {
                ContinuationToken = string.Empty,
                Delimiter = "/",
                MaxKeys = 100,
                Prefix = "/",
                StartAfter = null
            };

            var opt = new ListOptions {FullResponse = false, ThrowErrorIfNoFilesFound = false};

            var result = await ListTask.ListObjectsAsync(linput, _param, opt, new CancellationToken());

            Assert.IsInstanceOf<JArray>(result);
        }
    }
}