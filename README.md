# EntityFrameworkMock

[![Build status](https://ci.appveyor.com/api/projects/status/5ung41elf64ahshg/branch/master?svg=true)](https://ci.appveyor.com/project/huysentruitw/entity-framework-mock/branch/master)

Easy Mock wrapper for mocking EF6 DbContext and DbSet in your unit-tests. Integrates with Moq or NSubstitute.

For mocking Entity Framework Core (EF Core) see https://github.com/huysentruitw/entity-framework-core-mock

## Get it on NuGet

### Moq integration

    PM> Install-Package EntityFrameworkMock.Moq

### NSubstitute integration

    PM> Install-Package EntityFrameworkMock.NSubstitute

## Supports

* In-memory storage of test data
* Querying of in-memory test data (synchronous or asynchronous)
* Tracking of updates, inserts and deletes of in-memory test data
* Emulation of `SaveChanges` and `SaveChangesAsync` (only saves tracked changes to the mocked in-memory DbSet when one of these methods are called)
* Auto-increment identity columns, annotated by the `[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]` attribute
* Primary key on multiple columns, annotated by the `[Key, Column(Order = X)]` attributes
* Throwing a `DbUpdateException` when inserting 2 or more entities with the same primary key while calling `SaveChanges` / `SaveChangesAsync` (emulating EF behavior)
* Throwing a `DbUpdateConcurrencyException` when removing a model that no longer exists (emulating EF behavior)

For the Moq version, you can use all known [Moq](https://github.com/Moq/moq4/wiki/Quickstart) features, since both `DbSetMock` and `DbContextMock` inherit from `Mock<DbSet>` and `Mock<DbContext>` respectively.

## Example usage

    public class User
    {
        [Key, Column(Order = 0)]
        public Guid Id { get; set; }

        public string FullName { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public virtual DbSet<User> Users { get; set; }
    }

    [TestFixture]
    public class MyTests
    {
        var initialEntities = new[]
            {
                new User { Id = Guid.NewGuid(), FullName = "Eric Cartoon" },
                new User { Id = Guid.NewGuid(), FullName = "Billy Jewel" },
            };
            
        var dbContextMock = new DbContextMock<TestDbContext>("fake connectionstring");
        var usersDbSetMock = dbContextMock.CreateDbSetMock(x => x.Users, initialEntities);
        
        // Pass dbContextMock.Object to the class/method you want to test
        
        // Query dbContextMock.Object.Users to see if certain users were added or removed
        // or use Mock Verify functionality to verify if certain methods were called: usersDbSetMock.Verify(x => x.Add(...), Times.Once);
    }
