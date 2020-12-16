using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Stores.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Xunit;
using IdentityServer4.Fsql.Storage.Stores;

namespace IdentityServer4.Fsql.IntegrationTests.Stores
{
    public class DeviceFlowStoreTests
    {

        private readonly IPersistentGrantSerializer serializer = new PersistentGrantSerializer();

        [Fact]
        public async Task StoreDeviceAuthorizationAsync_WhenSuccessful_ExpectDeviceCodeAndUserCodeStored()
        {
            var deviceCode = Guid.NewGuid().ToString();
            var userCode = Guid.NewGuid().ToString();
            var data = new DeviceCode
            {
                ClientId = Guid.NewGuid().ToString(),
                CreationTime = DateTime.UtcNow,
                Lifetime = 300
            };

            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            await store.StoreDeviceAuthorizationAsync(deviceCode, userCode, data);

            var foundDeviceFlowCodes = g.operationalDb.Select<Storage.Entities.DeviceFlowCodes>().Where(x => x.DeviceCode == deviceCode).First();

            foundDeviceFlowCodes.Should().NotBeNull();
            foundDeviceFlowCodes?.DeviceCode.Should().Be(deviceCode);
            foundDeviceFlowCodes?.UserCode.Should().Be(userCode);

        }

        [Fact]
        public async Task StoreDeviceAuthorizationAsync_WhenSuccessful_ExpectDataStored()
        {
            var deviceCode = Guid.NewGuid().ToString();
            var userCode = Guid.NewGuid().ToString();
            var data = new DeviceCode
            {
                ClientId = Guid.NewGuid().ToString(),
                CreationTime = DateTime.UtcNow,
                Lifetime = 300
            };


            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            await store.StoreDeviceAuthorizationAsync(deviceCode, userCode, data);


            var foundDeviceFlowCodes = g.operationalDb.Select<Storage.Entities.DeviceFlowCodes>().Where(x => x.DeviceCode == deviceCode).First();

            foundDeviceFlowCodes.Should().NotBeNull();
            var deserializedData = new PersistentGrantSerializer().Deserialize<DeviceCode>(foundDeviceFlowCodes?.Data);

            deserializedData.CreationTime.Should().BeCloseTo(data.CreationTime);
            deserializedData.ClientId.Should().Be(data.ClientId);
            deserializedData.Lifetime.Should().Be(data.Lifetime);

        }

        [Fact]
        public async Task StoreDeviceAuthorizationAsync_WhenUserCodeAlreadyExists_ExpectException()
        {
            var existingUserCode = $"user_{Guid.NewGuid().ToString()}";
            var deviceCodeData = new DeviceCode
            {
                ClientId = "device_flow",
                RequestedScopes = new[] { "openid", "api1" },
                CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                Lifetime = 300,
                IsOpenId = true,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(
                    new List<Claim> { new Claim(JwtClaimTypes.Subject, $"sub_{Guid.NewGuid().ToString()}") }))
            };


            var entity = new Storage.Entities.DeviceFlowCodes
            {
                DeviceCode = $"device_{Guid.NewGuid().ToString()}",
                UserCode = existingUserCode,
                ClientId = deviceCodeData.ClientId,
                SubjectId = deviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = deviceCodeData.CreationTime,
                Expiration = deviceCodeData.CreationTime.AddSeconds(deviceCodeData.Lifetime),
                Data = serializer.Serialize(deviceCodeData)
            };
            g.operationalDb.Insert(entity).ExecuteAffrows();


            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());


            await Assert.ThrowsAsync<Exception>(() =>
                store.StoreDeviceAuthorizationAsync($"device_{Guid.NewGuid().ToString()}", existingUserCode, deviceCodeData));

        }


        [Fact]
        public async Task StoreDeviceAuthorizationAsync_WhenDeviceCodeAlreadyExists_ExpectException()
        {
            var existingDeviceCode = $"device_{Guid.NewGuid().ToString()}";
            var deviceCodeData = new DeviceCode
            {
                ClientId = "device_flow",
                RequestedScopes = new[] { "openid", "api1" },
                CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                Lifetime = 300,
                IsOpenId = true,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(
                    new List<Claim> { new Claim(JwtClaimTypes.Subject, $"sub_{Guid.NewGuid().ToString()}") }))
            };

            var repo = g.operationalDb.GetRepository<Storage.Entities.DeviceFlowCodes>();

            var entity = new Storage.Entities.DeviceFlowCodes
            {
                DeviceCode = existingDeviceCode,
                UserCode = $"user_{Guid.NewGuid().ToString()}",
                ClientId = deviceCodeData.ClientId,
                SubjectId = deviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = deviceCodeData.CreationTime,
                Expiration = deviceCodeData.CreationTime.AddSeconds(deviceCodeData.Lifetime),
                Data = serializer.Serialize(deviceCodeData)
            };
            repo.Insert(entity);

            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());

            await Assert.ThrowsAsync<Exception>(() =>
                                    store.StoreDeviceAuthorizationAsync(existingDeviceCode, $"user_{Guid.NewGuid().ToString()}", deviceCodeData));

        }

        [Fact]
        public async Task FindByUserCodeAsync_WhenUserCodeExists_ExpectDataRetrievedCorrectly()
        {
            var testDeviceCode = $"device_{Guid.NewGuid().ToString()}";
            var testUserCode = $"user_{Guid.NewGuid().ToString()}";

            var expectedSubject = $"sub_{Guid.NewGuid().ToString()}";
            var expectedDeviceCodeData = new DeviceCode
            {
                ClientId = "device_flow",
                RequestedScopes = new[] { "openid", "api1" },
                CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                Lifetime = 300,
                IsOpenId = true,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(JwtClaimTypes.Subject, expectedSubject) }))
            };

            var repo = g.operationalDb.GetRepository<Storage.Entities.DeviceFlowCodes>();

            var entity = new Storage.Entities.DeviceFlowCodes
            {
                DeviceCode = testDeviceCode,
                UserCode = testUserCode,
                ClientId = expectedDeviceCodeData.ClientId,
                SubjectId = expectedDeviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = expectedDeviceCodeData.CreationTime,
                Expiration = expectedDeviceCodeData.CreationTime.AddSeconds(expectedDeviceCodeData.Lifetime),
                Data = serializer.Serialize(expectedDeviceCodeData)
            };
            repo.Insert(entity);


            DeviceCode code;

            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            code = await store.FindByUserCodeAsync(testUserCode);


            code.Should().BeEquivalentTo(expectedDeviceCodeData,
                assertionOptions => assertionOptions.Excluding(x => x.Subject));

            code.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject && x.Value == expectedSubject).Should().NotBeNull();
        }

        [Fact]
        public async Task FindByUserCodeAsync_WhenUserCodeDoesNotExist_ExpectNull()
        {
            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            var code = await store.FindByUserCodeAsync($"user_{Guid.NewGuid().ToString()}");
            code.Should().BeNull();
        }

        [Fact]
        public async Task FindByDeviceCodeAsync_WhenDeviceCodeExists_ExpectDataRetrievedCorrectly()
        {
            var testDeviceCode = $"device_{Guid.NewGuid().ToString()}";
            var testUserCode = $"user_{Guid.NewGuid().ToString()}";

            var expectedSubject = $"sub_{Guid.NewGuid().ToString()}";
            var expectedDeviceCodeData = new DeviceCode
            {
                ClientId = "device_flow",
                RequestedScopes = new[] { "openid", "api1" },
                CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                Lifetime = 300,
                IsOpenId = true,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(JwtClaimTypes.Subject, expectedSubject) }))
            };

            var repo = g.operationalDb.GetRepository<Storage.Entities.DeviceFlowCodes>();
            var entity = new Storage.Entities.DeviceFlowCodes
            {
                DeviceCode = testDeviceCode,
                UserCode = testUserCode,
                ClientId = expectedDeviceCodeData.ClientId,
                SubjectId = expectedDeviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = expectedDeviceCodeData.CreationTime,
                Expiration = expectedDeviceCodeData.CreationTime.AddSeconds(expectedDeviceCodeData.Lifetime),
                Data = serializer.Serialize(expectedDeviceCodeData)
            };
            repo.Insert(entity);


            DeviceCode code;

            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            code = await store.FindByDeviceCodeAsync(testDeviceCode);


            code.Should().BeEquivalentTo(expectedDeviceCodeData,
                assertionOptions => assertionOptions.Excluding(x => x.Subject));

            code.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject && x.Value == expectedSubject).Should().NotBeNull();
        }

        [Fact]
        public async Task FindByDeviceCodeAsync_WhenDeviceCodeDoesNotExist_ExpectNull()
        {

            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            var code = await store.FindByDeviceCodeAsync($"device_{Guid.NewGuid().ToString()}");
            code.Should().BeNull();

        }

        [Fact]
        public async Task UpdateByUserCodeAsync_WhenDeviceCodeAuthorized_ExpectSubjectAndDataUpdated()
        {
            var testDeviceCode = $"device_{Guid.NewGuid().ToString()}";
            var testUserCode = $"user_{Guid.NewGuid().ToString()}";

            var expectedSubject = $"sub_{Guid.NewGuid().ToString()}";
            var unauthorizedDeviceCode = new DeviceCode
            {
                ClientId = "device_flow",
                RequestedScopes = new[] { "openid", "api1" },
                CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                Lifetime = 300,
                IsOpenId = true
            };

            var repo = g.operationalDb.GetRepository<Storage.Entities.DeviceFlowCodes>();
            var entity = new Storage.Entities.DeviceFlowCodes
            {
                DeviceCode = testDeviceCode,
                UserCode = testUserCode,
                ClientId = unauthorizedDeviceCode.ClientId,
                CreationTime = unauthorizedDeviceCode.CreationTime,
                Expiration = unauthorizedDeviceCode.CreationTime.AddSeconds(unauthorizedDeviceCode.Lifetime),
                Data = serializer.Serialize(unauthorizedDeviceCode)
            };
            repo.Insert(entity);


            var authorizedDeviceCode = new DeviceCode
            {
                ClientId = unauthorizedDeviceCode.ClientId,
                RequestedScopes = unauthorizedDeviceCode.RequestedScopes,
                AuthorizedScopes = unauthorizedDeviceCode.RequestedScopes,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(JwtClaimTypes.Subject, expectedSubject) })),
                IsAuthorized = true,
                IsOpenId = true,
                CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                Lifetime = 300
            };
            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            await store.UpdateByUserCodeAsync(testUserCode, authorizedDeviceCode);

            Storage.Entities.DeviceFlowCodes updatedCodes;
            updatedCodes = repo.Where(x => x.UserCode == testUserCode).First();

            // should be unchanged
            updatedCodes.DeviceCode.Should().Be(testDeviceCode);
            updatedCodes.ClientId.Should().Be(unauthorizedDeviceCode.ClientId);
            updatedCodes.CreationTime.Should().Be(unauthorizedDeviceCode.CreationTime);
            updatedCodes.Expiration.Should().Be(unauthorizedDeviceCode.CreationTime.AddSeconds(authorizedDeviceCode.Lifetime));

            // should be changed
            var parsedCode = serializer.Deserialize<DeviceCode>(updatedCodes.Data);
            parsedCode.Should().BeEquivalentTo(authorizedDeviceCode, assertionOptions => assertionOptions.Excluding(x => x.Subject));
            parsedCode.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject && x.Value == expectedSubject).Should().NotBeNull();
        }

        [Fact]
        public async Task RemoveByDeviceCodeAsync_WhenDeviceCodeExists_ExpectDeviceCodeDeleted()
        {
            var testDeviceCode = $"device_{Guid.NewGuid().ToString()}";
            var testUserCode = $"user_{Guid.NewGuid().ToString()}";

            var existingDeviceCode = new DeviceCode
            {
                ClientId = "device_flow",
                RequestedScopes = new[] { "openid", "api1" },
                CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                Lifetime = 300,
                IsOpenId = true
            };

            var repo = g.operationalDb.GetRepository<Storage.Entities.DeviceFlowCodes>();

            var entity = new Storage.Entities.DeviceFlowCodes
            {
                DeviceCode = testDeviceCode,
                UserCode = testUserCode,
                ClientId = existingDeviceCode.ClientId,
                CreationTime = existingDeviceCode.CreationTime,
                Expiration = existingDeviceCode.CreationTime.AddSeconds(existingDeviceCode.Lifetime),
                Data = serializer.Serialize(existingDeviceCode)
            };
            repo.Insert(entity);


            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            await store.RemoveByDeviceCodeAsync(testDeviceCode);


            repo.Select.Where(x => x.UserCode == testUserCode).First().Should().BeNull();

        }
        [Fact]
        public async Task RemoveByDeviceCodeAsync_WhenDeviceCodeDoesNotExists_ExpectSuccess()
        {
            var store = new DeviceFlowStore(g.operationalDb, new PersistentGrantSerializer(), FakeLogger<DeviceFlowStore>.Create());
            await store.RemoveByDeviceCodeAsync($"device_{Guid.NewGuid().ToString()}");
        }
    }
}