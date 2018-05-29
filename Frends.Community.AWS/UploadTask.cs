using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Amazon.S3.IO;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Filemask is Windows-style, eg. *.*, *file?.txt.
    ///     Bucket Name without s3://-prefix.
    /// </summary>
    public class UploadTask
    {
        /// <summary>
        ///     Filemask is Windows-style, eg. *.*, *file?.txt
        ///     Bucketname without s3://-prefix.
        /// </summary>
        /// <param name="input" />
        /// <param name="parameters" />
        /// <param name="options" />
        /// <param name="cancellationToken" />
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> UploadFiles(
            [PropertyTab] UploadInput input,
            [PropertyTab] Parameters parameters,
            [PropertyTab] UploadOptions options,
            CancellationToken cancellationToken
        )
        {
            // First check to see if this task gets performed at all.
            cancellationToken.ThrowIfCancellationRequested();

            parameters.IsAnyNullOrWhiteSpaceThrow();

            if (!Directory.Exists(input.FilePath))
                throw new ArgumentException(@"Source path not found. ", nameof(input.FilePath));

            var localRoot = new DirectoryInfo(input.FilePath);
            var filesToCopy = localRoot.GetFiles(
                input.FileMask ?? "*", // if filemask is not set, get all files.
                options.UploadFromCurrentDirectoryOnly
                    ? SearchOption.TopDirectoryOnly
                    : SearchOption.AllDirectories);

            if (options.ThrowErrorIfNoMatch && filesToCopy.Length < 1)
                throw new ArgumentException(
                    $"No files match the filemask within supplied path. {nameof(input.FileMask)}");

            return ExecuteUpload(localRoot, filesToCopy, input.S3Directory, parameters, options, cancellationToken);
        }

        private static List<string> ExecuteUpload(
            FileSystemInfo localRoot,
            IEnumerable<FileInfo> filesToCopy,
            string s3Directory,
            Parameters parameters,
            UploadOptions options,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = new List<string>();

            using (var client = Utilities.GetS3Client(parameters, cancellationToken))
            {
                var root = Utilities.GetS3Directory(
                    client,
                    s3Directory,
                    parameters.BucketName,
                    cancellationToken);

                foreach (var file in filesToCopy)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!file.Exists)
                        throw new IOException($"Source file does not exist: {file.FullName}");

                    var s3File = new S3FileInfo(
                        client,
                        parameters.BucketName,
                        Path.Combine(
                            root.Name,
                            options.PreserveFolderStructure
                                ? file.FullName.Replace(Utilities.GetFullPathWithEndingSlashes(localRoot.FullName),
                                    string.Empty)
                                : file.Name
                        ));

                    s3File = options.DeleteSource
                        ? s3File.MoveFromLocal(file.FullName, options.Overwrite)
                        : s3File.CopyFromLocal(file.FullName, options.Overwrite);

                    result.Add(options.ReturnListOfObjectKeys ? s3File.FullName : file.FullName);
                }
            }

            return result;
        }
    }
}