- [Installing](#installing)
- [Building from source](#building-from-source)
- [Tasks](#tasks)
  - [DownloadAsync](#downloadasync)
    - [DownloadAsync Input](#download-input)
    - [DownloadAsync Parameters](#download-parameters)
    - [DownloadAsync Result](#download-result)
  - [UploadAsync](#upload-async)
    - [UploadAsync Input](#upload-input)
    - [UploadAsync Parameters](#upload-parameters)
    - [UploadAsync Options](#upload-options)
    - [UploadAsync Result](#upload-result)
  - [ListObjectsAsync](#listobjectsasync)
    - [ListObjectsAsync Input](#listobjectsasync-input)
    - [ListObjectsAsync Parameters](#listobjectsasync-parameters)
    - [ListObjectsAsync Options](#listobjectsasync-options)
    - [ListObjectsAsync Result](#listobjectsasync-result)
- [Building](#building)
- [Contributing](#contributing)
- [License](#license)

***
## Installing
You can install the task via FRENDS UI Task View or you can find the nuget package from the following nuget feed
'[https://www.myget.org/F/frends/api/v2](https://www.myget.org/F/frends/api/v2)'
***
## Building from source
Download source from GitHub and restore external AWS API-library nuget references, build solution and create nuget package using nuspec-file from Release binaries. Upload nuget package to FRENDS4-solution.
***
## Tasks

### DownloadAsync
Task downloads files from AWS S3. Full directory download gets all files after the same prefix.
To individually download multiple files, you need to run task in a loop and provide object keys exactly.
You can use ListObjectsAsync-task to provide objects keys.
**Does not support wildcard characters, such as '*'.** S3 is a flat filesystem.

It is highly encouraged to design your S3 filing structure around the flat file schema.

#### Download Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
DownloadWholeDirectory | Boolean | Download all files behind the prefix. | true
SourcePrefix| String | Prefix for the files, only visible when DownloadWholeDirectory is **true**. | /, object/, object/sub/
DestinationPath | String | Location to save file to, only visible when DownloadWholeDirectory is **true**. | C:\download\
SourcePrefixAndKey | String | Prefix and filename, only visible when DownloadWholeDirectory is **false**. | /object.key, /object/file.txt
DestinationPathAndFilename | String | Destination path with filename, only visible when DownloadWholeDirectory is **false**. | C:\download\filename.txt, "C:\download\" + DateTime.now + ".txt"

#### Download Parameters
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1

#### Download Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | List<string> | List of filepaths to downloaded files. | c:\download\file.csv
***
### UploadAsync
Upload 
#### Upload Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
FilePath | String | Filepath to upload files from. | C:\upload, \\network\folder\
FileMask | String | Filename or wildcards (eg. *.txt) | *.*, filename.csv
Prefix | String | Prefix for object key.  | folder/{{DateTime.now}}

#### Upload Parameters
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1

#### Upload Options
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
ThrowErrorIfNoMatch | Boolean | If no files match the FilePath and FileMask supplied, throw error. | true
ReturnListOfObjectKeys | Boolean | You can choose to return the keys uploaded or filenames with path uploaded. | true
StorageClass | Selector | Choose the type of storage for files. Read S3 documentation for further details. | Standard

#### Upload Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | List<string> | List of file keys or filepaths. | c:\upload\file.csv, s3-bucket/object/prefix/file.csv
***
### ListObjects
Lists files from S3. You can choose to return full response or just object keys.
Returns an JObject data structure. Keys are in ["S3Objects"]-array.
You can combine this task with DownloadAsync-task to get the keys you want to download.

#### ListObjects Input
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Prefix | String | Prefix to list files from. | object/prefix/, object/prefix/key.csv /
Delimiter | String | Limits the list to a character. See AWS S3 documents for further details on usage. | /, /20001010/
MaxKeys | Integer | Limits the result list. | 100, 1, 99999999
StartAfter | String | Start listing after specified key (eg. date if filenames are organised with dates). | object/prefix/key
ContinuationToken | String | If list is truncated (MaxKeys), response contains ContinuationToken. You can use this token to resume listing. | 1ueGcxLPRx1Tr/XYExHnhbYLgveDs2J/wm36Hy4vbOwM=

#### ListObjects Parameters
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. | s3-bucket
AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. | AKIAIOSFODNN7EXAMPLE
AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. | wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Region | Selector | Location for S3 bucket, select from dropdown-list. | EUWest1

#### ListObjects Options
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
FullResponse | Boolean | Choose between list of files as JObject or full response as JObject. | true

#### ListObjects Result
Property | Type | Description | Example (comma separated)
---------|------|-------------|--------
Result | JObject | List of file keys or full response with metadata. |
***
## License
MIT License.