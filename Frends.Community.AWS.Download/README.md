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