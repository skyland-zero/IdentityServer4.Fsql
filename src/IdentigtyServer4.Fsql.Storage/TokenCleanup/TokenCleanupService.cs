// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Fsql.Storage.DbMark;
using IdentityServer4.Fsql.Storage.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Fsql.Storage.TokenCleanup
{
    /// <summary>
    /// Helper to cleanup stale persisted grants and device codes.
    /// </summary>
    public class TokenCleanupService
    {
        private readonly OperationalStoreOptions _options;
        private readonly IFreeSql<OperationalDb> _freeSql;
        private readonly IOperationalStoreNotification _operationalStoreNotification;
        private readonly ILogger<TokenCleanupService> _logger;

        /// <summary>
        /// Constructor for TokenCleanupService.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="persistedGrantDbContext"></param>
        /// <param name="operationalStoreNotification"></param>
        /// <param name="logger"></param>
        public TokenCleanupService(
            OperationalStoreOptions options,
            IFreeSql<OperationalDb> freeSql,
            ILogger<TokenCleanupService> logger,
            IOperationalStoreNotification operationalStoreNotification = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (_options.TokenCleanupBatchSize < 1) throw new ArgumentException("Token cleanup batch size interval must be at least 1");

            _freeSql = freeSql ?? throw new ArgumentNullException(nameof(freeSql));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _operationalStoreNotification = operationalStoreNotification;
        }

        /// <summary>
        /// Method to clear expired persisted grants.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveExpiredGrantsAsync()
        {
            try
            {
                _logger.LogTrace("Querying for expired grants to remove");

                await RemoveGrantsAsync();
                await RemoveDeviceCodesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception removing expired grants: {exception}", ex.Message);
            }
        }

        /// <summary>
        /// Removes the stale persisted grants.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task RemoveGrantsAsync()
        {
            var found = Int32.MaxValue;

            while (found >= _options.TokenCleanupBatchSize)
            {
                var expiredGrants = await _freeSql.Select<Entities.PersistedGrant>()
                    .Where(x => x.Expiration < DateTime.UtcNow)
                    .OrderBy(x => x.Key)
                    .Take(_options.TokenCleanupBatchSize)
                    .ToListAsync();

                found = expiredGrants.Count;
                _logger.LogInformation("Removing {grantCount} grants", found);

                if (found > 0)
                {
                    await _freeSql.Delete<Entities.PersistedGrant>(expiredGrants).ExecuteAffrowsAsync();

                    if (_operationalStoreNotification != null)
                    {
                        await _operationalStoreNotification.PersistedGrantsRemovedAsync(expiredGrants);
                    }
                }
            }
        }


        /// <summary>
        /// Removes the stale device codes.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task RemoveDeviceCodesAsync()
        {
            var found = Int32.MaxValue;

            while (found >= _options.TokenCleanupBatchSize)
            {
                var expiredCodes = await _freeSql.Select<Entities.DeviceFlowCodes>()
                    .Where(x => x.Expiration < DateTime.UtcNow)
                    .OrderBy(x => x.DeviceCode)
                    .Take(_options.TokenCleanupBatchSize)
                    .ToListAsync();

                found = expiredCodes.Count;
                _logger.LogInformation("Removing {deviceCodeCount} device flow codes", found);

                if (found > 0)
                {

                    await _freeSql.Delete<Entities.DeviceFlowCodes>(expiredCodes).ExecuteAffrowsAsync();

                    if (_operationalStoreNotification != null)
                    {
                        await _operationalStoreNotification.DeviceCodesRemovedAsync(expiredCodes);
                    }
                }
            }
        }

        //private async Task SaveChangesAsync()
        //{
        //    var count = 3;

        //    while (count > 0)
        //    {
        //        try
        //        {
        //            await _persistedGrantDbContext.SaveChangesAsync();
        //            return;
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            count--;

        //            // we get this if/when someone else already deleted the records
        //            // we want to essentially ignore this, and keep working
        //            _logger.LogDebug("Concurrency exception removing expired grants: {exception}", ex.Message);

        //            foreach (var entry in ex.Entries)
        //            {
        //                // mark this entry as not attached anymore so we don't try to re-delete
        //                entry.State = EntityState.Detached;
        //            }
        //        }
        //    }

        //    _logger.LogDebug("Too many concurrency exceptions. Exiting.");
        //}
    }
}