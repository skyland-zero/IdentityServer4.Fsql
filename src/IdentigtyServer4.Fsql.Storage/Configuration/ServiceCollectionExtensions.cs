using IdentityServer4.Fsql.Storage.DbMark;
using IdentityServer4.Fsql.Storage.Extensions;
using IdentityServer4.Fsql.Storage.Options;
using IdentityServer4.Fsql.Storage.TokenCleanup;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IdentityServer4.Fsql.Storage.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Configuration FreeSql实例到DI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IServiceCollection AddConfigurationDbContext(this IServiceCollection services,
            Action<ConfigurationStoreOptions> storeOptionsAction = null)
        {
            var options = new ConfigurationStoreOptions();
            services.AddSingleton(options);
            storeOptionsAction?.Invoke(options);

            var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(options.DataType, options.ConnectionString)
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库
                .Build<ConfigurationDb>(); //请务必定义成 Singleton 单例模式

            fsql.ConfigureResourcesContext();
            fsql.ConfigureClientContext();
            fsql.SyncStructureResources();
            fsql.SyncStructureClient();

            services.AddSingleton(fsql);

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="storeOptionsAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddOperationalDbContext(this IServiceCollection services,
            Action<OperationalStoreOptions> storeOptionsAction = null)
        {
            var options = new OperationalStoreOptions();
            services.AddSingleton(options);
            storeOptionsAction?.Invoke(options);


            var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(options.DataType, options.ConnectionString)
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库
                .Build<OperationalDb>(); //请务必定义成 Singleton 单例模式

            fsql.ConfigurePersistedGrantContext();
            fsql.SyncStructurePersistedGrant();

            services.AddSingleton(fsql);
            //TODO  services.AddTransient<TokenCleanupService>();
            return services;
        }

        /// <summary>
        /// Adds an implementation of the IOperationalStoreNotification to the DI system.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOperationalStoreNotification<T>(this IServiceCollection services)
           where T : class, IOperationalStoreNotification
        {
            services.AddTransient<IOperationalStoreNotification, T>();
            return services;
        }


    }
}
