using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.AWS
{
    #region DownloadTask
    #pragma warning disable CS1591

    /// <summary>
    /// Input class, you can download whole directories or single files.
    /// </summary>
    [DisplayName("Input")]
    public class DownloadInput
    {
        /// <summary>
        /// Downloads all objects with this prefix.
        /// Examples: folder, path/to/folder
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string S3Directory { get; set; }

        /// <summary>
        /// String pattern to search files.
        /// Might not be exactly the same as in Windows.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string SearchPattern { get; set; }

        /// <summary>
        /// Directory to create folders and files to.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string DestinationPath { get; set; }
    }

    [DisplayName("Options")]
    public class DownloadOptions
    {
        /// <summary>
        /// Set to false to download files from current directory only.
        /// </summary>
        [DefaultValue(true)]
        public bool DownloadFromCurrentDirectoryOnly { get; set; }

        /// <summary>
        /// Set to true to move files.
        /// </summary>
        [DefaultValue(false)]
        public bool DeleteSourceFile { get; set; }

        /// <summary>
        /// Overwrite files.
        /// </summary>
        [DefaultValue(false)]
        public bool Overwrite { get; set; }

        /// <summary>
        /// If search pattern does not match, throw error.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoMatches { get; set; }
    }

    #endregion

    #region ListTask

    /// <summary>
    /// Input parameter class for ListObjectsAsync.
    /// </summary>
    [DisplayName("Input")]
    public class ListInput
    {
        /// <summary>
        /// Object prefix ( folder path ).
        /// Use this to set prefix for each file.
        /// Default: null
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string Prefix { get; set; }

        /// <summary>
        /// Delimiter.
        /// Use in conjuction with prefix to limit results to specific level of the flat namespace hierarchy.
        /// See: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string Delimiter { get; set; }

        /// <summary>
        /// Max number of keys returned.
        /// </summary>
        [DefaultValue(100)]
        [DisplayFormat(DataFormatString = "Expression")]
        public int MaxKeys { get; set; }

        /// <summary>
        /// A key to start the listing from.
        /// Default: null.
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string StartAfter { get; set; }

        /// <summary>
        /// If previous response is truncated, use the ContinuationToken from that response here, to continue listing.
        /// Default: null.
        /// </summary>
        [DefaultValue(null)]
        [DisplayFormat(DataFormatString = "Text")]
        public string ContinuationToken { get; set; }
    }

    /// <summary>
    /// Options-class.
    /// </summary>
    [DisplayName("Options")]
    public class ListOptions
    {
        /// <summary>
        /// True will return full response, false will return object keys only.
        /// </summary>
        [DefaultValue(false)]
        public bool FullResponse { get; set; }

        /// <summary>
        /// Throw error if reponse has no items in "S3Objects" array.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoFilesFound { get; set; }
    }

    #endregion

    #region UploadTask

    /// <summary>
    /// Input filepath and filemask.
    /// </summary>
    [DisplayName("Input")]
    public class UploadInput
    {
        /// <summary>
        /// Path to folder ( c:\temp\ , \\network\folder ).
        /// </summary>
        [DefaultValue(@"c:\temp\")]
        [DisplayFormat(DataFormatString = "Text")]
        public string FilePath { get; set; }

        /// <summary>
        /// Windows-style filemask, ( *.* , ?_file.*, foo_*.txt ).
        /// Empty field = all files (*).
        /// </summary>
        [DefaultValue(@"*")]
        [DisplayFormat(DataFormatString = "Text")]
        public string FileMask { get; set; }

        /// <summary>
        /// S3 root directory.
        /// If directory does not exist, it will be created.
        /// </summary>
        [DefaultValue(@"")]
        [DisplayFormat(DataFormatString = "Text")]
        public string S3Directory { get; set; }

        /// <summary>
        /// Enable/disable S3CannedACL selection, default value false.
        /// </summary>
        [DefaultValue("false")]
        public bool S3CannedACL { get; set; }

        /// <summary>
        /// S3CannedACL selection, default private.
        /// </summary>
        [DisplayName("S3CannedACL")]
        [UIHint(nameof(S3CannedACL), "", true)]
        public S3CannedACLs CannedACL { get; set; }
    }

    /// <summary>
    /// Task behaviour.
    /// Defaults work fine.
    /// </summary>
    [DisplayName("Options")]
    public class UploadOptions
    {
        /// <summary>
        /// Set to false to upload files from current directory only.
        /// Filemask must not be empty.
        /// </summary>
        [DefaultValue(true)]
        public bool UploadFromCurrentDirectoryOnly { get; set; }

        /// <summary>
        /// Set to true to create subdirectories in AWS.
        /// Works with UploadFromCurrentDirectoryOnly = false.
        /// </summary>
        [DefaultValue(false)]
        public bool PreserveFolderStructure { get; set; }

        /// <summary>
        /// Set to true to overwrite files with the same path and name (aka object key).
        /// </summary>
        [DefaultValue(false)]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Deletes local source files after transfer.
        /// </summary>
        [DefaultValue(false)]
        public bool DeleteSource { get; set; }

        /// <summary>
        /// If there are no files in the path matching the filemask supplied, throw error.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        /// If you wish, you can return object keys from S3 ( format: prefix/prefix/filename ).
        /// </summary>
        [DefaultValue(false)]
        public bool ReturnListOfObjectKeys { get; set; }
    }

    #endregion

    #region TempCredTask

    /// <summary>
    /// Input parameters for Temporary Credentials.
    /// </summary>
    public class TempCredInput
    {
        /// <summary>
        /// AWS Role parameter.
        /// Example: arn:aws:iam:::role/someRole.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("arn:aws:iam:::role/frends")]
        public string RoleArn { get; set; }

        /// <summary>
        /// External Id used to track requests.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("FRENDS")]
        public string CredentialExternalId { get; set; }

        /// <summary>
        /// External Id used to track requests.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("FRENDS_{{#process.executionid}}")]
        public string CredentialUniqueRequestId { get; set; }

        /// <summary>
        /// Credentials expire after this time (in seconds).
        /// Note: Minimum and maximum allowed expiration are set by AWS and S3 configuration.
        /// </summary>
        [DefaultValue(3600)]
        public int CredentialDurationSeconds { get; set; }
    }

    #endregion

    #region Parameters for all!

    /// <summary>
    /// Parameter class with username and keys.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// AWS Bucket name.
        /// Example: yourbucket.
        /// </summary>
        [DisplayName("Name of bucket")]
        public string BucketName { get; set; }

        /// <summary>
        /// Key name for Amazon S3 File transfer.
        /// </summary>
        [PasswordPropertyText(true)]
        [UIHint(nameof(UseDefaultCredentials), "", false)]
        [DisplayName("AWS Access Key ID")]
        [DisplayFormat(DataFormatString = "Expression")]
        public string AwsAccessKeyId { get; set; }

        /// <summary>
        /// Secret key name for Amazon S3 File transfer.
        /// </summary>
        [PasswordPropertyText(true)]
        [UIHint(nameof(UseDefaultCredentials), "", false)]
        [DisplayName("AWS Secret Access Key")]
        [DisplayFormat(DataFormatString = "Expression")]
        public string AwsSecretAccessKey { get; set; }

        /// <summary>
        /// Usage: The result of GetTemporaryCredentials-task.
        /// </summary>
        [UIHint(nameof(UseDefaultCredentials), "", false)]
        [DisplayName("Temporary Credentials")]
        public dynamic AwsCredentials { get; set; }

        /// <summary>
        /// Region selection, default EUWest1.
        /// </summary>
        [DisplayName("Region")]
        public Regions Region { get; set; }

        /// <summary>
        /// Credentials are loaded from the application's default configuration, and if unsuccessful from the Instance Profile service on an EC2 instance. 
        /// </summary>
        [DisplayName("Use Default Credentials")]
        public bool UseDefaultCredentials { get; set; }

        /// <summery>
        /// Usage: Throws exception if error occures in upload.
        /// </summery>
        [DefaultValue(false)]
        public bool ThrowExceptionOnErrorResponse { get; set; }
    }

    #endregion

    #region Enumerations

    public enum Regions
    {
        AfSouth1,
        ApEast1,
        ApNortheast1,
        ApNortheast2,
        ApNortheast3,
        ApSouth1,
        ApSoutheast1,
        ApSoutheast2,
        CaCentral1,
        CnNorth1,
        CnNorthWest1,
        EuCentral1,
        EuNorth1,
        EuSouth1,
        EuWest1,
        EuWest2,
        EuWest3,
        MeSouth1,
        SaEast1,
        UsEast1,
        UsEast2,
        UsWest1,
        UsWest2

    }

    public enum S3CannedACLs
    {
        Private,
        PublicRead,
        PublicReadWrite,
        AuthenticatedRead,
        BucketOwnerRead,
        BucketOwnerFullControl,
        LogDeliveryWrite
    }
    #endregion
}