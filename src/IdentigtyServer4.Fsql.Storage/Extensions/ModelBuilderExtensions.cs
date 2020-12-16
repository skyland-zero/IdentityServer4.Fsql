using IdentityServer4.Fsql.Storage.DbMark;
using IdentityServer4.Fsql.Storage.Entities;

namespace IdentityServer4.Fsql.Storage.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static IFreeSql<ConfigurationDb> ConfigureClientContext(this IFreeSql<ConfigurationDb> fsql)
        {
            fsql.Aop.ConfigEntityProperty += (s, e) =>
            {
                if (e.Property.Name == "Id")
                {
                    e.ModifyResult.IsIdentity = true;
                }
            };
            fsql.CodeFirst.Entity<Client>(client =>
            {
                client.ToTable("Client");
                client.HasKey(x => x.Id);

                client.Property(x => x.ClientId).HasMaxLength(200).IsRequired();
                client.Property(x => x.ProtocolType).HasMaxLength(200).IsRequired();
                client.Property(x => x.ClientName).HasMaxLength(200);
                client.Property(x => x.ClientUri).HasMaxLength(2000);
                client.Property(x => x.LogoUri).HasMaxLength(2000);
                client.Property(x => x.Description).HasMaxLength(1000);
                client.Property(x => x.FrontChannelLogoutUri).HasMaxLength(2000);
                client.Property(x => x.BackChannelLogoutUri).HasMaxLength(2000);
                client.Property(x => x.ClientClaimsPrefix).HasMaxLength(200);
                client.Property(x => x.PairWiseSubjectSalt).HasMaxLength(200);
                client.Property(x => x.UserCodeType).HasMaxLength(100);
                client.Property(x => x.AllowedIdentityTokenSigningAlgorithms).HasMaxLength(100);

                client.HasIndex(x => x.ClientId).IsUnique();

                client.HasMany(x => x.AllowedGrantTypes).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.RedirectUris).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.PostLogoutRedirectUris).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.AllowedScopes).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.ClientSecrets).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.Claims).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.IdentityProviderRestrictions).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.AllowedCorsOrigins).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                client.HasMany(x => x.Properties).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
            });

            fsql.CodeFirst.Entity<ClientGrantType>(grantType =>
            {
                grantType.ToTable("ClientGrantType");
                grantType.Property(x => x.GrantType).HasMaxLength(250).IsRequired();
            });

            fsql.CodeFirst.Entity<ClientRedirectUri>(redirectUri =>
            {
                redirectUri.ToTable("ClientRedirectUri");
                redirectUri.Property(x => x.RedirectUri).HasMaxLength(2000).IsRequired();
            });

            fsql.CodeFirst.Entity<ClientPostLogoutRedirectUri>(postLogoutRedirectUri =>
            {
                postLogoutRedirectUri.ToTable("ClientPostLogoutRedirectUri");
                postLogoutRedirectUri.Property(x => x.PostLogoutRedirectUri).HasMaxLength(2000).IsRequired();
            });

            fsql.CodeFirst.Entity<ClientScope>(scope =>
            {
                scope.ToTable("ClientScopes");
                scope.Property(x => x.Scope).HasMaxLength(200).IsRequired();
            });

            fsql.CodeFirst.Entity<ClientSecret>(secret =>
            {
                secret.ToTable("ClientSecret");
                secret.Property(x => x.Value).HasMaxLength(4000).IsRequired();
                secret.Property(x => x.Type).HasMaxLength(250).IsRequired();
                secret.Property(x => x.Description).HasMaxLength(2000);
            });

            fsql.CodeFirst.Entity<ClientClaim>(claim =>
            {
                claim.ToTable("ClientClaim");
                claim.Property(x => x.Type).HasMaxLength(250).IsRequired();
                claim.Property(x => x.Value).HasMaxLength(250).IsRequired();
            });

            fsql.CodeFirst.Entity<ClientIdPRestriction>(idPRestriction =>
            {
                idPRestriction.ToTable("ClientIdPRestriction");
                idPRestriction.Property(x => x.Provider).HasMaxLength(200).IsRequired();
            });

            fsql.CodeFirst.Entity<ClientCorsOrigin>(corsOrigin =>
            {
                corsOrigin.ToTable("ClientCorsOrigin");
                corsOrigin.Property(x => x.Origin).HasMaxLength(150).IsRequired();
            });

            fsql.CodeFirst.Entity<ClientProperty>(property =>
            {
                property.ToTable("ClientProperty");
                property.Property(x => x.Key).HasMaxLength(250).IsRequired();
                property.Property(x => x.Value).HasMaxLength(2000).IsRequired();
            });


            return fsql;
        }

        public static IFreeSql<ConfigurationDb> ConfigureResourcesContext(this IFreeSql<ConfigurationDb> fsql)
        {
            fsql.Aop.ConfigEntityProperty += (s, e) =>
            {
                if (e.Property.Name == "Id")
                {
                    e.ModifyResult.IsIdentity = true;
                }
            };
            fsql.CodeFirst.Entity<IdentityResource>(identityResource =>
            {
                identityResource.ToTable("IdentityResource").HasKey(x => x.Id);

                identityResource.Property(x => x.Name).HasMaxLength(200).IsRequired();
                identityResource.Property(x => x.DisplayName).HasMaxLength(200);
                identityResource.Property(x => x.Description).HasMaxLength(1000);

                identityResource.HasIndex(x => x.Name).IsUnique();

                identityResource.HasMany(x => x.UserClaims).WithOne(x => x.IdentityResource).HasForeignKey(x => x.IdentityResourceId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                identityResource.HasMany(x => x.Properties).WithOne(x => x.IdentityResource).HasForeignKey(x => x.IdentityResourceId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
            });

            fsql.CodeFirst.Entity<IdentityResourceClaim>(claim =>
            {
                claim.ToTable("IdentityResourceClaim").HasKey(x => x.Id);

                claim.Property(x => x.Type).HasMaxLength(200).IsRequired();
            });

            fsql.CodeFirst.Entity<IdentityResourceProperty>(property =>
            {
                property.ToTable("IdentityResourceProperty");
                property.Property(x => x.Key).HasMaxLength(250).IsRequired();
                property.Property(x => x.Value).HasMaxLength(2000).IsRequired();
            });



            fsql.CodeFirst.Entity<ApiResource>(apiResource =>
            {
                apiResource.ToTable("ApiResource").HasKey(x => x.Id);

                apiResource.Property(x => x.Name).HasMaxLength(200).IsRequired();
                apiResource.Property(x => x.DisplayName).HasMaxLength(200);
                apiResource.Property(x => x.Description).HasMaxLength(1000);
                apiResource.Property(x => x.AllowedAccessTokenSigningAlgorithms).HasMaxLength(100);

                apiResource.HasIndex(x => x.Name).IsUnique();

                apiResource.HasMany(x => x.Secrets).WithOne(x => x.ApiResource).HasForeignKey(x => x.ApiResourceId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                apiResource.HasMany(x => x.Scopes).WithOne(x => x.ApiResource).HasForeignKey(x => x.ApiResourceId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                apiResource.HasMany(x => x.UserClaims).WithOne(x => x.ApiResource).HasForeignKey(x => x.ApiResourceId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
                apiResource.HasMany(x => x.Properties).WithOne(x => x.ApiResource).HasForeignKey(x => x.ApiResourceId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
            });

            fsql.CodeFirst.Entity<ApiResourceSecret>(apiSecret =>
            {
                apiSecret.ToTable("ApiResourceSecret").HasKey(x => x.Id);

                apiSecret.Property(x => x.Description).HasMaxLength(1000);
                apiSecret.Property(x => x.Value).HasMaxLength(4000).IsRequired();
                apiSecret.Property(x => x.Type).HasMaxLength(250).IsRequired();
            });

            fsql.CodeFirst.Entity<ApiResourceClaim>(apiClaim =>
            {
                apiClaim.ToTable("ApiResourceClaim").HasKey(x => x.Id);

                apiClaim.Property(x => x.Type).HasMaxLength(200).IsRequired();
            });

            fsql.CodeFirst.Entity<ApiResourceScope>(apiScope =>
            {
                apiScope.ToTable("ApiResourceScope").HasKey(x => x.Id);

                apiScope.Property(x => x.Scope).HasMaxLength(200).IsRequired();
            });

            fsql.CodeFirst.Entity<ApiResourceProperty>(property =>
            {
                property.ToTable("ApiResourceProperty");
                property.Property(x => x.Key).HasMaxLength(250).IsRequired();
                property.Property(x => x.Value).HasMaxLength(2000).IsRequired();
            });


            fsql.CodeFirst.Entity<ApiScope>(scope =>
            {
                scope.ToTable("ApiScope").HasKey(x => x.Id);

                scope.Property(x => x.Name).HasMaxLength(200).IsRequired();
                scope.Property(x => x.DisplayName).HasMaxLength(200);
                scope.Property(x => x.Description).HasMaxLength(1000);

                scope.HasIndex(x => x.Name).IsUnique();

                scope.HasMany(x => x.UserClaims).WithOne(x => x.Scope).HasForeignKey(x => x.ScopeId);//.IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
            fsql.CodeFirst.Entity<ApiScopeClaim>(scopeClaim =>
            {
                scopeClaim.ToTable("ApiScopeClaim").HasKey(x => x.Id);

                scopeClaim.Property(x => x.Type).HasMaxLength(200).IsRequired();
            });
            fsql.CodeFirst.Entity<ApiScopeProperty>(property =>
            {
                property.ToTable("ApiScopeProperty").HasKey(x => x.Id);
                property.Property(x => x.Key).HasMaxLength(250).IsRequired();
                property.Property(x => x.Value).HasMaxLength(2000).IsRequired();
            });

            return fsql;
        }

        /// <summary>
        /// Configures the persisted grant context.
        /// </summary>
        /// <param name="fsql.CodeFirst">The model builder.</param>
        /// <param name="storeOptions">The store options.</param>
        public static IFreeSql<OperationalDb> ConfigurePersistedGrantContext(this IFreeSql<OperationalDb> fsql)
        {
            fsql.Aop.ConfigEntityProperty += (s, e) =>
            {
                if (e.Property.Name == "Id")
                {
                    e.ModifyResult.IsIdentity = true;
                }
            };
            fsql.CodeFirst.Entity<PersistedGrant>(grant =>
            {
                grant.ToTable("PersistedGrants");

                grant.Property(x => x.Key).HasMaxLength(200);//.ValueGeneratedNever();
                grant.Property(x => x.Type).HasMaxLength(50).IsRequired();
                grant.Property(x => x.SubjectId).HasMaxLength(200);
                grant.Property(x => x.SessionId).HasMaxLength(100);
                grant.Property(x => x.ClientId).HasMaxLength(200).IsRequired();
                grant.Property(x => x.Description).HasMaxLength(200);
                grant.Property(x => x.CreationTime).IsRequired();
                // 50000 chosen to be explicit to allow enough size to avoid truncation, yet stay beneath the MySql row size limit of ~65K
                // apparently anything over 4K converts to nvarchar(max) on SqlServer
                grant.Property(x => x.Data).HasMaxLength(50000).IsRequired();

                grant.HasKey(x => x.Key);

                grant.HasIndex(x => new { x.SubjectId, x.ClientId, x.Type });
                grant.HasIndex(x => new { x.SubjectId, x.SessionId, x.Type });
                grant.HasIndex(x => x.Expiration);
            });

            fsql.CodeFirst.Entity<DeviceFlowCodes>(codes =>
            {
                codes.ToTable("DeviceFlowCodes");

                codes.Property(x => x.DeviceCode).HasMaxLength(200).IsRequired();
                codes.Property(x => x.UserCode).HasMaxLength(200).IsRequired();
                codes.Property(x => x.SubjectId).HasMaxLength(200);
                codes.Property(x => x.SessionId).HasMaxLength(100);
                codes.Property(x => x.ClientId).HasMaxLength(200).IsRequired();
                codes.Property(x => x.Description).HasMaxLength(200);
                codes.Property(x => x.CreationTime).IsRequired();
                codes.Property(x => x.Expiration).IsRequired();
                // 50000 chosen to be explicit to allow enough size to avoid truncation, yet stay beneath the MySql row size limit of ~65K
                // apparently anything over 4K converts to nvarchar(max) on SqlServer
                codes.Property(x => x.Data).HasMaxLength(50000).IsRequired();

                codes.HasKey(x => new { x.UserCode });

                codes.HasIndex(x => x.DeviceCode).IsUnique();
                codes.HasIndex(x => x.Expiration);
            });

            return fsql;
        }


    }
}
