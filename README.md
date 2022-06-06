# Frends.Community.AWS

[![Actions Status](https://github.com/CommunityHiQ/Frends.Community.AWS/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.AWS/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.AWS) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

Frends tasks to download, upload and list files for AWS S3 flat file storage.
***
- [Installing](#installing)
- [Tasks](#tasks)
  - [Parameters](#parameters)
  - [DownloadFiles](#downloadfiles)
    - [DownloadFiles Input](#downloadfiles-input)
	- [DownloadFiles Options](#downloadfiles-options)
    - [DownloadFiles Result](#downloadfiles-result)
  - [UploadFiles](#uploadfiles)
    - [UploadFiles Input](#uploadfiles-input)
    - [UploadFiles Options](#uploadfiles-options)
    - [UploadFiles Result](#uploadfiles-result)
  - [ListObjectsAsync](#listobjectsasync)
    - [ListObjectsAsync Input](#listobjectsasync-input)
    - [ListObjectsAsync Options](#listobjectsasync-options)
    - [ListObjectsAsync Result](#listobjectsasync-result)
  - [GetTemporaryCredentialsAsync](#gettemporarycredentialsasync)
	- [GetTemporaryCredentialsAsync Input](#gettemporarycredentialsasync-input)
	- [GetTemporaryCredentialsAsync Result](#gettemporarycredentialsasync-result)
- [License](#license)
- [Building from source](#building-from-source)
- [Contributing](#contributing)
- [Changelog](#changelog)

***
## Installing
You can install the task via FRENDS UI Task View or you can find the nuget package from the following nuget feed
'[https://www.myget.org/F/frends/api/v2](https://www.myget.org/F/frends/api/v2)'
***
## Tasks

### Parameters
All tasks use the same Parameters-tab.
If AwsCredentials is set, AwsAccessKeyId and AwsSecretAccessKey are ignored.

Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | String | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AwsAccessKeyId | String (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AwsSecretAccessKey | String (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
AwsCredentials | dynamic | Used ONLY for GetTemporaryCredentialsTask result. If set, other keys are ignored. | #result[Get Temporary Credentials]
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1
UseDefaultCredentials | bool | If set to true, credentials are loaded from the application's default configuration, and if unsuccessful from the Instance Profile service on an EC2 instance.  | false
ThrowExceptionOnErrorResponse | bool | If set true, task throws error when upload was not successful | true
***
### DownloadFiles
Simulates Windows-style folder structure. Can download subdirectories.

#### DownloadFiles Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
SourceDirectory | string | S3 Directory | prefix, prefix/another
SearchPattern | string | Filemask to match files with | \*, \*.\*, \*test\*.csv
DestinationPath | string | Local path to download files to | c:\temp, \\\\network\path

#### DownloadFiles Options
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
DownloadFromCurrentDirectoryOnly | bool | To download from subdirectories, set to false. | true
DeleteSourceFile | bool | If set to true, moves files from S3 to local (deletes sourcefiles) | false
Overwrite | bool | If set to true, overwrites local files. | false
ThrowErrorIfNoMatches | bool | If search pattern does not find match any files, throw error. | true

#### DownloadFiles Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | List\<string\> | List of filepaths to downloaded files. | c:\download\file.csv
***
### UploadFiles
Upload gets files based on directory and filemask, uploads them to S3 using the same filename to a specific directory.
Optionally moves instead of copy, can do recursive matching and can preserver folder structure.

#### UploadFiles Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
FilePath | string | Filepath to upload files from. | C:\upload, \\\\network\folder\
FileMask | string | Filename or wildcards (eg. *.txt) | \*.\*, filename.csv
S3Directory | string | Root directory in S3. | folder/{{DateTime.Now}}
S3CannedACL | bool | Enable S3CannedACL selection | false
CannedACL | Selector | Selection of S3CannedACL permissions, visible if S3CannedACL enabled | Private

#### UploadFiles Options
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
UploadFromCurrentDirectoryOnly | Boolean | Set to false to search files from subdirectories. | true
PreserveFolderStructure | Boolean | If used in conjunction with UploadFromCurrentDirectoryOnly, subdirectories will be created. | false
Overwrite | Boolean | Overwrites files in S3. | false
DeleteSource | Boolean | Deletes local files after transfer | false
ThrowErrorIfNoMatch | Boolean | If no files match the FilePath and FileMask supplied, throw error. | true
ReturnListOfObjectKeys | Boolean | You can choose to return the keys uploaded or filenames with path uploaded. | true

#### UploadFiles Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | List\<string\> | List of file keys or filepaths. | c:\upload\file.csv, object/prefix/file.csv
***
### ListObjectsAsync
Lists files from S3. You can choose to return full response or just object keys.
Returns an JObject data structure. Keys are in ["S3Objects"]-array.

#### ListObjectsAsync Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Prefix | String | Prefix to list files from. Leave empty for root. | "", prefix, prefix/key.csv
Delimiter | String | Limits the list to a character. Leave empty for all files. http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html. | "", /, /20001010/
MaxKeys | Integer | Limits the result list. | 100, 1, 99999999
StartAfter | String | Start listing after specified key (eg. date if filenames are organised with dates). Can be empty. | "", object/prefix/key
ContinuationToken | String | If list is truncated (eg. MaxKeys is reached), response contains ContinuationToken. You can use this token to resume list. Can be empty.| 1ueGcxLPRx1Tr/XYExHnhbYLgveDs2J/wm36HyEXAMPLE=

#### ListObjectsAsync Options
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
FullResponse | Boolean | Choose between list of files as JObject or full response as JObject. | true

#### ListObjectsAsync Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | JObject | List of file keys or full response with metadata. | JObject { JArray("S3Objects"), JProperty }
***
### GetTemporaryCredentialsAsync
Gets temporary credentials. All fields must be set.
The result of this task is Credentials-object, that contains AccessKey, SecretAccessKey, Token and Duration.
You can use the result as a parameter for the other Tasks in Parameters/Credentials-field.

#### GetTemporaryCredentialsAsync Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
RoleArn | String | Role, which to assume.| arn:aws:iam::123456789012:role/AssumerRole
CredentialExternalId | String | Predetermined id for 3rd parties. | Foobar
CredentialUniqueRequestId | String | ID used to track requests in AWS | Foobar_Request_{GUID}
CredentialDurationSeconds | Int | How long the credentials last. Check AWS documentation and AWS configuration for min/max values. | 3600

#### GetTemporaryCredentialsAsync Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | Dynamic | Object. Can be used directly in the AwsCredentials field in Parameters tab. | Credentials { AccessKeyId, SecretAccessKey, Expiration, SessionToken }
***
## License
MIT License.
***
## Building from source

Clone a copy of the repo

`git clone https://github.com/CommunityHiQ/Frends.Community.AWS.git`

Restore dependencies

`nuget restore frends.community.aws`

Rebuild the project

(OPTIONAL)
Run Tests with nunit3. Tests can be found under
Integration tests require working S3 Bucket in a config.json fi

`Frends.Community.AWS.Tests\bin\Release\Frends.Community.AWS.Tests.dll`

Create a nuget package

`nuget pack nuspec/Frends.Community.AWS.nuspec`

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Changelog

| Version | Changes                                                                                                                                                  |
|---------|----------------------------------------------------------------------------------------------------------------------------------------------------------|
| pre 1.0 | pending                                                                                                                                                  |
| 1.1.0   | Updated version as 1.0.0 has already been used in some environments. Fixed typos.                                                                        |
| 1.1.6   | Added feature to move files (deletes sources) to DownloadTask, better error messages.                                                                    |
| 1.1.7   | Removed Frends.Task.Attributes, using DataAnnotations instead.                                                                                           |
| 1.1.8   | Proper tests.                                                                                                                                            |
| 1.2.0   | Rewrote Upload-task for additional features and increased ease of use. No more async.                                                                    |
| 1.2.7   | Added GetTemporaryCredentials task. Added Credentials-field to Parameters. It accepts the result of GetTemporaryCredentials task.                        |
| 1.3.2   | Now it is possible to call GetTemporaryCredentials also without AwsSecretAccessKey and AwsAccessKeyId. GetTemporaryCrednetials returns now dynamic.      |
| 1.3.3   | Documentation update and relocating nuspec file.                                                                                                         |
| 1.3.4   | Added new and removed old RegionEndpoints. Updated SDK.                                                                                                  |
| 1.3.6   | Multitarget support (.Net Standard 2.0 and .Net Framework 4.7.1                                                                                          |
| 1.3.8   | Added UseDefaultCredentials option.                                                                                                                      |
| 1.3.9   | Added parameter ThrowExceptionOnErrorResponse, fixed issue: invalid credentials and unsuccessful upload threw no exception and added TestData for tests. | 
| 1.3.10  | Added input parameter Canned Acl to change the ACL restriction of the uploaded object. Also Added boolean value enabling the cannedAcl.                  |
| 1.3.11  | SDK update.                  																															 |
