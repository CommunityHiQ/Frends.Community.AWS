using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Amazon.S3;
using Amazon.S3.IO;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Amazon AWS S3 File DownloadTask
    /// </summary>
    public class DownloadTask
    {
        /// <summary>
        ///     Amazon AWS S3 DownloadFiles task.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="option"></param>
        /// <param name="cToken"></param>
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> DownloadFiles(
            [PropertyTab] DownloadInput input,
            [PropertyTab] Parameters parameters,
            [PropertyTab] DownloadOptions option,
            CancellationToken cToken
        )
        {
            cToken.ThrowIfCancellationRequested();
            if (parameters.AwsCredentials == null) parameters.IsAnyNullOrWhiteSpaceThrow();

            if (string.IsNullOrWhiteSpace(input.DestinationPath))
                throw new ArgumentNullException(nameof(input.DestinationPath));

            return DownloadUtility(input, parameters, option, cToken);
        }

        /// <summary>
        ///     Method to create client and call DownloadFiles.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cToken"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private static List<string> DownloadUtility(
            DownloadInput input,
            Parameters parameters,
            DownloadOptions option,
            CancellationToken cToken
        )
        {
            using (var s3Client = new AmazonS3Client(
                parameters.AwsAccessKeyId, parameters.AwsSecretAccessKey, Utilities.RegionSelection(parameters.Region)))
            {
                var dirInfo = new S3DirectoryInfo(s3Client, parameters.BucketName, input.S3Directory);
                if (dirInfo.Exists)
                    return DownloadFiles(input, option, dirInfo, cToken);
                throw new ArgumentException($"Cannot find {input.S3Directory} directory. {nameof(input.S3Directory)}");
            }
        }

        /// <summary>
        ///     Directory download operation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="option"></param>
        /// <param name="cToken"></param>
        /// <param name="dirInfo"></param>
        /// <returns>Task</returns>
        private static List<string> DownloadFiles(
            DownloadInput input,
            DownloadOptions option,
            S3DirectoryInfo dirInfo,
            CancellationToken cToken
        )
        {
            var files = dirInfo.GetFiles(input.SearchPattern,
                option.DownloadFromCurrentDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);

            if (option.ThrowErrorIfNoMatches && files.Length < 1)
                throw new ArgumentException("Could not find any files matching pattern.");

            var filelist = new List<string>();

            foreach (var file in files)
            {
                if (!file.Exists) continue;

                cToken.ThrowIfCancellationRequested();

                var path = Path.Combine(input.DestinationPath, file.Name);

                try
                {
                    // Apparently MoveToLocal does not have overwrite as signature :(
                    var localFile = option.DeleteSourceFile
                        ? file.MoveToLocal(path, option.Overwrite)
                        : file.CopyToLocal(path, option.Overwrite);

                    if (!localFile.Exists)
                        throw new IOException($"Could not find {localFile.FullName} from local filesystem.");

                    filelist.Add(localFile.FullName);
                }
                catch (IOException ex)
                {
                    // normal exception does not give filename info, which would be nice.
                    throw new IOException($"{path} already exists or insufficient privileges to write file.", ex);
                }
            }

            return filelist;
        }
    }
}