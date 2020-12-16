// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Fsql.Storage.DbMark;
using IdentityServer4.Fsql.Storage.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Fsql.Storage.Stores
{
    /// <summary>
    /// Implementation of IClientStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IClientStore" />
    public class ClientStore : IClientStore
    {
        /// <summary>
        /// The FreeSql.
        /// </summary>
        protected readonly IFreeSql<ConfigurationDb> FreeSql;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger<ClientStore> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public ClientStore(IFreeSql<ConfigurationDb> freeSql, ILogger<ClientStore> logger)
        {
            FreeSql = freeSql ?? throw new ArgumentNullException(nameof(freeSql));
            Logger = logger;
        }

        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public virtual async Task<Client> FindClientByIdAsync(string clientId)
        {
            var baseQuery = FreeSql.Select<Entities.Client>().Where(x => x.ClientId == clientId);

            if (!await baseQuery.AnyAsync()) return null;

            var client = await baseQuery
                .IncludeMany(x => x.AllowedCorsOrigins)
                .IncludeMany(x => x.AllowedGrantTypes)
                .IncludeMany(x => x.AllowedScopes)
                .IncludeMany(x => x.Claims)
                .IncludeMany(x => x.IdentityProviderRestrictions)
                .IncludeMany(x => x.PostLogoutRedirectUris)
                .IncludeMany(x => x.Properties)
                .IncludeMany(x => x.RedirectUris)
                .IncludeMany(x => x.ClientSecrets)
                .FirstAsync();

            var model = client.ToModel();

            Logger.LogDebug("{clientId} found in database: {clientIdFound}", clientId, model != null);

            return model;
        }
    }
}