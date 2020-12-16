using IdentityModel;
using IdentityServer4.Fsql.Storage.Mappers;
using IdentityServer4.Fsql.Storage.Stores;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Fsql.IntegrationTests.Stores
{
    public class ResourceStoreTests
    {
        private static IdentityResource CreateIdentityTestResource()
        {
            return new IdentityResource()
            {
                Name = Guid.NewGuid().ToString(),
                DisplayName = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ShowInDiscoveryDocument = true,
                UserClaims =
                {
                    JwtClaimTypes.Subject,
                    JwtClaimTypes.Name,
                }
            };
        }

        private static ApiResource CreateApiResourceTestResource()
        {
            return new ApiResource()
            {
                Name = Guid.NewGuid().ToString(),
                ApiSecrets = new List<Secret> { new Secret("secret".ToSha256()) },
                Scopes = { Guid.NewGuid().ToString() },
                UserClaims =
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }
            };
        }

        private static ApiScope CreateApiScopeTestResource()
        {
            return new ApiScope()
            {
                Name = Guid.NewGuid().ToString(),
                UserClaims =
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }
            };
        }


        [Fact]
        public async Task FindApiResourcesByNameAsync_WhenResourceExists_ExpectResourceAndCollectionsReturned()
        {
            var resource = CreateApiResourceTestResource();

            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();
            var entity = resource.ToEntity();
            repo.Insert(entity);
            repo.SaveMany(entity, "UserClaims");
            repo.SaveMany(entity, "Scopes");
            repo.SaveMany(entity, "Secrets");
            repo.SaveMany(entity, "Properties");

            ApiResource foundResource;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();


            Assert.NotNull(foundResource);
            Assert.True(foundResource.Name == resource.Name);

            Assert.NotNull(foundResource.UserClaims);
            Assert.NotEmpty(foundResource.UserClaims);
            Assert.NotNull(foundResource.ApiSecrets);
            Assert.NotEmpty(foundResource.ApiSecrets);
            Assert.NotNull(foundResource.Scopes);
            Assert.NotEmpty(foundResource.Scopes);
        }

        [Fact]
        public async Task FindApiResourcesByNameAsync_WhenResourcesExist_ExpectOnlyResourcesRequestedReturned()
        {
            var resource = CreateApiResourceTestResource();
            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();
            var entity1 = resource.ToEntity();
            repo.Insert(entity1);
            repo.SaveMany(entity1, "UserClaims");
            repo.SaveMany(entity1, "Scopes");
            repo.SaveMany(entity1, "Secrets");
            repo.SaveMany(entity1, "Properties");
            var entity2 = CreateApiResourceTestResource().ToEntity();
            repo.Insert(entity2);
            repo.SaveMany(entity2, "UserClaims");
            repo.SaveMany(entity2, "Scopes");
            repo.SaveMany(entity2, "Secrets");
            repo.SaveMany(entity2, "Properties");

            ApiResource foundResource;
            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();

            Assert.NotNull(foundResource);
            Assert.True(foundResource.Name == resource.Name);

            Assert.NotNull(foundResource.UserClaims);
            Assert.NotEmpty(foundResource.UserClaims);
            Assert.NotNull(foundResource.ApiSecrets);
            Assert.NotEmpty(foundResource.ApiSecrets);
            Assert.NotNull(foundResource.Scopes);
            Assert.NotEmpty(foundResource.Scopes);
        }




        [Fact]
        public async Task FindApiResourcesByScopeNameAsync_WhenResourcesExist_ExpectResourcesReturned()
        {
            var testApiResource = CreateApiResourceTestResource();
            var testApiScope = CreateApiScopeTestResource();
            testApiResource.Scopes.Add(testApiScope.Name);

            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();

            var entity = testApiResource.ToEntity();
            repo.Insert(entity);
            repo.SaveMany(entity, "Scopes");

            g.configurationDb.Insert(testApiScope.ToEntity()).ExecuteAffrows();

            IEnumerable<ApiResource> resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = await store.FindApiResourcesByScopeNameAsync(new List<string>
            {
                    testApiScope.Name
            });


            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == testApiResource.Name));
        }

        [Fact]
        public async Task FindApiResourcesByScopeNameAsync_WhenResourcesExist_ExpectOnlyResourcesRequestedReturned()
        {
            var testIdentityResource = CreateIdentityTestResource();
            var testApiResource = CreateApiResourceTestResource();
            var testApiScope = CreateApiScopeTestResource();
            testApiResource.Scopes.Add(testApiScope.Name);

            var identityResourcesRepo = g.configurationDb.GetRepository<Storage.Entities.IdentityResource>();
            var apiResourcesRepo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();
            var apiScopesRepo = g.configurationDb.GetRepository<Storage.Entities.ApiScope>();

            var testIdentityResourceEntity = testIdentityResource.ToEntity();
            identityResourcesRepo.Insert(testIdentityResourceEntity);
            identityResourcesRepo.SaveMany(testIdentityResourceEntity, "UserClaims");
            identityResourcesRepo.SaveMany(testIdentityResourceEntity, "Properties");

            var testApiResourceEntity = testApiResource.ToEntity();
            apiResourcesRepo.Insert(testApiResourceEntity);
            apiResourcesRepo.SaveMany(testApiResourceEntity, "Secrets");
            apiResourcesRepo.SaveMany(testApiResourceEntity, "Scopes");
            apiResourcesRepo.SaveMany(testApiResourceEntity, "UserClaims");
            apiResourcesRepo.SaveMany(testApiResourceEntity, "Properties");

            var testApiScopeEntity = testApiScope.ToEntity();
            apiScopesRepo.Insert(testApiScopeEntity);
            apiScopesRepo.SaveMany(testApiScopeEntity, "UserClaims");
            apiScopesRepo.SaveMany(testApiScopeEntity, "Properties");

            var testIdentityResourceEntity2 = CreateIdentityTestResource().ToEntity();
            identityResourcesRepo.Insert(testIdentityResourceEntity2);
            identityResourcesRepo.SaveMany(testIdentityResourceEntity2, "UserClaims");
            identityResourcesRepo.SaveMany(testIdentityResourceEntity2, "Properties");

            var testApiResourceEntity2 = CreateApiResourceTestResource().ToEntity();
            apiResourcesRepo.Insert(testApiResourceEntity2);
            apiResourcesRepo.SaveMany(testApiResourceEntity2, "Secrets");
            apiResourcesRepo.SaveMany(testApiResourceEntity2, "Scopes");
            apiResourcesRepo.SaveMany(testApiResourceEntity2, "UserClaims");
            apiResourcesRepo.SaveMany(testApiResourceEntity2, "Properties");

            var testApiScopeEntity2 = CreateApiScopeTestResource().ToEntity();
            apiScopesRepo.Insert(testApiScopeEntity2);
            apiScopesRepo.SaveMany(testApiScopeEntity2, "UserClaims");
            apiScopesRepo.SaveMany(testApiScopeEntity2, "Properties");

            IEnumerable<ApiResource> resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = await store.FindApiResourcesByScopeNameAsync(new[] { testApiScope.Name });


            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == testApiResource.Name));
        }




        [Fact]
        public async Task FindIdentityResourcesByScopeNameAsync_WhenResourceExists_ExpectResourceAndCollectionsReturned()
        {
            var resource = CreateIdentityTestResource();

            var entity = resource.ToEntity();

            var repo = g.configurationDb.GetRepository<Storage.Entities.IdentityResource>();
            repo.Insert(entity);
            repo.SaveMany(entity, "UserClaims");
            repo.SaveMany(entity, "Properties");

            IList<IdentityResource> resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = (await store.FindIdentityResourcesByScopeNameAsync(new List<string>
            {
                resource.Name
            })).ToList();


            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            var foundScope = resources.Single();

            Assert.Equal(resource.Name, foundScope.Name);
            Assert.NotNull(foundScope.UserClaims);
            Assert.NotEmpty(foundScope.UserClaims);
        }

        [Fact]
        public async Task FindIdentityResourcesByScopeNameAsync_WhenResourcesExist_ExpectOnlyRequestedReturned()
        {
            var resource = CreateIdentityTestResource();

            var entity = resource.ToEntity();

            var repo = g.configurationDb.GetRepository<Storage.Entities.IdentityResource>();
            repo.Insert(entity);
            repo.SaveMany(entity, "UserClaims");
            repo.SaveMany(entity, "Properties");

            IList<IdentityResource> resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = (await store.FindIdentityResourcesByScopeNameAsync(new List<string>
                {
                    resource.Name
                })).ToList();


            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == resource.Name));
        }



        [Fact]
        public async Task FindApiScopesByNameAsync_WhenResourceExists_ExpectResourceAndCollectionsReturned()
        {
            var resource = CreateApiScopeTestResource();

            var entity = resource.ToEntity();

            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiScope>();

            repo.Insert(entity);
            repo.SaveMany(entity, "UserClaims");
            repo.SaveMany(entity, "Properties");

            IList<ApiScope> resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = (await store.FindApiScopesByNameAsync(new List<string>
            {
                resource.Name
            })).ToList();


            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            var foundScope = resources.Single();

            Assert.Equal(resource.Name, foundScope.Name);
            Assert.NotNull(foundScope.UserClaims);
            Assert.NotEmpty(foundScope.UserClaims);
        }

        [Fact]
        public async Task FindApiScopesByNameAsync_WhenResourcesExist_ExpectOnlyRequestedReturned()
        {
            var resource = CreateApiScopeTestResource();

            var entity = resource.ToEntity();

            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiScope>();

            repo.Insert(entity);
            repo.SaveMany(entity, "UserClaims");
            repo.SaveMany(entity, "Properties");

            IList<ApiScope> resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = (await store.FindApiScopesByNameAsync(new List<string>
            {
                resource.Name
            })).ToList();

            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == resource.Name));
        }




        [Fact]
        public async Task GetAllResources_WhenAllResourcesRequested_ExpectAllResourcesIncludingHidden()
        {
            var visibleIdentityResource = CreateIdentityTestResource();
            var visibleApiResource = CreateApiResourceTestResource();
            var visibleApiScope = CreateApiScopeTestResource();
            var hiddenIdentityResource = new IdentityResource { Name = Guid.NewGuid().ToString(), ShowInDiscoveryDocument = false };
            var hiddenApiResource = new ApiResource
            {
                Name = Guid.NewGuid().ToString(),
                Scopes = { Guid.NewGuid().ToString() },
                ShowInDiscoveryDocument = false
            };
            var hiddenApiScope = new ApiScope
            {
                Name = Guid.NewGuid().ToString(),
                ShowInDiscoveryDocument = false
            };

            var identityResourcesRepo = g.configurationDb.GetRepository<Storage.Entities.IdentityResource>();
            var apiResourcesRepo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();
            var apiScopesRepo = g.configurationDb.GetRepository<Storage.Entities.ApiScope>();

            var visibleIdentityResourceEntity = visibleIdentityResource.ToEntity();
            identityResourcesRepo.Insert(visibleIdentityResourceEntity);
            identityResourcesRepo.SaveMany(visibleIdentityResourceEntity, "UserClaims");
            identityResourcesRepo.SaveMany(visibleIdentityResourceEntity, "Properties");

            var visibleApiResourceEntity = visibleApiResource.ToEntity();
            apiResourcesRepo.Insert(visibleApiResourceEntity);
            apiResourcesRepo.SaveMany(visibleApiResourceEntity, "Secrets");
            apiResourcesRepo.SaveMany(visibleApiResourceEntity, "Scopes");
            apiResourcesRepo.SaveMany(visibleApiResourceEntity, "UserClaims");
            apiResourcesRepo.SaveMany(visibleApiResourceEntity, "Properties");

            var visibleApiScopeEntity = visibleApiScope.ToEntity();
            apiScopesRepo.Insert(visibleApiScopeEntity);
            apiScopesRepo.SaveMany(visibleApiScopeEntity, "UserClaims");
            apiScopesRepo.SaveMany(visibleApiScopeEntity, "Properties");

            var hiddenIdentityResourceEntity = hiddenIdentityResource.ToEntity();
            identityResourcesRepo.Insert(hiddenIdentityResourceEntity);
            identityResourcesRepo.SaveMany(hiddenIdentityResourceEntity, "UserClaims");
            identityResourcesRepo.SaveMany(hiddenIdentityResourceEntity, "Properties");

            var hiddenApiResourceEntity = hiddenApiResource.ToEntity();
            apiResourcesRepo.Insert(hiddenApiResourceEntity);
            apiResourcesRepo.SaveMany(hiddenApiResourceEntity, "Secrets");
            apiResourcesRepo.SaveMany(hiddenApiResourceEntity, "Scopes");
            apiResourcesRepo.SaveMany(hiddenApiResourceEntity, "UserClaims");
            apiResourcesRepo.SaveMany(hiddenApiResourceEntity, "Properties");

            var hiddenApiScopeEntity = hiddenApiScope.ToEntity();
            apiScopesRepo.Insert(hiddenApiScopeEntity);
            apiScopesRepo.SaveMany(hiddenApiScopeEntity, "UserClaims");
            apiScopesRepo.SaveMany(hiddenApiScopeEntity, "Properties");


            Resources resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = await store.GetAllResourcesAsync();


            Assert.NotNull(resources);
            Assert.NotEmpty(resources.IdentityResources);
            Assert.NotEmpty(resources.ApiResources);
            Assert.NotEmpty(resources.ApiScopes);

            Assert.Contains(resources.IdentityResources, x => x.Name == visibleIdentityResource.Name);
            Assert.Contains(resources.IdentityResources, x => x.Name == hiddenIdentityResource.Name);

            Assert.Contains(resources.ApiResources, x => x.Name == visibleApiResource.Name);
            Assert.Contains(resources.ApiResources, x => x.Name == hiddenApiResource.Name);

            Assert.Contains(resources.ApiScopes, x => x.Name == visibleApiScope.Name);
            Assert.Contains(resources.ApiScopes, x => x.Name == hiddenApiScope.Name);
        }


    }
}
