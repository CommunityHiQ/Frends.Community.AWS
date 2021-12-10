using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.SecurityToken.Model;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Frends.Community.AWS.Tests
{
    /// <summary>
    ///     Tests will create local files, upload and download them.
    ///     Tests clean folders and files before and after tests.
    /// </summary>
    [TestFixture]
    [Order(4)]
    [Description("Ordered integration tests.")]
    public class IntegrationTests
    {
        [OneTimeSetUp]
        public static void Setup()
        {
            _param = new Parameters
            {
                AwsAccessKeyId = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AccessKey"),
                AwsSecretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_SecretAccessKey"),
                BucketName = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_BucketName"),
                Region = (Regions)int.Parse(Environment.GetEnvironmentVariable("HiQ_AWSS3Test_Region"))
            };
            _assumerParam = new Parameters
            {
                AwsAccessKeyId = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AssumerKey"),
                AwsSecretAccessKey = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_AssumerSecret"),
                BucketName = _param.BucketName,
                Region = _param.Region
            };
            _root = Environment.GetEnvironmentVariable("HiQ_AWS3Test_LocalTestFolder");
            _download = Path.Combine(_root, "download");
            _tempCredRole = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_Arn");
            _extId = Environment.GetEnvironmentVariable("HiQ_AWSS3Test_ExternalId");

            Cleanup(); // in case something was left behind

            if (!CreateTestFiles(_root, Files)) throw new IOException("Could not create testfiles.");
        }

        [OneTimeTearDown]
        public static void Cleanup()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);

            void DeleteRootFolder()
            {
                using (var s3Client = new AmazonS3Client(_param.AwsAccessKeyId, _param.AwsSecretAccessKey,
                    new AmazonS3Config {RegionEndpoint = Utilities.RegionSelection(_param.Region)}))
                {
                    var directoryToDelete = new S3DirectoryInfo(s3Client, _param.BucketName, Prefix.Replace("/", ""));
                    if (directoryToDelete.Exists) directoryToDelete.Delete(true);
                }
            }

            DeleteRootFolder();
        }

        private static Parameters _param;
        private static Parameters _assumerParam;
        private static string _root;
        private const string Prefix = "test/";
        private static string _download;
        private static string _tempCredRole;
        private static string _extId;

        private static readonly (string name, int bytes)[] WindowsFiles =
        {
            ("a.file", 1000),
            (@"upload\b.file", 1000),
            (@"upload\c.file", 100000),
            (@"upload\d.file", 1000000),
            (@"upload\prefix\e.file", 100000)
        };

        private static readonly (string name, int bytes)[] LinuxFiles =
        {
            ("a.file", 1000),
            (@"upload/b.file", 1000),
            (@"upload/c.file", 100000),
            (@"upload/d.file", 1000000),
            (@"upload/prefix/e.file", 100000)
        };

        private static readonly (string name, int bytes)[] Files = OSFiles();

        private static (string name, int bytes)[] OSFiles()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxFiles;
            }
            else
            {
                return WindowsFiles;
            }
        }

        private static bool CreateTestFiles(string root, IEnumerable<(string name, int bytes)> files)
        {
            foreach (var (name, bytes) in files)
            {
                /*if (!Directory.Exists(root))*/
                Directory.CreateDirectory(root);
                var path = Path.Combine(root, name);
                var file = new FileInfo(path);
                file.Directory?.Create();
                File.WriteAllBytes(
                    file.ToString(),
                    new byte[bytes]
                );
            }

            return true;
        }

        /// <summary>
        ///     AWS should be empty when running this test.
        /// </summary>
        [Test]
        [Order(1)]
        public void Error_ListShouldThrowIfDoesNotFindObjects()
        {
            var linput = new ListInput
            {
                ContinuationToken = string.Empty,
                Delimiter = "/",
                MaxKeys = 100,
                Prefix = null,
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
        [Order(2)]
        public async Task Test_UploadSingleShouldReturn()
        {
            var uinput = new UploadInput
            {
                FileMask = "a.file",
                FilePath = _root,
                S3Directory = Prefix
            };

            var opt = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true,
                UploadFromCurrentDirectoryOnly = true,
                DeleteSource = false,
                Overwrite = false,
                PreserveFolderStructure = true
            };

            var result = await UploadTask.UploadFiles(uinput, _param, opt, new CancellationToken());

            Assert.AreEqual(result.Count, 1);
        }

        [Test]
        [Order(3)]
        public async Task Test_UploadMultipleFilesShouldReturn()
        {
            var uinput = new UploadInput
            {
                FileMask = "*",
                FilePath = _root,
                S3Directory = Prefix
            };

            var opt = new UploadOptions
            {
                ReturnListOfObjectKeys = true,
                ThrowErrorIfNoMatch = true,
                UploadFromCurrentDirectoryOnly = false,
                DeleteSource = false,
                Overwrite = true,
                PreserveFolderStructure = true
            };

            var result = await UploadTask.UploadFiles(uinput, _param, opt, new CancellationToken());

            Assert.AreEqual(result.Count, 5);
        }

        [Test]
        [Order(4)]
        public async Task Test_ListShouldReturn()
        {
            var linput = new ListInput
            {
                ContinuationToken = string.Empty,
                Delimiter = "",
                MaxKeys = 100,
                Prefix = Prefix,
                StartAfter = null
            };

            var opt = new ListOptions { FullResponse = true, ThrowErrorIfNoFilesFound = false };

            // Sleep 2 seconds to give AWS some time to write the objects.
            // Otherwise only a.file object is created before the test executes.
            Thread.Sleep(2000);

            var result = await ListTask.ListObjectsAsync(linput, _param, opt, new CancellationToken());

            Assert.True(result.HasValues);
            Assert.AreEqual(200, result.Value<int>("HttpStatusCode")); // should be full response and proper request.
            Assert.AreEqual(5, result.Value<JArray>("S3Objects").Count);
        }

        [Test]
        [Order(5)]
        public async Task Test_ListNoFullResponseShouldReturnArrayOnly()
        {
            var linput = new ListInput
            {
                ContinuationToken = string.Empty,
                Delimiter = "/",
                MaxKeys = 100,
                Prefix = Prefix,
                StartAfter = null
            };

            var opt = new ListOptions { FullResponse = false, ThrowErrorIfNoFilesFound = false };

            var result = await ListTask.ListObjectsAsync(linput, _param, opt, new CancellationToken());

            Assert.IsInstanceOf<JArray>(result);
        }

        [Test]
        [Order(6)]
        public void Test_DownloadAllFiles()
        {
            var dinput = new DownloadInput
            {
                DestinationPath = _download,
                S3Directory = Prefix,
                SearchPattern = "*"
            };

            var opt = new DownloadOptions
            {
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatches = true
            };

            var result = DownloadTask.DownloadFiles(dinput, _param, opt, new CancellationToken());

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(_download, Path.GetDirectoryName(result.First()));
        }

        [Test]
        [Order(7)]
        public void Error_OverwriteOffShouldThrow()
        {
            var dinput = new DownloadInput
            {
                DestinationPath = _download,
                S3Directory = Prefix,
                SearchPattern = "a*"
            };

            var opt = new DownloadOptions
            {
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = false,
                ThrowErrorIfNoMatches = true
            };

            void DelegateDownload()
            {
                DownloadTask.DownloadFiles(dinput, _param, opt, new CancellationToken());
            }

            Assert.Throws<AggregateException>(DelegateDownload);
        }

        [Test]
        [Order(8)]
        public void Test_DownloadOverwriteShouldReturn()
        {
            var dinput = new DownloadInput
            {
                DestinationPath = _download,
                S3Directory = Prefix,
                SearchPattern = "b*"
            };

            var opt = new DownloadOptions
            {
                DeleteSourceFile = false,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = true,
                ThrowErrorIfNoMatches = true
            };

            var result = DownloadTask.DownloadFiles(dinput, _param, opt, new CancellationToken());

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("b.file", Path.GetFileName(result.First()));
        }

        [Test]
        [Order(9)]
        public void Test_DownloadShouldDeleteSources()
        {
            var dinput = new DownloadInput
            {
                DestinationPath = _download,
                S3Directory = Prefix,
                SearchPattern = "c*"
            };

            var opt = new DownloadOptions
            {
                DeleteSourceFile = true,
                DownloadFromCurrentDirectoryOnly = false,
                Overwrite = true,
                ThrowErrorIfNoMatches = true
            };

            var result = DownloadTask.DownloadFiles(dinput, _param, opt, new CancellationToken());

            // try to download again with error throw on, should not find the same file.
            void DownloadThatThrows()
            {
                DownloadTask.DownloadFiles(dinput, _param, opt, new CancellationToken());
            }

            Assert.AreEqual(1, result.Count);
            Assert.Throws<AggregateException>(DownloadThatThrows);
        }

        [Test]
        [Order(10)]
        public async Task Test_GetTemporaryCredentialsShouldReturnProper()
        {
            var tcinput = new TempCredInput
            {
                CredentialDurationSeconds = 3600,
                CredentialExternalId = _extId,
                CredentialUniqueRequestId = "12345foobar",
                RoleArn = _tempCredRole
            };

            var result = await GetTemporaryCredentialsTask.GetTemporaryCredentialsAsync(
                tcinput, _assumerParam, new CancellationToken());

            Assert.IsInstanceOf(typeof(Credentials), result);
        }
    }
}