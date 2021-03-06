﻿using Arcus.BackgroundJobs.KeyVault;
using Arcus.Messaging.Pumps.Abstractions.MessageHandling;
using Arcus.Messaging.Pumps.ServiceBus;
using Arcus.Security.Core.Caching;
using CloudNative.CloudEvents;
using GuardNet;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/> to make the registration of jobs more dev-friendly.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a background job to the <see cref="IServiceCollection"/> to automatically invalidate cached Azure Key Vault secrets.
        /// </summary>
        /// <param name="services">The services collection to add the job to.</param>
        /// <param name="subscriptionNamePrefix">The name of the Azure Service Bus subscription that will be created to receive Key Vault events.</param>
        /// <param name="serviceBusTopicConnectionStringSecretKey">The configuration key that points to the Azure Service Bus Topic connection string.</param>
        public static IServiceCollection AddAutoInvalidateKeyVaultSecretBackgroundJob(
            this IServiceCollection services,
            string subscriptionNamePrefix,
            string serviceBusTopicConnectionStringSecretKey)
        {
            Guard.NotNullOrWhitespace(subscriptionNamePrefix, nameof(subscriptionNamePrefix), "Requires a non-blank subscription name of the Azure Service Bus Topic subscription, to receive Key Vault events");
            Guard.NotNullOrWhitespace(serviceBusTopicConnectionStringSecretKey, nameof(serviceBusTopicConnectionStringSecretKey), "Requires a non-blank configuration key that points to a Azure Service Bus Topic");
            
            services.AddCloudEventBackgroundJob(subscriptionNamePrefix, serviceBusTopicConnectionStringSecretKey);
            services.AddSingleton<IMessageHandler<CloudEvent, AzureServiceBusMessageContext>, InvalidateKeyVaultSecretHandler>(serviceProvider =>
            {
                var cachedSecretProvider = serviceProvider.GetRequiredService<ICachedSecretProvider>();
                var logger = serviceProvider.GetRequiredService<ILogger<InvalidateKeyVaultSecretHandler>>();
                return new InvalidateKeyVaultSecretHandler(cachedSecretProvider, logger);
            });

            return services;
        }
    }
}
