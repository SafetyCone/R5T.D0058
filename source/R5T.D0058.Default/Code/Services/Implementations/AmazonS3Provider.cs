using System;
using System.Threading.Tasks;

using Amazon.Runtime;
using Amazon.S3;

using R5T.D0057;


namespace R5T.D0058
{
    public class AmazonS3Provider : IAmazonS3Provider
    {
        private AWSCredentials AWSCredentials { get; }
        private IRegionEndpointProvider RegionEndpointProvider { get; }


        public AmazonS3Provider(
            AWSCredentials aWSCredentials,
            IRegionEndpointProvider awsRegionEndpointProvider)
        {
            this.AWSCredentials = aWSCredentials;
            this.RegionEndpointProvider = awsRegionEndpointProvider;
        }

        public async Task<IAmazonS3> GetS3()
        {
            var awsRegionEndpoint = await this.RegionEndpointProvider.GetRegionEndpoint();

            var s3Client = new AmazonS3Client(this.AWSCredentials, awsRegionEndpoint);
            return s3Client;
        }
    }
}
