using System;
using System.Threading.Tasks;

using Amazon.S3;


namespace R5T.D0058
{
    public interface IAmazonS3Provider
    {
        Task<IAmazonS3> GetS3();
    }
}
