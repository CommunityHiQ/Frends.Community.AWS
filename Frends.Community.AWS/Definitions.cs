using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.AWS
{
    #region DownloadTask

    /// <summary>
    ///     Input class, you can download whole directories or single files.
    /// </summary>
    [DisplayName("Input")]
    public class DownloadInput
    {
        /// <summary>
        ///     Downloads all objects with this prefix.
        ///     Examples: folder, path/to/folder
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string S3Directory { get; set; }

        /// <summary>
        ///     String pattern to search files. Might not be exactly the same as in Windows.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string SearchPattern { get; set; }

        /// <summary>
        ///     Directory to create folders and files to.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string DestinationPath { get; set; }
    }

    /// <summary>
    /// </summary>
    [DisplayName("Options")]
    public class DownloadOptions
    {
        /// <summary>
        ///     Set to false to download files from current directory only.
        /// </summary>
        [DefaultValue(true)]
        public bool DownloadFromCurrentDirectoryOnly { get; set; }

        /// <summary>
        ///     Set to true to move files.
        /// </summary>
        [DefaultValue(false)]
        public bool DeleteSourceFile { get; set; }

        /// <summary>
        ///     Overwrite files.
        /// </summary>
        [DefaultValue(false)]
        public bool Overwrite { get; set; }

        /// <summary>
        ///     If search pattern does not match, throw error.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoMatches { get; set; }
    }

    #endregion

    #region List

    /// <summary>
    ///     Input parameter class for ListObjectsAsync
    /// </summary>
    [DisplayName("Input")]
    public class ListInput
    {
        /// <summary>
        ///     Object prefix ( folder path ).
        ///     Use this to set prefix for each file.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string Prefix { get; set; }

        /// <summary>
        ///     Delimiter.
        ///     Use in conjuction with prefix to limit results to specific level of the flat namespace hierarchy.
        ///     See: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string Delimiter { get; set; }

        /// <summary>
        ///     Max number of keys returned.
        /// </summary>
        [DefaultValue(100)]
        [DisplayFormat(DataFormatString = "Expression")]
        public int MaxKeys { get; set; }

        /// <summary>
        ///     A key to start the listing from.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string StartAfter { get; set; }

        /// <summary>
        ///     If previous response is truncated, use the ContinuationToken from that response here, to continue listing.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string ContinuationToken { get; set; }
    }

    /// <summary>
    ///     Options class.
    /// </summary>
    [DisplayName("Options")]
    public class ListOptions
    {
        /// <summary>
        ///     True will return full response, false will return object keys only.
        /// </summary>
        [DefaultValue(false)]
        public bool FullResponse { get; set; }

        /// <summary>
        ///     Throw error if reponse has no items in "S3Objects" array.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoFilesFound { get; set; }
    }

    #endregion

    #region UploadTask

    /// <summary>
    ///     Input filepath and filemask.
    /// </summary>
    [DisplayName("Input")]
    public class UploadInput
    {
        /// <summary>
        ///     Path to folder.
        ///     ( c:\temp\ , \\network\folder )
        /// </summary>
        [DefaultValue(@"c:\temp\")]
        [DisplayFormat(DataFormatString = "Text")]
        public string FilePath { get; set; }

        /// <summary>
        ///     Windows-style filemask, ( *.* , ?_file.*, foo_*.txt ).
        ///     Empty field = all files (*)
        /// </summary>
        [DefaultValue(@"*")]
        [DisplayFormat(DataFormatString = "Text")]
        public string FileMask { get; set; }

        /// <summary>
        ///     S3 root directory. If directory does not exist, it will be created.
        /// </summary>
        [DefaultValue(@"")]
        [DisplayFormat(DataFormatString = "Text")]
        public string S3Directory { get; set; }
    }

    /// <summary>
    ///     Task behaviour.
    ///     Defaults work fine.
    /// </summary>
    [DisplayName("Options")]
    public class UploadOptions
    {
        /// <summary>
        ///     Set to false to upload files from current directory only.
        ///     Filemask must not be empty.
        /// </summary>
        [DefaultValue(true)]
        public bool UploadFromCurrentDirectoryOnly { get; set; }

        /// <summary>
        ///     Set to true to create subdirectories in AWS.
        ///     Works with UploadFromCurrentDirectoryOnly = false.
        /// </summary>
        [DefaultValue(false)]
        public bool PreserveFolderStructure { get; set; }

        /// <summary>
        ///     Set to true to overwrite files with the same path and name
        ///     (aka object key).
        /// </summary>
        [DefaultValue(false)]
        public bool Overwrite { get; set; }

        /// <summary>
        ///     Deletes local source files after transfer.
        /// </summary>
        [DefaultValue(false)]
        public bool DeleteSource { get; set; }

        /// <summary>
        ///     If there are no files in the path matching the filemask supplied,
        ///     throw error.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        ///     If you wish, you can return object keys from S3
        ///     ( format: prefix/prefix/filename )
        /// </summary>
        [DefaultValue(false)]
        public bool ReturnListOfObjectKeys { get; set; }
    }

    #endregion

    #region Parameters for all!

    /// <summary>
    ///     Parameter class with username and keys.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        ///     AWS Bucketname
        ///     Example: yourbucket
        /// </summary>
        [DisplayName("Name of bucket")]
        public string BucketName { get; set; }

        /// <summary>
        ///     Key name for Amazon s3 File transfer aws_access_key_id
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DisplayName("AWS Access Key ID")]
        public string AwsAccessKeyId { get; set; }

        /// <summary>
        ///     Secret  key name for Amazon s3 File transfer aws_secret_access_key
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DisplayName("AWS Secret Access Key")]
        public string AwsSecretAccessKey { get; set; }

        /// <summary>
        ///     Region selection, default EUWest1.
        /// </summary>
        [DisplayName("Region")]
        public Regions Region { get; set; }
    }

    #endregion

    #region Enumerations

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum Regions
    {
        EuWest1,
        EuWest2,
        EuCentral1,
        ApNortheast1,
        ApNortheast2,
        ApSouth1,
        ApSoutheast1,
        ApSoutheast2,
        CaCentral1,
        CnNorth1,
        SaEast1,
        UsEast1,
        UsEast2,
        UsWest1,
        UsWest2
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    #endregion
}