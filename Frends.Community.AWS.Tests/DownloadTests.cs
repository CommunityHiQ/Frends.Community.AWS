﻿using System;
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
                SourcePrefixAndFilename = @"folder/file",
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
                SourcePrefixAndFilename = @" ",
                DestinationPathAndFilename = @"c:\folder\"
            };

            ActualValueDelegate<Task> testDelegate =
                async () => await Download.DownloadAsync(
                    input, param, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(input.SourcePrefixAndFilename)}"));
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
                SourcePrefixAndFilename = @"foo/bar",
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
                DestinationPath = "foo/bar",
                SourceDirectory = " "
            };

            ActualValueDelegate<Task> testDelegate =
                async () => await Download.DownloadAsync(
                    input, param, new CancellationToken());

            Assert.That(testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With.Message.EndsWith($"{nameof(input.SourceDirectory)}"));
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
                SourceDirectory = Path.GetTempPath()
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