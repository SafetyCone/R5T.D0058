using System;
using System.Threading.Tasks;

using Amazon.S3;


namespace R5T.D0058
{
    public static class IAmazonS3ProviderExtensions
    {
        public static async Task Run(this IAmazonS3Provider s3Provider, Func<IAmazonS3, Task> action)
        {
            using (var s3 = await s3Provider.GetS3())
            {
                await action(s3);
            }
        }
    }
}
