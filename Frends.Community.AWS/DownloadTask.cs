using System;
using System.IO;
using System.Threading;
using Frends.Tasks.Attributes;
using Amazon.S3;
using Amazon.S3.IO;
using System.Collections.Generic;

namespace Frends.Community.AWS
{
    /// <summary>        
    ///     Amazon AWS S3 File Download task
    /// </summary>
    public class Download
    {
        /// <summary>
        ///     Amazon AWS S3 Transfer files.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="option"></param>
        /// <param name="cToken"></param>
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> DownloadFiles(
            [CustomDisplay(DisplayOption.Tab)] DownloadInput input,
            [CustomDisplay(DisplayOption.Tab)] Parameters parameters,
            [CustomDisplay(DisplayOption.Tab)] DownloadOptions option,
            CancellationToken cToken
            )
        {
            cToken.ThrowIfCancellationRequested();

            #region Error checks and helps
            if (string.IsNullOrWhiteSpace(parameters.AWSAccessKeyID))
                throw new ArgumentNullException(nameof(parameters.AWSAccessKeyID), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.AWSSecretAccessKey))
                throw new ArgumentNullException(nameof(parameters.AWSSecretAccessKey), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.BucketName))
                throw new ArgumentNullException(nameof(parameters.BucketName), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(input.DestinationPath))
                throw new ArgumentNullException(nameof(input.DestinationPath), "Cannot be empty. ");

            #endregion

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
                parameters.AWSAccessKeyID, parameters.AWSSecretAccessKey, Utilities.RegionSelection(parameters.Region)))
            {
                var dirInfo = new S3DirectoryInfo(s3Client, parameters.BucketName, input.S3Directory);
                if (dirInfo.Exists)
                    return DownloadFiles(input, option, dirInfo, cToken);
                throw new ArgumentException($"Cannot find {input.S3Directory} directory. {nameof(input.S3Directory)}");
            }
        }

        /// <summary>
        /// Directory download operation.
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

                var path = input.DestinationPath + file.Name;
                
                var fileinfo = option.DeleteSourceFile
                    ? file.MoveToLocal(path)
                    : file.CopyToLocal(path, option.Overwrite);
                
                if (!fileinfo.Exists)
                    throw new IOException($"Could not find {fileinfo.FullName} from local filesystem.");

                filelist.Add(fileinfo.FullName);
            }

            return filelist;
        }
    }
}
