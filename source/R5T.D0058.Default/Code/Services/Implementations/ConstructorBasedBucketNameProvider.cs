using System;
using System.Threading.Tasks;

using R5T.T0064;


namespace R5T.D0058
{
    [ServiceImplementationMarker]
    public class ConstructorBasedBucketNameProvider : IBucketNameProvider, IServiceImplementation
    {
        private BucketName BucketName { get; }


        [ServiceImplementationConstructorMarker]
        public ConstructorBasedBucketNameProvider(
            [NotServiceComponent] BucketName bucketName)
        {
            this.BucketName = bucketName;
        }

        public ConstructorBasedBucketNameProvider(string bucketNameValue)
            : this(BucketName.From(bucketNameValue))
        {
        }

        public Task<BucketName> GetBucketName()
        {
            return Task.FromResult(this.BucketName);
        }
    }
}
