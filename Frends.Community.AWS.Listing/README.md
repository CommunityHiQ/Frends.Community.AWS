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