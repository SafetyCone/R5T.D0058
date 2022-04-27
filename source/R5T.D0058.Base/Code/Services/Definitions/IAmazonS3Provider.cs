using System;
using System.Threading.Tasks;

using Amazon.S3;

using R5T.T0064;


namespace R5T.D0058
{
    [ServiceDefinitionMarker]
    public interface IAmazonS3Provider : IServiceDefinition
    {
        Task<IAmazonS3> GetS3();
    }
}
