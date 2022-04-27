using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using R5T.Magyar;

using R5T.T0064;


namespace R5T.D0058
{
    [ServiceDefinitionMarker]
    public interface IAmazonS3Operator : IServiceDefinition
    {
        #region Buckets

        /// <summary>
        /// Creates a bucket, idempotent.
        /// Returns whether the bucket was actually created (true) or if the bucket already existed (false).
        /// </summary>
        /// <remarks>
        /// Implementations should be idempotent.
        /// </remarks>
        // TODO: a region overload?
        Task<bool> CreateBucket(IAmazonS3 s3, BucketName bucketName);

        /// <summary>
        /// Deletes a bucket, idempotent.
        /// Returns whether the bucket was actually deleted (true) or if the bucket already did not exist (false).
        /// </summary>
        /// <remarks>
        /// Implementations should be idempotent.
        /// </remarks>
        Task<bool> DeleteBucket(IAmazonS3 s3, BucketName bucketName, bool allowDeleteIfNotEmpty = false);

        /// <summary>
        /// Since AWS S3 bucket names are GLOBALLY UNIQUE (wtf?), this method tests whether there is any bucket with the given name, anywhere, with any owner.
        /// When you want to know if a bucket exists, this is generally ot what you want. Instead, see <see cref="ListAllBucketsForOwner(AmazonS3Client)"/>.
        /// </summary>
        /// <remarks>
        /// This method implementation should use <see cref="Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(IAmazonS3, string)"/>.
        /// </remarks>
        Task<bool> DoesBucketAlreadyExistGlobally(IAmazonS3 s3, BucketName bucketName);

        /// <summary>
        /// Determines what region the bucket is in.
        /// </summary>
        Task<S3Region> GetS3RegionForBucket(IAmazonS3 s3, BucketName bucketName);

        /// <summary>
        /// Lists all S3 buckets for owner specified by the S3 client.
        /// S3 buckets are regional, but the list of all buckets is cross-regional, and limited only owner (which is implicitly specified by the input S3 client).
        /// </summary>
        Task<List<S3Bucket>> ListAllBucketsForOwner(IAmazonS3 s3);

        Task<List<S3Object>> ListObjectsInBucket(IAmazonS3 s3, BucketName bucketName, string prefix = S3ObjectKeyHelper.DefaultPrefix, int maximumCount = AwsHelper.DefaultMaximumResultsPerPageCount);

        #endregion

        #region Objects

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Unbelievably, there is no official way to check if a key exists other than trapping an exception!
        /// There was the useful S3FileInfo, which would be great, it just doesn't exist for .NET Standard???
        /// Basically, you have to list the single object, and in that case you are already getting the S3Object, so you might as well make lemonade and return the S3Object.
        /// </remarks>
        Task<WasFound<S3Object>> GetObjectIfExists(IAmazonS3 s3, BucketName bucketName, ObjectKey key);

        /// <summary>
        /// The S3 transfer utility functionality will overwrite the content of an existing key with new content and there seems to be no native way to prevent overwrite.
        /// </summary>
        /// <remarks>
        /// Separate stream and file path methods are provided since the S3 transfer utility provides special functionality for file paths.
        /// There is special S3 upload functionality that given the path of a large file, internally creates multiple threads to upload the file. This functionality is not available for streams, and helps throughput for large files.
        /// With both streams and file paths, a multi-part upload strategy is used for large files.
        /// </remarks>
        Task UploadStreamWithOverwriteByDefault(IAmazonS3 s3, BucketName bucketName, ObjectKey key, Stream stream);

        /// <summary>
        /// The S3 transfer utility functionality will overwrite the content of an existing key with new content and there seems to be no native way to prevent overwrite.
        /// </summary>
        /// <remarks>
        /// Separate stream and file path methods are provided since the S3 transfer utility provides special functionality for file paths.
        /// There is special S3 upload functionality that given the path of a large file, internally creates multiple threads to upload the file. This functionality is not available for streams, and helps throughput for large files.
        /// With both streams and file paths, a multi-part upload strategy is used for large files.
        /// Stringly-typed paths.
        /// </remarks>
        Task UploadFileWithOverwriteByDefault(IAmazonS3 s3, BucketName bucketName, ObjectKey key, string filePath);

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Separate stream and file path methods are provided since the S3 transfer utility provides functionality for file paths, while the S3 client GetObject() is used for stream, and the transfer utility is the preferred methodology.
        /// </remarks>
        Task DownloadToStream(IAmazonS3 s3, Stream stream, BucketName bucketName, ObjectKey key);

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Separate stream and file path methods are provided since the S3 transfer utility provides functionality for file paths, while the S3 client GetObject() is used for stream, and the transfer utility is the preferred methodology.
        /// Multipart downloads are only faster if the file was originally uploaded in multiple parts? https://github.com/aws/aws-sdk-java/issues/893
        /// </remarks>
        Task DownloadToFile(IAmazonS3 s3, string filePath, BucketName bucketName, ObjectKey key, bool overwrite = false);

        /// <summary>
        /// Deletes an object, idempotent.
        /// </summary>
        /// <remarks>
        /// The native S3 client DeleteObject() call is idempotent.
        /// Implementations should be idempotent.
        /// </remarks>
        Task DeleteObjectOkIfDoesNotExist(IAmazonS3 s3, BucketName bucketName, ObjectKey key);

        #endregion
    }
}
