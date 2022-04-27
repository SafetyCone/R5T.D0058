using System;
using System.Threading.Tasks;

using R5T.T0064;


namespace R5T.D0058
{
    [ServiceDefinitionMarker]
    public interface IBucketNameProvider : IServiceDefinition
    {
        Task<BucketName> GetBucketName();
    }
}
