using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Domain.Entities;
using Xunit;
using Shouldly;

namespace Roman.RedisManager.Tests
{
    public class RedisConfigurationTests
    {
        [Fact]
        public void ResolveServerGroup_WithValidGuid_ReturnsConfiguration()
        {
            var config = new RedisConfiguration
            {
                ServerGroups = new[]
                {
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("abc00000-0000-0000-0000-000000000000"),
                        Name = "foo",
                        ConnectionString = "conn",
                        GroupType = Domain.Entities.GroupType.Standalone
                    }
                }
            };

            var guid = Guid.Parse("abc00000-0000-0000-0000-000000000000");
            var result = config.ResolveServerGroup(guid);

            result.ShouldNotBeNull();
            result.Name.ShouldBe("foo");
        }

        [Fact]
        public void ResolveServerGroup_WithEmptyGuid_ThrowsArgumentException()
        {
            var config = new RedisConfiguration
            {
                ServerGroups = new[]
                {
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Name = "foo",
                        ConnectionString = "conn",
                        GroupType = Domain.Entities.GroupType.Standalone
                    }
                }
            };

            Should.Throw<ArgumentException>(() => config.ResolveServerGroup(Guid.Empty));
        }

        [Fact]
        // invalid GUID string case removed because method now takes Guid
        // client code should parse before calling ResolveServerGroup
        public void ResolveServerGroup_WithUnknownGuid_ThrowsKeyNotFoundException()
        {
            var config = new RedisConfiguration
            {
                ServerGroups = new[]
                {
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "foo",
                        ConnectionString = "conn",
                        GroupType = Domain.Entities.GroupType.Standalone
                    }
                }
            };

            Should.Throw<KeyNotFoundException>(() => config.ResolveServerGroup(Guid.Parse("22222222-2222-2222-2222-222222222222")));
        }


        [Fact]
        public void ServerGroupConfiguration_WithEmptyId_FailsValidation()
        {
            var config = new RedisServerGroupConfiguration
            {
                Id = Guid.Empty,
                Name = "foo",
                ConnectionString = "conn",
                GroupType = Domain.Entities.GroupType.Standalone
            };

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(config, context, results, validateAllProperties: true).ShouldBeFalse();
            results.ShouldContain(r => r.MemberNames.Contains(nameof(RedisServerGroupConfiguration.Id)));
        }
    }
}