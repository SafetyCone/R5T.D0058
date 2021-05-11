using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using R5T.Magyar;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;


namespace R5T.D0058
{
    public static class IAmazonS3OperatorExtensions
    {
        /// <summary>
        /// Determines whether a bucket exists by name for the owner specified by the S3 client.
        /// This is usually what you mean when when you want to know whether a bucket exists (and not whether the bucket exists globally for anyone), but also see <see cref="IS3ClientOperator.DoesBucketAlreadyExistGlobally(AmazonS3Client, BucketName)"/>.
        /// </summary>
        /// <remarks>
        /// S3 bucket names are GLOBALLY unique (wtf?). Thus you can test whether a bucket name exists and receive a YES result even though you don't see that bucket yourself. Someone else owns that bucket, and yes it exists.
        /// What you actually want to know if whether you own a bucket with that name.
        /// This method gets a list of ALL buckets of the owner specified by the S3 client, then tests to see if the input bucket name exists in that list.
        /// </remarks>
        public static async Task<bool> BucketExists(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            var bucketNames = await @operator.ListAllBucketsForOwner(client);

            var bucketExists = bucketNames.Where(x => x.BucketName == bucketName.Value).Any();
            return bucketExists;
        }

        public static Task<bool> CreateBucketOkIfAlreadyExists(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            // The create bucket method is already idempotent.
            return @operator.CreateBucket(client, bucketName);
        }

        public static async Task CreateBucketThrowIfAlreadyExists(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            var bucketWasActuallyCreated = await @operator.CreateBucket(client, bucketName);
            if (!bucketWasActuallyCreated)
            {
                throw new Exception($"Failed to cerate bucket '{bucketName}'. Bucket already exists."); // TODO, re-create the actual Amazon S3 exception.
            }
        }

        public static async Task EnsureBucketExists(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            // The create bucket method is already idempotent.
            await @operator.CreateBucket(client, bucketName);

            // Test.
            var bucketExists = await @operator.BucketExists(client, bucketName);
            if (!bucketExists)
            {
                throw new Exception($"Bucket '{bucketName}' did not exist even after creation.");
            }
        }

        public static Task<bool> DeleteBucketOkIfDoesNotExist(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            // The delete bucket method is already idempotent.
            return @operator.DeleteBucket(client, bucketName);
        }

        public static async Task DeleteBucketThrowIfDoesNotExist(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            var bucketWasActuallyDeleted = await @operator.DeleteBucket(client, bucketName);
            if (!bucketWasActuallyDeleted)
            {
                throw new Exception($"Failed to delete bucket '{bucketName}'. Bucket did not exist."); // TODO, re-create the actual Amazon S3 exception.
            }
        }

        public static async Task EnsureBucketDoesNotExist(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            // Just call the idempotent delete bucket method.
            await @operator.DeleteBucket(client, bucketName);

            // Test.
            var bucketExists = await @operator.BucketExists(client, bucketName);
            if (bucketExists)
            {
                throw new Exception($"Bucket '{bucketName}' still existed even after deletion.");
            }
        }

        /// <summary>
        /// Determines what region the bucket is in.
        /// </summary>
        public static async Task<RegionEndpoint> GetRegionEndpointForBucket(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            var s3Region = await @operator.GetS3RegionForBucket(client, bucketName);

            // Try this.
            var regionEndpoint = RegionEndpoint.GetBySystemName(s3Region.Value);
            return regionEndpoint;
        }

        public static async Task<bool> IsBucketNameAvailable(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName)
        {
            var bucketAlreadyExistsGlobally = await @operator.DoesBucketAlreadyExistGlobally(client, bucketName);

            var isAvailable = !bucketAlreadyExistsGlobally;
            return isAvailable;
        }

        public static async Task<bool> ObjectExists(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName, ObjectKey key)
        {
            var wasFound = await @operator.GetObjectIfExists(client, bucketName, key);
            return wasFound;
        }

        public static async Task<S3Object> GetObject(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName, ObjectKey key)
        {
            var wasFound = await @operator.GetObjectIfExists(client, bucketName, key);

            wasFound.ThrowArgumentExceptionIfNotFound($"S3 object not found. Bucket: {bucketName}, key: {key}.");

            return wasFound.Result;
        }

        public static async Task UploadFile(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName, ObjectKey key, string filePath, bool overwrite = true)
        {
            if (!overwrite)
            {
                var objectExists = await @operator.ObjectExists(client, bucketName, key);
                if (objectExists)
                {
                    throw new Exception($"Object already exists. Bucket: {bucketName}, key: {key}.");
                }
            }

            await @operator.UploadFileWithOverwriteByDefault(client, bucketName, key, filePath);
        }

        public static async Task UploadStream(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName, ObjectKey key, Stream stream, bool overwrite = true)
        {
            if (!overwrite)
            {
                var objectExists = await @operator.ObjectExists(client, bucketName, key);
                if (objectExists)
                {
                    throw new Exception($"Object already exists. Bucket: {bucketName}, key: {key}.");
                }
            }

            await @operator.UploadStreamWithOverwriteByDefault(client, bucketName, key, stream);
        }

        /// <summary>
        /// Idempotent. Returns whether the object was actually deleted (true) or if the object already did not exist (false).
        /// </summary>
        public static async Task<bool> DeleteObjectOkIfDoesNotExistReturnResult(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName, ObjectKey key)
        {
            var objectExists = await @operator.ObjectExists(client, bucketName, key);

            await @operator.DeleteObjectOkIfDoesNotExist(client, bucketName, key);

            return objectExists;
        }

        /// <summary>
        /// Idempotent default.
        /// </summary>
        public static Task DeleteObject(this IAmazonS3Operator @operator, AmazonS3Client client, BucketName bucketName, ObjectKey key)
        {
            return @operator.DeleteObjectOkIfDoesNotExist(client, bucketName, key);
        }
    }
}
