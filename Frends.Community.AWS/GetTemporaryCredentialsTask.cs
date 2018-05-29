﻿using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Gets temporary credentials.
    /// </summary>
    public class GetTemporaryCredentialsTask
    {
        /// <summary>
        ///     You can use the result of this task as Parameter
        ///     for other AWS related tasks in the same process
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Credentials</returns>
        public static async Task<Credentials> GetTemporaryCredentialsAsync(
            [PropertyTab] TempCredInput input,
            [PropertyTab] Parameters parameters,
            CancellationToken cancellationToken)
        {
            input.IsAnyNullOrWhiteSpaceThrow();

            using (var stsClient = new AmazonSecurityTokenServiceClient(
                parameters.AwsAccessKeyId,
                parameters.AwsSecretAccessKey,
                Utilities.RegionSelection(parameters.Region)))
            {
                var assumeRoleRequest = new AssumeRoleRequest
                {
                    DurationSeconds = input.CredentialDurationSeconds,
                    ExternalId = input.CredentialExternalId,
                    RoleArn = input.RoleArn,
                    RoleSessionName = input.CredentialUniqueRequestId
                };

                return (await stsClient.AssumeRoleAsync(assumeRoleRequest, cancellationToken)).Credentials;
            }
        }
    }
}