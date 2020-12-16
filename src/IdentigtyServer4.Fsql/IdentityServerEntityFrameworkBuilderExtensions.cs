using IdentityServer4.Fsql.Services;
using IdentityServer4.Fsql.Storage.Configuration;
using IdentityServer4.Fsql.Storage.Options;
using IdentityServer4.Fsql.Storage.Stores;
using IdentityServer4.Fsql.Storage.TokenCleanup;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace IdentityServer4.Fsql
{
    public static class IdentityServerEntityFrameworkBuilderExtensions
    {

        /// <summary>
        /// Configures EF implementation of IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <typeparam name="TContext">The IConfigurationDbContext to use.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStore(
            this IIdentityServerBuilder builder,
            Action<ConfigurationStoreOptions> storeOptionsAction = null)
        {
            builder.Services.AddConfigurationDbContext(storeOptionsAction);

            builder.AddClientStore<ClientStore>();
            builder.AddResourceStore<ResourceStore>();
            builder.AddCorsPolicyService<CorsPolicyService>();

            return builder;
        }

        /// <summary>
        /// Configures caching for IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStoreCache(
            this IIdentityServerBuilder builder)
        {
            builder.AddInMemoryCaching();

            // add the caching decorators
            builder.AddClientStoreCache<ClientStore>();
            builder.AddResourceStoreCache<ResourceStore>();
            builder.AddCorsPolicyCache<CorsPolicyService>();

            return builder;
        }

        /// <summary>
        /// Configures EF implementation of IPersistedGrantStore with IdentityServer.
        /// </summary>
        /// <typeparam name="TContext">The IPersistedGrantDbContext to use.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddOperationalStore(
            this IIdentityServerBuilder builder,
            Action<OperationalStoreOptions> storeOptionsAction = null)
        {
            builder.Services.AddOperationalDbContext(storeOptionsAction);

            builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
            builder.Services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();
            builder.Services.AddSingleton<IHostedService, TokenCleanupHost>();

            return builder;
        }

        /// <summary>
        /// Adds an implementation of the IOperationalStoreNotification to IdentityServer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddOperationalStoreNotification<T>(
           this IIdentityServerBuilder builder)
           where T : class, IOperationalStoreNotification
        {
            builder.Services.AddOperationalStoreNotification<T>();
            return builder;
        }

    }
}
