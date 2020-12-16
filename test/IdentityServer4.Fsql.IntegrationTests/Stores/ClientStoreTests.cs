using FluentAssertions;
using IdentityServer4.Fsql.Storage.Mappers;
using IdentityServer4.Fsql.Storage.Stores;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace IdentityServer4.Fsql.IntegrationTests.Stores
{
    public class ClientStoreTests
    {
        [Fact]
        public async Task FindClientByIdAsync_WhenClientDoesNotExist_ExpectNull()
        {
            var store = new ClientStore(g.configurationDb, FakeLogger<ClientStore>.Create());
            var client = await store.FindClientByIdAsync(Guid.NewGuid().ToString());
            client.Should().BeNull();
        }

        [Fact]
        public async Task FindClientByIdAsync_WhenClientExists_ExpectClientRetured()
        {
            var repo = g.configurationDb.GetRepository<IdentityServer4.Fsql.Storage.Entities.Client>();

            var testClient = new Client
            {
                ClientId = "test_client",
                ClientName = "Test Client"
            };
            var entity = testClient.ToEntity();
            repo.Insert(entity);
            repo.SaveMany(entity, "AllowedCorsOrigins");
            repo.SaveMany(entity, "AllowedGrantTypes");
            repo.SaveMany(entity, "AllowedScopes");
            repo.SaveMany(entity, "Claims");
            repo.SaveMany(entity, "ClientSecrets");
            repo.SaveMany(entity, "IdentityProviderRestrictions");
            repo.SaveMany(entity, "PostLogoutRedirectUris");
            repo.SaveMany(entity, "Properties");
            repo.SaveMany(entity, "RedirectUris");


            Client client;
            var store = new ClientStore(g.configurationDb, FakeLogger<ClientStore>.Create());
            client = await store.FindClientByIdAsync(testClient.ClientId);

            client.Should().NotBeNull();

            DeleteTestData(entity);
        }

        [Fact]
        public async Task FindClientByIdAsync_WhenClientExistsWithCollections_ExpectClientReturnedCollections()
        {
            var repo = g.configurationDb.GetRepository<IdentityServer4.Fsql.Storage.Entities.Client>();
            //add test data
            var testClient = new Client
            {
                ClientId = "properties_test_client",
                ClientName = "Properties Test Client",
                AllowedCorsOrigins = { "https://localhost" },
                AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                AllowedScopes = { "openid", "profile", "api1" },
                Claims = { new ClientClaim("test", "value") },
                ClientSecrets = { new Secret("secret".Sha256()) },
                IdentityProviderRestrictions = { "AD" },
                PostLogoutRedirectUris = { "https://locahost/signout-callback" },
                Properties = { { "foo1", "bar1" }, { "foo2", "bar2" }, },
                RedirectUris = { "https://locahost/signin" }
            };
            var entity = testClient.ToEntity();
            repo.Insert(entity);
            repo.SaveMany(entity, "AllowedCorsOrigins");
            repo.SaveMany(entity, "AllowedGrantTypes");
            repo.SaveMany(entity, "AllowedScopes");
            repo.SaveMany(entity, "Claims");
            repo.SaveMany(entity, "ClientSecrets");
            repo.SaveMany(entity, "IdentityProviderRestrictions");
            repo.SaveMany(entity, "PostLogoutRedirectUris");
            repo.SaveMany(entity, "Properties");
            repo.SaveMany(entity, "RedirectUris");

            Client client;
            var store = new ClientStore(g.configurationDb, FakeLogger<ClientStore>.Create());
            client = await store.FindClientByIdAsync(testClient.ClientId);

            client.Should().BeEquivalentTo(testClient);

            //clean test data
            DeleteTestData(entity);
        }

        [Fact]
        public async Task FindClientByIdAsync_WhenClientsExistWithManyCollections_ExpectClientReturnedInUnderFiveSeconds()
        {
            //add test data
            var repo = g.configurationDb.GetRepository<IdentityServer4.Fsql.Storage.Entities.Client>();
            var testClient = new Client
            {
                ClientId = "test_client_with_uris",
                ClientName = "Test client with URIs",
                AllowedScopes = { "openid", "profile", "api1" },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials
            };

            for (int i = 0; i < 50; i++)
            {
                testClient.RedirectUris.Add($"https://localhost/{i}");
                testClient.PostLogoutRedirectUris.Add($"https://localhost/{i}");
                testClient.AllowedCorsOrigins.Add($"https://localhost:{i}");
            }

            var entity = testClient.ToEntity();
            repo.Insert(entity);
            repo.SaveMany(entity, "AllowedCorsOrigins");
            repo.SaveMany(entity, "AllowedGrantTypes");
            repo.SaveMany(entity, "AllowedScopes");
            repo.SaveMany(entity, "Claims");
            repo.SaveMany(entity, "ClientSecrets");
            repo.SaveMany(entity, "IdentityProviderRestrictions");
            repo.SaveMany(entity, "PostLogoutRedirectUris");
            repo.SaveMany(entity, "Properties");
            repo.SaveMany(entity, "RedirectUris");


            var list = new List<Storage.Entities.Client>();
            for (int i = 0; i < 50; i++)
            {
                list.Add(new Client
                {
                    ClientId = testClient.ClientId + i,
                    ClientName = testClient.ClientName,
                    AllowedScopes = testClient.AllowedScopes,
                    AllowedGrantTypes = testClient.AllowedGrantTypes,
                    RedirectUris = testClient.RedirectUris,
                    PostLogoutRedirectUris = testClient.PostLogoutRedirectUris,
                    AllowedCorsOrigins = testClient.AllowedCorsOrigins,
                }.ToEntity());
            }
            list.ForEach(entity =>
            {
                repo.Insert(entity);
                repo.SaveMany(entity, "AllowedCorsOrigins");
                repo.SaveMany(entity, "AllowedGrantTypes");
                repo.SaveMany(entity, "AllowedScopes");
                repo.SaveMany(entity, "Claims");
                repo.SaveMany(entity, "ClientSecrets");
                repo.SaveMany(entity, "IdentityProviderRestrictions");
                repo.SaveMany(entity, "PostLogoutRedirectUris");
                repo.SaveMany(entity, "Properties");
                repo.SaveMany(entity, "RedirectUris");
            });

            var store = new ClientStore(g.configurationDb, FakeLogger<ClientStore>.Create());

            const int timeout = 5000;
            var task = Task.Run(() => store.FindClientByIdAsync(testClient.ClientId));

            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                var client = task.Result;
                client.Should().BeEquivalentTo(testClient);
            }
            else
            {
                throw new TestTimeoutException(timeout);
            }


            //delete test data
            DeleteTestData(entity);
            list.ForEach(entity =>
            {
                DeleteTestData(entity);
            });
        }

        private static void DeleteTestData(Storage.Entities.Client entity)
        {
            var repo = g.configurationDb.GetRepository<IdentityServer4.Fsql.Storage.Entities.Client>();
            entity.AllowedCorsOrigins.Clear();
            entity.AllowedGrantTypes.Clear();
            entity.AllowedScopes.Clear();
            entity.Claims.Clear();
            entity.ClientSecrets.Clear();
            entity.IdentityProviderRestrictions.Clear();
            entity.PostLogoutRedirectUris.Clear();
            entity.Properties.Clear();
            entity.RedirectUris.Clear();
            repo.SaveMany(entity, "AllowedCorsOrigins");
            repo.SaveMany(entity, "AllowedGrantTypes");
            repo.SaveMany(entity, "AllowedScopes");
            repo.SaveMany(entity, "Claims");
            repo.SaveMany(entity, "ClientSecrets");
            repo.SaveMany(entity, "IdentityProviderRestrictions");
            repo.SaveMany(entity, "PostLogoutRedirectUris");
            repo.SaveMany(entity, "Properties");
            repo.SaveMany(entity, "RedirectUris");
            repo.Delete(entity);
        }

    }
}
