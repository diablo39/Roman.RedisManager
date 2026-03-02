using System.ComponentModel.DataAnnotations;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.Server;

namespace Roman.RedisManager.Tests.Domain.Configuration
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
                        GroupType = GroupType.Standalone
                    }
                }
            };

            var guid = Guid.Parse("abc00000-0000-0000-0000-000000000000");
            var result = config.ResolveServerGroup(guid);

            result.ShouldNotBeNull();
            result.Name.ShouldBe("foo");
            result.Id.ShouldBe(Guid.Parse("abc00000-0000-0000-0000-000000000000"));
            result.ConnectionString.ShouldBe("conn");
            result.GroupType.ShouldBe(GroupType.Standalone);
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
                        GroupType = GroupType.Standalone
                    }
                }
            };

            var ex = Should.Throw<ArgumentException>(() => config.ResolveServerGroup(Guid.Empty));
            ex.ParamName.ShouldBe("groupId");
        }

        [Fact]
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
                        GroupType = GroupType.Standalone
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
                GroupType = GroupType.Standalone
            };

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(config, context, results, validateAllProperties: true).ShouldBeFalse();
            results.ShouldContain(r => r.MemberNames.Contains(nameof(RedisServerGroupConfiguration.Id)));
        }

        [Fact]
        public void ResolveServerGroup_WithMultipleGroups_ReturnsCorrectGroup()
        {
            var targetId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            var config = new RedisConfiguration
            {
                ServerGroups = new[]
                {
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        Name = "other",
                        ConnectionString = "conn-other",
                        GroupType = GroupType.Standalone
                    },
                    new RedisServerGroupConfiguration
                    {
                        Id = targetId,
                        Name = "target",
                        ConnectionString = "conn-target",
                        GroupType = GroupType.Cluster
                    }
                }
            };

            var result = config.ResolveServerGroup(targetId);

            result.Name.ShouldBe("target");
            result.Id.ShouldBe(targetId);
            result.ConnectionString.ShouldBe("conn-target");
            result.GroupType.ShouldBe(GroupType.Cluster);
        }

        [Fact]
        public void ResolveServerGroup_WhenServerGroupsIsNull_ThrowsInvalidOperationException()
        {
            var config = new RedisConfiguration { ServerGroups = null! };

            Should.Throw<InvalidOperationException>(() => config.ResolveServerGroup(Guid.NewGuid()));
        }

        [Fact]
        public void ServerGroupConfiguration_MissingName_FailsValidation()
        {
            var config = new RedisServerGroupConfiguration
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                ConnectionString = "conn",
                GroupType = GroupType.Standalone
            };

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(config, context, results, validateAllProperties: true).ShouldBeFalse();
            results.ShouldContain(r => r.MemberNames.Contains(nameof(RedisServerGroupConfiguration.Name)));
        }

        [Fact]
        public void ServerGroupConfiguration_MissingConnectionString_FailsValidation()
        {
            var config = new RedisServerGroupConfiguration
            {
                Id = Guid.NewGuid(),
                Name = "valid-name",
                ConnectionString = string.Empty,
                GroupType = GroupType.Standalone
            };

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(config, context, results, validateAllProperties: true).ShouldBeFalse();
            results.ShouldContain(r => r.MemberNames.Contains(nameof(RedisServerGroupConfiguration.ConnectionString)));
        }

        [Fact]
        public void ServerGroupConfiguration_ValidObject_PassesValidation()
        {
            var config = new RedisServerGroupConfiguration
            {
                Id = Guid.NewGuid(),
                Name = "valid-name",
                ConnectionString = "localhost:6379",
                GroupType = GroupType.Standalone
            };

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(config, context, results, validateAllProperties: true).ShouldBeTrue();
            results.ShouldBeEmpty();
        }
    }
}