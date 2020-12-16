// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Fsql.Storage.DbMark;
using IdentityServer4.Fsql.Storage.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Fsql.Storage.Stores
{
    /// <summary>
    /// Implementation of IResourceStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IResourceStore" />
    public class ResourceStore : IResourceStore
    {
        /// <summary>
        /// The FreeSql.
        /// </summary>
        protected readonly IFreeSql<ConfigurationDb> FreeSql;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger<ResourceStore> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public ResourceStore(IFreeSql<ConfigurationDb> freeSql, ILogger<ResourceStore> logger)
        {
            FreeSql = freeSql ?? throw new ArgumentNullException(nameof(freeSql));
            Logger = logger;
        }

        /// <summary>
        /// Finds the API resources by name.
        /// </summary>
        /// <param name="apiResourceNames">The names.</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            if (apiResourceNames == null) throw new ArgumentNullException(nameof(apiResourceNames));

            var query = FreeSql.Select<Entities.ApiResource>().Where(x => apiResourceNames.Contains(x.Name));

            var apis = query
                .IncludeMany(x => x.Secrets)
                .IncludeMany(x => x.Scopes)
                .IncludeMany(x => x.UserClaims)
                .IncludeMany(x => x.Properties);

            var result = (await apis.ToListAsync())
                .Where(x => apiResourceNames.Contains(x.Name))
                .Select(x => x.ToModel()).ToArray();

            if (result.Any())
            {
                Logger.LogDebug("Found {apis} API resource in database", result.Select(x => x.Name));
            }
            else
            {
                Logger.LogDebug("Did not find {apis} API resource in database", apiResourceNames);
            }

            return result;
        }

        /// <summary>
        /// Gets API resources by scope name.
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var names = scopeNames.ToArray();

            var query = FreeSql.Select<Entities.ApiResource>().Where(x => x.Scopes.AsSelect().Any(y => names.Contains(y.Scope)));

            var apis = query
                .IncludeMany(x => x.Secrets)
                .IncludeMany(x => x.Scopes)
                .IncludeMany(x => x.UserClaims)
                .IncludeMany(x => x.Properties);

            var results = (await apis.ToListAsync())
                .Where(api => api.Scopes.Any(x => names.Contains(x.Scope)));
            var models = results.Select(x => x.ToModel()).ToArray();

            Logger.LogDebug("Found {apis} API resources in database", models.Select(x => x.Name));

            return models;
        }

        /// <summary>
        /// Gets identity resources by scope name.
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var scopes = scopeNames.ToArray();

            var query = FreeSql.Select<Entities.IdentityResource>().Where(x => scopes.Contains(x.Name));

            var resources = query
                .IncludeMany(x => x.UserClaims)
                .IncludeMany(x => x.Properties);

            var results = await resources.ToListAsync();

            Logger.LogDebug("Found {scopes} identity scopes in database", results.Select(x => x.Name));

            return results.Select(x => x.ToModel()).ToArray();
        }

        /// <summary>
        /// Gets scopes by scope name.
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            var scopes = scopeNames.ToArray();

            var query = FreeSql.Select<Entities.ApiScope>().Where(x => scopes.Contains(x.Name));

            var resources = query
                .IncludeMany(x => x.UserClaims)
                .IncludeMany(x => x.Properties);

            var results = await resources.ToListAsync();

            Logger.LogDebug("Found {scopes} scopes in database", results.Select(x => x.Name));

            return results.Select(x => x.ToModel()).ToArray();
        }

        /// <summary>
        /// Gets all resources.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Resources> GetAllResourcesAsync()
        {
            var identity = FreeSql.Select<Entities.IdentityResource>()
              .IncludeMany(x => x.UserClaims)
              .IncludeMany(x => x.Properties);

            var apis = FreeSql.Select<Entities.ApiResource>()
                .IncludeMany(x => x.Secrets)
                .IncludeMany(x => x.Scopes)
                .IncludeMany(x => x.UserClaims)
                .IncludeMany(x => x.Properties);

            var scopes = FreeSql.Select<Entities.ApiScope>()
                .IncludeMany(x => x.UserClaims)
                .IncludeMany(x => x.Properties);

            var result = new Resources(
                (await identity.ToListAsync()).Select(x => x.ToModel()),
                (await apis.ToListAsync()).Select(x => x.ToModel()),
                (await scopes.ToListAsync()).Select(x => x.ToModel())
            );

            Logger.LogDebug("Found {scopes} as all scopes, and {apis} as API resources",
                result.IdentityResources.Select(x => x.Name).Union(result.ApiScopes.Select(x => x.Name)),
                result.ApiResources.Select(x => x.Name));

            return result;
        }
    }
}