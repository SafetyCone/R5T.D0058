using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;

using R5T.Magyar;
using R5T.Magyar.IO;


namespace R5T.D0058
{
    public class AmazonS3Operator : IAmazonS3Operator
    {
        public async Task<bool> CreateBucket(IAmazonS3 s3, BucketName bucketName)
        {
            var request = new PutBucketRequest
            {
                BucketName = bucketName.Value,
                UseClientRegion = true,
            };

            try
            {
                var response = await s3.PutBucketAsync(request);

                return true;
            }
            catch (AmazonS3Exception exception)
            {
                if (exception.BucketAlreadyExists())
                {
                    // That's ok, the bucket already existed.
                    return false;
                }
                else
                {
                    throw new UnhandledAmazonS3Exception(exception);
                }
            }
        }

        public async Task<bool> DeleteBucket(IAmazonS3 s3, BucketName bucketName, bool allowDeleteIfNotEmpty = false)
        {
            // TODO: implement allow-if-not-empty logic when I have put objects in things.

            var request = new DeleteBucketRequest
            {
                BucketName = bucketName.Value,
                UseClientRegion = true,
            };

            try
            {
                var response = await s3.DeleteBucketAsync(request);

                return true;
            }
            catch (AmazonS3Exception s3Exception)
            {
                if (s3Exception.BucketNotFound())
                {
                    // Then it's ok, bucket did not exist.
                    return false;
                }
                else
                {
                    // Rethrow as a type we can stop on.
                    throw new UnhandledAmazonS3Exception(s3Exception);
                }
            }
        }

        public Task<bool> DoesBucketAlreadyExistGlobally(IAmazonS3 s3, BucketName bucketName)
        {
            return AmazonS3Util.DoesS3BucketExistV2Async(s3, bucketName.Value);
        }

        public async Task<S3Region> GetS3RegionForBucket(IAmazonS3 s3, BucketName bucketName)
        {
            var request = new GetBucketLocationRequest
            {
                BucketName = bucketName.Value,
            };

            var response = await s3.GetBucketLocationAsync(request);
            if (!response.Ok())
            {
                throw new AmazonS3Exception($"Failed to get region for bucket: {bucketName}");
            }

            return response.Location;
        }

        public async Task<List<S3Bucket>> ListAllBucketsForOwner(IAmazonS3 s3)
        {
            // No need for a request. One might worry about pagination of bucket (since queries default to 100 results, or can be set to a maximum of 1000), however, you can only have 100 S3 buckets! (Or 1000 if you have a special request.)
            // See: https://docs.aws.amazon.com/AmazonS3/latest/userguide/create-bucket-overview.html

            var response = await s3.ListBucketsAsync();

            return response.Buckets;
        }

        public Task<List<S3Object>> ListObjectsInBucket(IAmazonS3 s3, BucketName bucketName, string prefix = S3ObjectKeyHelper.DefaultPrefix, int maximumCount = QueryHelper.NoLimitMaximumResultsCount)
        {
            var actualMaximumCount = AwsHelper.GetAwsActualMaximumResultsPerPageCount(maximumCount);

            var request = new ListObjectsV2Request
            {
                BucketName = bucketName.Value,
                Prefix = prefix,
                MaxKeys = actualMaximumCount,
            };

            return s3.ListAllObjects(request);
        }

        public async Task<WasFound<S3Object>> GetObjectIfExists(IAmazonS3 s3, BucketName bucketName, ObjectKey key)
        {
            // So do this hacky way instead, sking for only one object.
            var request = new ListObjectsRequest
            {
                BucketName = bucketName.Value,
                Marker = key.Value,
                MaxKeys = 1,
            };

            var response = await s3.ListObjectsAsync(request);

            var wasFound = WasFound.From(response.S3Objects.FirstOrDefault());
            return wasFound;
        }

        public async Task UploadStreamWithOverwriteByDefault(IAmazonS3 s3, BucketName bucketName, ObjectKey key, Stream stream)
        {
            using (var transferUtility = new TransferUtility(s3))
            {
                await transferUtility.UploadAsync(stream, bucketName.Value, key.Value);
            }
        }

        public async Task UploadFileWithOverwriteByDefault(IAmazonS3 s3, BucketName bucketName, ObjectKey key, string filePath)
        {
            using (var transferUtility = new TransferUtility(s3))
            {
                await transferUtility.UploadAsync(filePath, bucketName.Value, key.Value);
            }
        }

        public async Task DownloadToFile(IAmazonS3 s3, string filePath, BucketName bucketName, ObjectKey key, bool overwrite = true)
        {
            // The transfer utility overrides by default, so handle overwrite here.
            FileHelper.ThrowIfExists(filePath, overwrite);

            // Overwrite handled.
            using (var transferUtility = new TransferUtility(s3))
            {
                await transferUtility.DownloadAsync(filePath, bucketName.Value, key.Value);
            }
        }

        public async Task DownloadToStream(IAmazonS3 s3, Stream stream, BucketName bucketName, ObjectKey key)
        {
            //var request = new GetObjectRequest
            var response = await s3.GetObjectAsync(bucketName.Value, key.Value);

            await response.ResponseStream.CopyToAsync(stream);
        }

        public async Task DeleteObjectOkIfDoesNotExist(IAmazonS3 s3, BucketName bucketName, ObjectKey key)
        {
            await s3.DeleteObjectAsync(bucketName.Value, key.Value);
        }
    }
}
