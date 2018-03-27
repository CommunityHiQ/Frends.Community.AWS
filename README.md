# Frends.Community.AWS
Frends tasks to download, upload and list files for AWS S3 flat file storage.
***
- [Installing](#installing)
- [Tasks](#tasks)
  - [DownloadFiles](#downloadfiles)
    - [DownloadFiles Input](#downloadfiles-input)
    - [DownloadFiles Parameters](#downloadfiles-parameters)
	- [DownloadFiles Options](#downloadfiles-options)
    - [DownloadFiles Result](#downloadfiles-result)
  - [UploadAsync](#uploadasync)
    - [UploadAsync Input](#uploadasync-input)
    - [UploadAsync Parameters](#uploadasync-parameters)
    - [UploadAsync Options](#uploadasync-options)
    - [UploadAsync Result](#uploadasync-result)
  - [ListObjectsAsync](#listobjectsasync)
    - [ListObjectsAsync Input](#listobjectsasync-input)
    - [ListObjectsAsync Parameters](#listobjectsasync-parameters)
    - [ListObjectsAsync Options](#listobjectsasync-options)
    - [ListObjectsAsync Result](#listobjectsasync-result)
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

### DownloadFiles


#### DownloadFiles Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
SourceDirectory | string | S3 Directory | prefix, prefix/another
SearchPattern | string | Filemask to match files with | \*, \*.\*, \*test\*.csv
DestinationPath | string | Local path to download files to | c:\temp, \\\\network\path

#### DownloadFiles Parameters
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | String | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AWSAccessKeyID | String (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AWSSecretAccessKey | String (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1

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
### UploadAsync
Upload creates a list of files from FilePath and FileMask parameters, uploads them to the supplied bucket with Prefix-parameter
using the filenames it found.
You can append strings to prefix, for example DateTime.Now.
Otherwise, supply the prefix with trailing slash to prevent creating empty folders
#### UploadAsync Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
FilePath | string | Filepath to upload files from. | C:\upload, \\\\network\folder\
FileMask | string | Filename or wildcards (eg. *.txt) | \*.\*, filename.csv
Prefix | string | Prefix for object key. | folder/{{DateTime.Now}}

#### UploadAsync Parameters
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | String | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AWSAccessKeyID | String (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AWSSecretAccessKey | String (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1

#### UploadAsync Options
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
ThrowErrorIfNoMatch | Boolean | If no files match the FilePath and FileMask supplied, throw error. | true
ReturnListOfObjectKeys | Boolean | You can choose to return the keys uploaded or filenames with path uploaded. | true
StorageClass | Selector | Choose the type of storage for files. Read S3 documentation for further details. | Standard

#### UploadAsync Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | List\<string\> | List of file keys or filepaths. | c:\upload\file.csv, object/prefix/file.csv
***
### ListObjectsAsync
Lists files from S3. You can choose to return full response or just object keys.
Returns an JObject data structure. Keys are in ["S3Objects"]-array.
You can combine this task with DownloadAsync-task to get the keys you want to download.

#### ListObjectsAsync Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Prefix | String | Prefix to list files from. Leave empty for root. | "", prefix, prefix/key.csv
Delimiter | String | Limits the list to a character. Leave empty for all files. http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html. | "", /, /20001010/
MaxKeys | Integer | Limits the result list. | 100, 1, 99999999
StartAfter | String | Start listing after specified key (eg. date if filenames are organised with dates). Can be empty. | "", object/prefix/key
ContinuationToken | String | If list is truncated (eg. MaxKeys is reached), response contains ContinuationToken. You can use this token to resume list. Can be empty.| 1ueGcxLPRx1Tr/XYExHnhbYLgveDs2J/wm36HyEXAMPLE=

#### ListObjectsAsync Parameters
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | String | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AWSAccessKeyID | String (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AWSSecretAccessKey | String (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1

#### ListObjectsAsync Options
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
FullResponse | Boolean | Choose between list of files as JObject or full response as JObject. | true

#### ListObjectsAsync Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | JObject | List of file keys or full response with metadata. | JObject { JArray("S3Objects"), JProperty }
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

Run Tests with nunit3. Tests can be found under

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

| Version             | Changes                 |
| ---------------------| ---------------------|
| pre 1.0 | pending |
| 1.1.0 | Updated version as 1.0.0 has already been used in some environments. Fixed typos |
| 1.1.6 | Added feature to move files (deletes sources) to DownloadTask, better error messages. |
| 1.1.7 | Removed Frends.Task.Attributes, using DataAnnotations instead |