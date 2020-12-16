using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Fsql.Storage.Mappers;
using IdentityServer4.Fsql.Storage.Stores;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Xunit;

namespace IdentityServer4.Fsql.IntegrationTests.Stores
{
    public class PersistedGrantStoreTests
    {
        private static PersistedGrant CreateTestObject(string sub = null, string clientId = null, string sid = null, string type = null)
        {
            return new PersistedGrant
            {
                Key = Guid.NewGuid().ToString(),
                Type = type ?? "authorization_code",
                ClientId = clientId ?? Guid.NewGuid().ToString(),
                SubjectId = sub ?? Guid.NewGuid().ToString(),
                SessionId = sid ?? Guid.NewGuid().ToString(),
                CreationTime = new DateTime(2016, 08, 01),
                Expiration = new DateTime(2016, 08, 31),
                Data = Guid.NewGuid().ToString()
            };
        }

        [Fact]
        public async Task StoreAsync_WhenPersistedGrantStored_ExpectSuccess()
        {
            var persistedGrant = CreateTestObject();

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store.StoreAsync(persistedGrant);

            var foundGrant = g.operationalDb.Select<Storage.Entities.PersistedGrant>().Where(x => x.Key == persistedGrant.Key).First();
            Assert.NotNull(foundGrant);
        }

        [Fact]
        public async Task GetAsync_WithKeyAndPersistedGrantExists_ExpectPersistedGrantReturned()
        {
            var persistedGrant = CreateTestObject();

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();

            var entity = persistedGrant.ToEntity();

            repo.Insert(entity);

            PersistedGrant foundPersistedGrant;

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            foundPersistedGrant = await store.GetAsync(persistedGrant.Key);

            Assert.NotNull(foundPersistedGrant);
        }

        [Fact]
        public async Task GetAllAsync_WithSubAndTypeAndPersistedGrantExists_ExpectPersistedGrantReturned()
        {
            var persistedGrant = CreateTestObject();

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();

            var entity = persistedGrant.ToEntity();
            repo.Insert(entity);

            IList<PersistedGrant> foundPersistedGrants;

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            foundPersistedGrants = (await store.GetAllAsync(new PersistedGrantFilter { SubjectId = persistedGrant.SubjectId })).ToList();

            Assert.NotNull(foundPersistedGrants);
            Assert.NotEmpty(foundPersistedGrants);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter()
        {
            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();
            //clean last test data
            repo.Where(a => a.SubjectId == "sub1").ToDelete().ExecuteAffrows();
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t1").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t2").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t1").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t2").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t1").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t2").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t1").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t2").ToEntity());
            repo.Insert(CreateTestObject(sub: "sub1", clientId: "c3", sid: "s3", type: "t3").ToEntity());
            repo.Insert(CreateTestObject().ToEntity());


            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());

            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1"
            })).ToList().Count.Should().Be(9);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub10"
            })).ToList().Count.Should().Be(0);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1"
            })).ToList().Count.Should().Be(4);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c2"
            })).ToList().Count.Should().Be(4);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c3"
            })).ToList().Count.Should().Be(1);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c4"
            })).ToList().Count.Should().Be(0);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1"
            })).ToList().Count.Should().Be(2);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c3",
                SessionId = "s1"
            })).ToList().Count.Should().Be(0);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1",
                Type = "t1"
            })).ToList().Count.Should().Be(1);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1",
                Type = "t3"
            })).ToList().Count.Should().Be(0);

        }

        [Fact]
        public async Task RemoveAsync_WhenKeyOfExistingReceived_ExpectGrantDeleted()
        {
            var persistedGrant = CreateTestObject();

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();

            var entity = persistedGrant.ToEntity();

            repo.Insert(entity);

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store.RemoveAsync(persistedGrant.Key);

            var foundGrant = repo.Select.Where(x => x.Key == persistedGrant.Key).First();
            Assert.Null(foundGrant);

        }

        [Fact]
        public async Task RemoveAllAsync_WhenSubIdAndClientIdOfExistingReceived_ExpectGrantDeleted()
        {
            var persistedGrant = CreateTestObject();

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();

            var entity = persistedGrant.ToEntity();

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = persistedGrant.SubjectId,
                ClientId = persistedGrant.ClientId
            });

            var foundGrant = repo.Select.Where(x => x.Key == persistedGrant.Key).First();
            Assert.Null(foundGrant);

        }

        [Fact]
        public async Task RemoveAllAsync_WhenSubIdClientIdAndTypeOfExistingReceived_ExpectGrantDeleted()
        {
            var persistedGrant = CreateTestObject();

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();

            var entity = persistedGrant.ToEntity();

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = persistedGrant.SubjectId,
                ClientId = persistedGrant.ClientId,
                Type = persistedGrant.Type
            });

            var foundGrant = repo.Select.Where(x => x.Key == persistedGrant.Key).First();
            Assert.Null(foundGrant);

        }


        [Fact]
        public async Task RemoveAllAsync_Should_Filter()
        {
            void PopulateDb()
            {
                var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();
                repo.Where(a => a.SubjectId == "sub2").ToDelete().ExecuteAffrows();
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c1", sid: "s1", type: "t1").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c1", sid: "s1", type: "t2").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c1", sid: "s2", type: "t1").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c1", sid: "s2", type: "t2").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c2", sid: "s1", type: "t1").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c2", sid: "s1", type: "t2").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c2", sid: "s2", type: "t1").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c2", sid: "s2", type: "t2").ToEntity());
                repo.Insert(CreateTestObject(sub: "sub2", clientId: "c3", sid: "s3", type: "t3").ToEntity());
            }

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();
            var query = repo.Where(a => a.SubjectId == "sub2");

            PopulateDb();
            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2"
            });
            query.Count().Should().Be(0);


            PopulateDb();
            var store1 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store1.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub10"
            });
            query.Count().Should().Be(9);


            PopulateDb();
            var store2 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store2.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c1"
            });
            query.Count().Should().Be(5);


            PopulateDb();
            var store3 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store3.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c2"
            });
            query.Count().Should().Be(5);


            PopulateDb();
            var store4 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store4.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c3"
            });
            query.Count().Should().Be(8);


            PopulateDb();
            var store5 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store5.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c4"
            });
            query.Count().Should().Be(9);


            PopulateDb();
            var store6 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store6.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c1",
                SessionId = "s1"
            });
            query.Count().Should().Be(7);


            PopulateDb();
            var store7 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store7.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c3",
                SessionId = "s1"
            });
            query.Count().Should().Be(9);


            PopulateDb();
            var store8 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store8.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c1",
                SessionId = "s1",
                Type = "t1"
            });
            query.Count().Should().Be(8);


            PopulateDb();
            var store9 = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store9.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2",
                ClientId = "c1",
                SessionId = "s1",
                Type = "t3"
            });
            query.Count().Should().Be(9);

        }

        [Fact]
        public async Task Store_should_create_new_record_if_key_does_not_exist()
        {
            var persistedGrant = CreateTestObject();

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();

            var foundGrant = repo.Select.Where(x => x.Key == persistedGrant.Key).First();
            Assert.Null(foundGrant);

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            await store.StoreAsync(persistedGrant);

            var foundGrant2 = repo.Select.Where(x => x.Key == persistedGrant.Key).First();
            Assert.NotNull(foundGrant2);
        }

        [Fact]
        public async Task Store_should_update_record_if_key_already_exists()
        {
            var persistedGrant = CreateTestObject();

            var repo = g.operationalDb.GetRepository<Storage.Entities.PersistedGrant>();

            var entity = persistedGrant.ToEntity();

            repo.Insert(entity);


            var newDate = persistedGrant.Expiration.Value.AddHours(1);

            var store = new PersistedGrantStore(g.operationalDb, FakeLogger<PersistedGrantStore>.Create());
            persistedGrant.Expiration = newDate;
            await store.StoreAsync(persistedGrant);

            var foundGrant = repo.Select.Where(x => x.Key == persistedGrant.Key).First();
            Assert.NotNull(foundGrant);
            Assert.Equal(newDate, persistedGrant.Expiration);

        }
    }
}
