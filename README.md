- [Installing](#installing)
- [Building from source](#building-from-source)
- [Tasks](#tasks)
    - [DownloadAsync](#downloadasync)
        - [Input](#input)
    	 - [Parameters](#parameters)
    	 - [Options](#options)
    	 - [Result](#result)
      - [UploadAsync](#uploadasync)
    	 - [Input](#input)
    	 - [Parameters](#parameters)
    	 - [Options](#options)
    	 - [Result](#result)
     - [ListObjects](#listobjects)
    	 - [Input](#input)
    	 - [Parameters](#parameters)
    	 - [Options](#options)
    	 - [Result](#result)
- [Building](#building)
- [Contributing](#contributing)
- [License](#license)

## Installing
You can install the task via FRENDS UI Task View or you can find the nuget package from the following nuget feed
'[https://www.myget.org/F/frends/api/v2](https://www.myget.org/F/frends/api/v2)'

## Building from source
Download source from GitHub and restore external AWS API-library nuget references, build solution and create nuget package using nuspec-file from Release binaries. Upload nuget package to FRENDS4-solution.

## Tasks

### DownloadAsync
Task downloads files from AWS S3. Full directory download gets all files after the same prefix.
To individually download multiple files, you need to run task in a foreach-loop and provide filenames exactly. Use ListObjects-task to provide objects keys.
**Does not support wildcard characters, such as '*'.** S3 is a flat filesystem.

#### Input
Property | Type | Description | Example
---------|------|-------------|--------
DownloadWholeDirectory | Boolean | Download all files behind the prefix. |
SourceDirectory| String | Prefix for the files, only visible when DownloadWholeDirectory is true. |
SourcePrefixAndFilename | String | Prefix and filename, only visible when DownloadWholeDirectory is false. |
DestinationPath | String | Location to save file to, only visible when DownloadWholeDirectory is true. |
DestinationPathAndFilename | String | Destination path with filename, only visible when DownloadWholeDirectory is false. |

#### Parameters
Property | Type | Description | Example
---------|------|-------------|--------
BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. |
AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
Region | Selector | Bucket region, select from dropdown-list. |

#### Result
Property | Type | Description | Example
---------|------|-------------|--------
#result | List<string> | List of filepaths to downloaded files. |

### Upload

#### Input
Property | Type | Description | Example
---------|------|-------------|--------
FilePath | String | Filepath to upload files from. |
FileMask | String | Filename or wildcards (eg. *.txt) |
Prefix | String | Prefix for object key.  |

#### Parameters
Property | Type | Description | Example
---------|------|-------------|--------
BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. |
AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
Region | Selector | Bucket region, select from dropdown-list. |

#### Options
Property | Type | Description | Example
---------|------|-------------|--------
ThrowErrorIfNoMatch | Boolean | If no files match the FilePath and FileMask supplied, throw error. |
ReturnListOfObjectKeys | Boolean | You can choose to return the keys uploaded or filenames with path uploaded. |
StorageClass | Selector | Choose the type of storage for files. Read S3 documentation for further details. |

#### Result
Property | Type | Description | Example
---------|------|-------------|--------
#result | List<string> | List of file keys or filepaths. |

### Listing
Lists files from S3. You can choose to return full response or just keys.
This task was created to provide Download-task keys to dowlonad.

#### Input
Prefix | String | Prefix to list files from. |
Delimiter | String | Limits the list to a character. See AWS S3 documents for further details on usage. |
MaxKeys | Integer | Limits the result list. |
StartAfter | String | Start listing after specified key (eg. date if filenames are organised with dates). |
ContinuationToken | String | If list is truncated (MaxKeys), response contains ContinuationToken. You can use this token to resume listing. |
#### Parameters
Property | Type | Description | Example
---------|------|-------------|--------
BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. |
AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
Region | Selector | Bucket region, select from dropdown-list. |

#### Options
Property | Type | Description | Example
---------|------|-------------|--------
FullResponse | Boolean | Choose between list of files as JObject or full response as JObject. |

#### Result
Property | Type | Description | Example
---------|------|-------------|--------
#result | JObject | List of file keys or full response with metadata. |

## Documentation
Task downloads files from AWS S3. Full directory download gets all files behind the same prefix.
To individually download files, you need to run task in a foreach-loop and provide filenames exactly.
Does not support wildcard characters such as '*'. S3 is a flat filesystem.

#### Input
Property | Type | Description | Example
---------|------|-------------|--------
DownloadWholeDirectory | Boolean | Download all files behind the prefix. |
SourceDirectory| String | Prefix for the files, only visible when DownloadWholeDirectory is true. |
SourcePrefixAndFilename | String | Prefix and filename, only visible when DownloadWholeDirectory is false. |
DestinationPath | String | Location to save file to, only visible when DownloadWholeDirectory is true. |
DestinationPathAndFilename | String | Destination path with filename, only visible when DownloadWholeDirectory is false. |

#### Parameters
Property | Type | Description | Example
---------|------|-------------|--------
BucketName | Expression | S3 Buckets name, #env-variable use is encouraged.
AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged.
AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged.
Region | Selector | Bucket region, select from dropdown-list.

#### Result
Property | Type | Description | Example
---------|------|-------------|--------
#result | List<string> | List of filepaths to downloaded files. | 

## License
MIT License.