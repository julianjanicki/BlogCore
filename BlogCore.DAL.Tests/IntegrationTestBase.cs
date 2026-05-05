namespace BlogCore.DAL.Tests;

using BlogCore.DAL.Data;
using BlogCore.DAL.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;

[TestClass]
public abstract class IntegrationTestBase
{
    protected static readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("StrongPassword123!")
        .Build();

    protected BlogContext _context = null!;
    protected BlogRepository _repository = null!;
    private Respawner _respawner = null!;

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        _dbContainer.StartAsync().GetAwaiter().GetResult();
    }

    [TestInitialize]
    public async Task Setup()
    {
        var connectionString = _dbContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<BlogContext>()
            .UseSqlServer(connectionString)
            .Options;
        _context = new BlogContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new BlogRepository(_context);

        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                TablesToIgnore = new[] { new Table("__EFMigrationsHistory") }
            });
        }

        await ResetDatabaseAsync();
    }

    protected async Task ResetDatabaseAsync()
    {
        if (_respawner != null)
        {
            var connectionString = _dbContainer.GetConnectionString();
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        _dbContainer.StopAsync().GetAwaiter().GetResult();
    }
}
