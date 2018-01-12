# Frends.Community.AWS
Frends tasks to download, upload and list files for AWS S3 flat file storage.
***
- [Installing](#installing)
- [Tasks](#tasks)
  - [DownloadAsync](#downloadasync)
    - [DownloadAsync Input](#downloadasync-input)
    - [DownloadAsync Parameters](#downloadasync-parameters)
    - [DownloadAsync Result](#downloadasync-result)
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

### DownloadAsync
Task downloads files from AWS S3. Full directory download gets all files after the same prefix.
To individually download multiple files, you need to run task in a loop and provide object keys exactly.
You can use ListObjectsAsync-task to provide objects keys.
**Does not support wildcard characters, such as '*'.** S3 is a flat filesystem.
Using "/" as prefix downloads everything from root, keeping folder structure (assuming "/" is used as delimiter).

It is highly encouraged to design your S3 filing structure around the flat file schema.

#### DownloadAsync Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
DownloadWholeDirectory | Boolean | Download all files behind the prefix. | true
SourcePrefix| String | Prefix for the files, only visible when DownloadWholeDirectory is **true**. | /, /object, /object/sub
DestinationPath | String | Location to save file to, only visible when DownloadWholeDirectory is **true**. | C:\download\
SourcePrefixAndKey | String | Prefix and filename, only visible when DownloadWholeDirectory is **false**. | /object.key, /object/file.txt
DestinationPathAndFilename | String | Destination path with filename, only visible when DownloadWholeDirectory is **false**. | C:\download\filename.txt, "C:\download\" + DateTime.Now + ".txt"

#### DownloadAsync Parameters
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | String | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AWSAccessKeyID | String (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AWSSecretAccessKey | String (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1

#### DownloadAsync Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | List<string> | List of filepaths to downloaded files. | c:\download\file.csv
***
### UploadAsync
Upload creates a list of files from FilePath and FileMask parameters, uploads them to the supplied bucket with Prefix-parameter
using the filenames it found.
You can append strings to prefix, for example DateTime.Now.
Otherwise, supply the prefix with trailing slash to prevent creating empty folders
#### UploadAsync Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
FilePath | String | Filepath to upload files from. | C:\upload, \\network\folder\
FileMask | String | Filename or wildcards (eg. *.txt) | *.*, filename.csv
Prefix | String | Prefix for object key.  | folder/{{DateTime.now}}

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
Result | List<string> | List of file keys or filepaths. | c:\upload\file.csv, s3-bucket/object/prefix/file.csv
***
### ListObjectsAsync
Lists files from S3. You can choose to return full response or just object keys.
Returns an JObject data structure. Keys are in ["S3Objects"]-array.
You can combine this task with DownloadAsync-task to get the keys you want to download.

#### ListObjectsAsync Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Prefix | String | Prefix to list files from. | object/prefix/, object/prefix/key.csv /
Delimiter | String | Limits the list to a character. See AWS S3 documents for further details on usage. | /, /20001010/
MaxKeys | Integer | Limits the result list. | 100, 1, 99999999
StartAfter | String | Start listing after specified key (eg. date if filenames are organised with dates). | object/prefix/key
ContinuationToken | String | If list is truncated (MaxKeys), response contains ContinuationToken. You can use this token to resume listing. | 1ueGcxLPRx1Tr/XYExHnhbYLgveDs2J/wm36Hy4vbOwM=

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
Result | JObject | List of file keys or full response with metadata. | JObect { JArray("S3Objects"), JProperty }
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

`Frends.Community.PDFWriter.Tests\bin\Release\Frends.Community.AWS.Tests.dll`

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