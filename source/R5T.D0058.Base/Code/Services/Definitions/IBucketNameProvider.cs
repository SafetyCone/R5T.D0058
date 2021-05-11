using System;
using System.Threading.Tasks;


namespace R5T.D0058
{
    public interface IBucketNameProvider
    {
        Task<BucketName> GetBucketName();
    }
}
