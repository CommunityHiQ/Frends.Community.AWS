- [Frends.Community.AWS](#frendscommunityaws)
   - [Installing](#installing)
   - [Building](#building)
   - [Contributing](#contributing)
   - [License](#license)
       
# Frends.Community.AWS
FRENDS tasks related to transferring files to and from AWS.
See individual README.md for task specific documentation.

## Installing
See [Building](#building).

## Building
Download source from GitHub and restore external AWS API-library nuget references, build solution and create nuget packagage using nuspec-file. Upload nuget package to FRENDS4-solution.

## Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

## License
MIT License

- [FRENDS.Community.AWS](#frends.community.aws)
   - [Installing](#installing)
   - [Building](#building)
   - [Contributing](#contributing)
   - [Documentation](#documentation)
      - [upload](#upload)
		 - [Input](#input)
		 - [Parameters](#parameters)
		 - [Options](#options)
		 - [Result](#result)
   - [License](#license)
       
# FRENDS.Community.AWS.Download
This repository contais three separate FRENDS4 tasks, customized for this use case.

## Installing
See [Building](#building).

## Building
Download source from GitLab, create nuget packagage using nuspec-file. Upload nuget package to FRENDS4-solution.

## Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

## Documentation
Task downloads files from AWS S3. Full directory download gets all files behind the same prefix.
To individually download files, you need to run task in a foreach-loop and provide filenames exactly.
Does not support wildcard characters such as '*'. S3 is a flat filesystem.

### Upload

#### Input
| Property | Type | Description |
|----------|------|-------------|
| FilePath | String | Filepath to upload files from. |
| FileMask | String | Filename or wildcards (eg. *.txt) |

#### Parameters
| Property | Type | Description |
|----------|------|-------------|
| BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. |
| AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
| AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
| Region | Selector | Bucket region, select from dropdown-list. |
| Prefix | String | Prefix to list files from. |

#### Options
| Property | Type | Description |
|----------|------|-------------|
| ThrowErrorIfNoMatch | Boolean | If no files match the FilePath and FileMask supplied, throw error. |
| ReturnListOfObjectKeys | Boolean | You can choose to return the keys uploaded or filenames with path uploaded. |
| StorageClass | Selector | Choose the type of storage for files. Read S3 documentation for further details. |

#### Result
| Property | Type | Description |
|----------|------|-------------|
| #result | List<string> | List of file keys or filepaths. |

## License
MIT License.


- [FRENDS.Community.AWS](#frends.community.aws)
   - [Installing](#installing)
   - [Building](#building)
   - [Contributing](#contributing)
   - [Documentation](#documentation)
      - [Listing](#listing)
		 - [Parameters](#parameters)
		 - [Options](#options)
		 - [Result](#result)
   - [License](#license)
       
# FRENDS.Community.AWS.Download
This repository contais three separate FRENDS4 tasks, customized for this use case.

## Installing
See [Building](#building).

## Building
Download source from GitLab, create nuget packagage using nuspec-file. Upload nuget package to FRENDS4-solution.

## Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

## Documentation
Task downloads files from AWS S3. Full directory download gets all files behind the same prefix.
To individually download files, you need to run task in a foreach-loop and provide filenames exactly.
Does not support wildcard characters such as '*'. S3 is a flat filesystem.
Lists files from S3. You can choose between full response or just keys.
This task was created to provide Download-task keys to dowlonad.

### Listing

#### Parameters
| Property | Type | Description |
|----------|------|-------------|
| BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. |
| AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
| AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
| Region | Selector | Bucket region, select from dropdown-list. |
| Prefix | String | Prefix to list files from. |
| Delimiter | String | Limits the list to a character. See AWS S3 documents for further details on usage. |
| MaxKeys | Integer | Limits the result list. |
| StartAfter | String | Start listing after specified key (eg. date if filenames are organised with dates). |
| ContinuationToken | String | If list is truncated (MaxKeys), response contains ContinuationToken. You can use this token to resume listing. |


#### Options
| Property | Type | Description |
|----------|------|-------------|
| FullResponse | Boolean | Choose between list of files as JObject or full response as JObject. |

#### Result
| Property | Type | Description |
|----------|------|-------------|
| #result | JObject | List of file keys or full response with metadata. |

## License
MIT License.


- [FRENDS.Community.AWS](#frends.community.aws)
   - [Installing](#installing)
   - [Building](#building)
   - [Contributing](#contributing)
   - [Documentation](#documentation)
      - [Listing](#listing)
		 - [Input](#input)
		 - [Parameters](#parameters)
		 - [Result](#result)
   - [License](#license)
       
# FRENDS.Community.AWS.Download
This repository contais three separate FRENDS4 tasks, customized for this use case.

## Installing
See [Building](#building).

## Building
Download source from GitLab, create nuget packagage using nuspec-file. Upload nuget package to FRENDS4-solution.

## Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

## Documentation
Task downloads files from AWS S3. Full directory download gets all files behind the same prefix.
To individually download files, you need to run task in a foreach-loop and provide filenames exactly.
Does not support wildcard characters such as '*'. S3 is a flat filesystem.

### Listing

Uses Microsoft.Exchange.WebServices.Data to fetch emails from O365 Inbox.

#### Input
| Property | Type | Description |
|----------|------|-------------|
| DownloadWholeDirectory | Boolean | Download all files behind the prefix. |
| SourceDirectory| String | Prefix for the files, only visible when DownloadWholeDirectory is true. |
| SourcePrefixAndFilename | String | Prefix and filename, only visible when DownloadWholeDirectory is false. |
| DestinationPath | String | Location to save file to, only visible when DownloadWholeDirectory is true. |
| DestinationPathAndFilename | String | Destination path with filename, only visible when DownloadWholeDirectory is false. |

#### Parameters
| Property | Type | Description |
|----------|------|-------------|
| BucketName | Expression | S3 Buckets name, #env-variable use is encouraged. |
| AWSAccessKeyID | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
| AWSSecretAccessKey | Expression (secret) | S3 Access Key, #env-variable use is encouraged. |
| Region | Selector | Bucket region, select from dropdown-list. |

#### Result
| Property | Type | Description |
|----------|------|-------------|
| #result | List<string> | List of filepaths to downloaded files. |

## License
MIT License.