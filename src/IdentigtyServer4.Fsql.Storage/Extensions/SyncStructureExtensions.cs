using IdentityServer4.Fsql.Storage.DbMark;
using IdentityServer4.Fsql.Storage.Entities;
using System.Linq;

namespace IdentityServer4.Fsql.Storage.Extensions
{
    public static class SyncStructureExtensions
    {
        public static IFreeSql<ConfigurationDb> SyncStructureClient(this IFreeSql<ConfigurationDb> fsql)
        {
            fsql.CodeFirst.SyncStructure<Client>();
            fsql.CodeFirst.SyncStructure<ClientGrantType>();
            fsql.CodeFirst.SyncStructure<ClientRedirectUri>();
            fsql.CodeFirst.SyncStructure<ClientPostLogoutRedirectUri>();
            fsql.CodeFirst.SyncStructure<ClientScope>();
            fsql.CodeFirst.SyncStructure<ClientSecret>();
            fsql.CodeFirst.SyncStructure<ClientClaim>();
            fsql.CodeFirst.SyncStructure<ClientIdPRestriction>();
            fsql.CodeFirst.SyncStructure<ClientCorsOrigin>();
            fsql.CodeFirst.SyncStructure<ClientProperty>();
            return fsql;
        }

        public static IFreeSql<ConfigurationDb> SyncStructureResources(this IFreeSql<ConfigurationDb> fsql)
        {
            fsql.CodeFirst.SyncStructure<IdentityResource>();
            fsql.CodeFirst.SyncStructure<IdentityResourceClaim>();
            fsql.CodeFirst.SyncStructure<IdentityResourceProperty>();
            fsql.CodeFirst.SyncStructure<ApiResource>();
            fsql.CodeFirst.SyncStructure<ApiResourceSecret>();
            fsql.CodeFirst.SyncStructure<ApiResourceClaim>();
            fsql.CodeFirst.SyncStructure<ApiResourceScope>();
            fsql.CodeFirst.SyncStructure<ApiResourceProperty>();
            fsql.CodeFirst.SyncStructure<ApiScope>();
            fsql.CodeFirst.SyncStructure<ApiScopeClaim>();
            fsql.CodeFirst.SyncStructure<ApiScopeProperty>();
            return fsql;
        }

        public static IFreeSql<OperationalDb> SyncStructurePersistedGrant(this IFreeSql<OperationalDb> fsql)
        {
            fsql.CodeFirst.SyncStructure<PersistedGrant>();
            fsql.CodeFirst.SyncStructure<DeviceFlowCodes>();
            return fsql;
        }

    }
}
