using System;

using Microsoft.Extensions.DependencyInjection;

using Amazon.Runtime;

using R5T.Dacia;

using R5T.D0057;


namespace R5T.D0058
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="ConstructorBasedBucketNameProvider"/> implementation of <see cref="IBucketNameProvider"/> as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static IServiceCollection AddConstructorBasedBucketNameProvider(this IServiceCollection services, string bucketName)
        {
            services.AddSingleton<IBucketNameProvider>(new ConstructorBasedBucketNameProvider(bucketName));

            return services;
        }

        /// <summary>
        /// Adds the <see cref="ConstructorBasedS3BucketNameProvider"/> implementation of <see cref="IS3BucketNameProvider"/> as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static IServiceAction<IBucketNameProvider> AddConstructorBasedBucketNameProviderAction(this IServiceCollection services, string bucketName)
        {
            var serviceAction = ServiceAction.New<IBucketNameProvider>(() => services.AddConstructorBasedBucketNameProvider(bucketName));
            return serviceAction;
        }

        /// <summary>
        /// Adds the <see cref="AmazonS3Provider"/> implementation of <see cref="IAmazonS3Provider"/> as <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static IServiceCollection AddAmazonS3Provider(this IServiceCollection services,
            IServiceAction<AWSCredentials> aWSCredentialsAction,
            IServiceAction<IRegionEndpointProvider> awsRegionEndpointProviderAction)
        {
            services
                .AddSingleton<IAmazonS3Provider, AmazonS3Provider>()
                .Run(aWSCredentialsAction)
                .Run(awsRegionEndpointProviderAction)
                ;

            return services;
        }

        /// <summary>
        /// Adds the <see cref="AmazonS3Provider"/> implementation of <see cref="IAmazonS3Provider"/> as <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static IServiceAction<IAmazonS3Provider> AddAmazonS3ProviderAction(this IServiceCollection services,
            IServiceAction<AWSCredentials> aWSCredentialsAction,
            IServiceAction<IRegionEndpointProvider> awsRegionEndpointProviderAction)
        {
            var serviceAction = ServiceAction.New<IAmazonS3Provider>(() => services.AddAmazonS3Provider(
                aWSCredentialsAction,
                awsRegionEndpointProviderAction));

            return serviceAction;
        }

        /// <summary>
        /// Add the <see cref="AmazonS3Operator"/> implementation of <see cref="IAmazonS3Operator"/> as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static IServiceCollection AddS3Operator(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonS3Operator, AmazonS3Operator>();

            return services;
        }

        /// <summary>
        /// Add the <see cref="AmazonS3Operator"/> implementation of <see cref="IAmazonS3Operator"/> as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static IServiceAction<IAmazonS3Operator> AddS3OperatorAction(this IServiceCollection services)
        {
            var serviceAction = ServiceAction.New<IAmazonS3Operator>(() => services.AddS3Operator());
            return serviceAction;
        }
    }
}
