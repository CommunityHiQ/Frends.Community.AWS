﻿using System;
using System.ComponentModel;
using Frends.Tasks.Attributes;

namespace Frends.Community.AWS
{
    #region Download
    /// <summary>
    ///     Input class, you can download whole directories or single files.
    /// </summary>
    [DisplayName("Input")]
    public class DownloadInput
    {
        /// <summary>
        ///     Uses different method to download. Whole directory gets all objects recursively.
        /// </summary>
        
        public Boolean DownloadWholeDirectory { get; set; }

        /// <summary>
        ///     Downloads ALL objects with this prefix. Creates folder structure.
        ///     Examples: folder/, this/is/prefix/
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string SourceDirectory { get; set; }

        /// <summary>
        ///     Downloads single object (file).
        ///     Example: folder/file.txt, this/is/prefix/file
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string SourcePrefixAndFilename { get; set; }

        /// <summary>
        ///     Directory to create folders and files to.
        ///     Use trailing backlash ( \ ).
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string DestinationPath { get; set; }

        /// <summary>
        ///     Folder to write file.
        ///     You can use different filename.
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string DestinationPathAndFilename { get; set; }
    }
    #endregion

    #region List
    [DisplayName("Input")]
    public class ListInput
    {
        /// <summary>
        ///     Object prefix ( folder path ).
        ///     Use this to set prefix for each file.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }

        /// <summary>
        ///     Delimiter.
        ///     Use in conjuction with prefix to limit results to specific level of the flat namespace hierarchy.
        ///     See: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Delimiter { get; set; }

        /// <summary>
        ///     Max number of keys returned.
        /// </summary>
        [DefaultValue(100)]
        [DefaultDisplayType(DisplayType.Expression)]
        public int MaxKeys { get; set; }

        /// <summary>
        ///     A key to start the listing from.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string StartAfter { get; set; }

        /// <summary>
        ///     If previous response is truncated, use the ContinuationToken from that response here, to continue listing.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
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
    }
    #endregion

    #region Upload
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
        [DefaultDisplayType(DisplayType.Text)]
        public string FilePath { get; set; }

        /// <summary>
        ///     Windows-style filemask, ( *.* , ?_file.*, foo_*.txt ).
        ///     Empty field = all files.
        /// </summary>
        [DefaultValue(@"*.*")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FileMask { get; set; }
        /// <summary>
        ///     S3 prefix for files.
        /// </summary>
        [DefaultValue(@"\")]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }
    }

    /// <summary>
    ///     Task behaviour.
    ///     Defaults work fine.
    /// </summary>
    [DisplayName("Options")]
    public class UploadOptions
    {
        /// <summary>
        ///     If there are no files in the path matching the filemask supplied,
        ///     throw error to fail the process.
        /// </summary>
        [DefaultValue(true)]
        public Boolean ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        ///     If you wish, you can return object keys from S3
        ///     ( format: bucket/prefix/prefix/filename.extension )
        /// </summary>
        [DefaultValue(false)]
        public Boolean ReturnListOfObjectKeys { get; set; }

        /// <summary>
        ///     You can specify Storage Class for uploaded files.
        ///     Standard is default.
        ///     Consult AWS S3 Documentation for others.
        /// </summary>
        public StorageClasses StorageClass { get; set; }
    }
    #endregion

    #region Parameters for all!
    /// <summary>
    ///     Parameter class with username and keys.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        ///     AWS Bucket name with Path
        ///     Example: bucketname/path/to/directory or #env.variable.
        /// </summary>
        [DefaultDisplayType(DisplayType.Text)]
        public string BucketName { get; set; }

        /// <summary>
        ///     Key name for Amazon s3 File transfer aws_access_key_id
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSAccessKeyID { get; set; }

        /// <summary>
        ///     Secret  key name for Amazon s3 File transfer aws_secret_access_key
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSSecretAccessKey { get; set; }

        /// <summary>
        ///     Region selection, default EUWest1.
        /// </summary>
        [DisplayName("Region")]
        public Regions Region { get; set; }
    }
    #endregion

    #region Enumerations

    #pragma warning disable 1591
    public enum Regions
    {
        EUWest1, EUWest2, EUCentral1,
        APNortheast1, APNortheast2, APSouth1, APSoutheast1, APSoutheast2,
        CACentral1, CNNorth1, SAEast1,
        USEast1, USEast2, USGovCloudWest1, USWest1, USWest2
    }
    public enum StorageClasses
    {
        Standard, StandardInfrequent, Reduced, Glacier
    }
    #pragma warning restore 1591
    #endregion
}