// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FreeSql;
using IdentityServer4.Extensions;
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
    /// Implementation of IPersistedGrantStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IPersistedGrantStore" />
    public class PersistedGrantStore : IPersistedGrantStore
    {
        /// <summary>
        /// The FreeSql.
        /// </summary>
        protected readonly IFreeSql<OperationalDb> FreeSql;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedGrantStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public PersistedGrantStore(IFreeSql<OperationalDb> freeSql, ILogger<PersistedGrantStore> logger)
        {
            FreeSql = freeSql;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task StoreAsync(PersistedGrant token)
        {
            var existing = await FreeSql.Select<Entities.PersistedGrant>().Where(x => x.Key == token.Key).FirstAsync();

            try
            {
                if (existing == null)
                {
                    Logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

                    var persistedGrant = token.ToEntity();
                    await FreeSql.Insert(persistedGrant).ExecuteAffrowsAsync();
                }
                else
                {
                    Logger.LogDebug("{persistedGrantKey} found in database", token.Key);

                    token.UpdateEntity(existing);
                    await FreeSql.Update<Entities.PersistedGrant>().SetSource(existing).ExecuteAffrowsAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = await FreeSql.Select<Entities.PersistedGrant>().Where(x => x.Key == key).FirstAsync();
            var model = persistedGrant?.ToModel();

            Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return model;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var query = FreeSql.Select<Entities.PersistedGrant>();
            var persistedGrants = await Filter(query, filter).ToListAsync();

            var model = persistedGrants.Select(x => x.ToModel());

            Logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Count, filter);

            return model;
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAsync(string key)
        {
            var persistedGrant = await FreeSql.Select<Entities.PersistedGrant>().Where(x => x.Key == key).FirstAsync();
            if (persistedGrant != null)
            {
                Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

                try
                {
                    await FreeSql.Delete<Entities.PersistedGrant>(persistedGrant).ExecuteAffrowsAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
                }
            }
            else
            {
                Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var query = FreeSql.Select<Entities.PersistedGrant>();
            var persistedGrants = await Filter(query, filter).ToListAsync();

            Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", persistedGrants.Count, filter);

            try
            {
                await FreeSql.Delete<Entities.PersistedGrant>(persistedGrants).ExecuteAffrowsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {@filter}: {error}", persistedGrants.Count, filter, ex.Message);
            }
        }


        private ISelect<Entities.PersistedGrant> Filter(ISelect<Entities.PersistedGrant> query, PersistedGrantFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.ClientId))
            {
                query = query.Where(x => x.ClientId == filter.ClientId);
            }
            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            return query;
        }
    }
}