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