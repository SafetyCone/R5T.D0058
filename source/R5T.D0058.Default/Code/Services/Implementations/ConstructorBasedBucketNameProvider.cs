using System;
using System.Threading.Tasks;


namespace R5T.D0058
{
    public class ConstructorBasedBucketNameProvider : IBucketNameProvider
    {
        private BucketName BucketName { get; }


        public ConstructorBasedBucketNameProvider(BucketName bucketName)
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
