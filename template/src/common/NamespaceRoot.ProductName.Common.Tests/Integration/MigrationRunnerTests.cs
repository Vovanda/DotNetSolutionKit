using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NamespaceRoot.ProductName.Common.Infrastructure.Persistence.EntityFramework;
//#if (Database != "mssql")
using Npgsql;
//#endif
//#if (Database != "postgres")
using Microsoft.Data.SqlClient;
//#endif

namespace NamespaceRoot.ProductName.Common.Tests.Integration;

/// <summary>
/// The first start of a service on an empty database: its migrations apply, and the log has no error in
/// it. Npgsql's EF 10 provider reads the history table before it creates it and logs that failed read as an
/// error; an operator reading the first start's log would look for a fault that is not there.
/// </summary>
internal abstract class MigrationRunnerTests
{
    private string _database = null!;
    private string _connection = null!;

    protected sealed class Db(DbContextOptions<Db> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Thing>().ToTable("things");
    }

    protected sealed class Thing
    {
        public Guid Id { get; set; }
    }

    [DbContext(typeof(Db))]
    [Migration("20260101000000_Things")]
    private sealed class Things : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.CreateTable(
                name: "things",
                columns: table => new { Id = table.Column<Guid>(nullable: false) },
                constraints: table => table.PrimaryKey("PK_things", x => x.Id));
    }

    protected abstract Task<string> CreateDatabaseAsync(string name);

    protected abstract Task DropDatabaseAsync(string name);

    protected abstract DbContextOptionsBuilder<Db> Options(string connection);

    [SetUp]
    public async Task CreateDatabase()
    {
        _database = $"migrations_{Guid.NewGuid():N}";
        _connection = await CreateDatabaseAsync(_database);
    }

    [TearDown]
    public Task DropDatabase() => DropDatabaseAsync(_database);

    [Test(Description = "On an empty database the migrations apply and nothing is logged as an error")]
    public void Should_LogNoError_When_TheDatabaseIsEmpty()
    {
        var errors = new ConcurrentQueue<string>();
        using var loggerFactory = LoggerFactory.Create(logging => logging.AddProvider(new ErrorRecorder(errors)));
        using var db = new Db(Options(_connection).UseLoggerFactory(loggerFactory).Options);

        MigrationRunner.RunMigrations(db, NullLogger.Instance);

        db.Database.GetAppliedMigrations().ShouldBe(["20260101000000_Things"]);
        errors.ShouldBeEmpty();
    }

    private sealed class ErrorRecorder(ConcurrentQueue<string> errors) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new Recorder(categoryName, errors);

        public void Dispose() { }

        private sealed class Recorder(string category, ConcurrentQueue<string> errors) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                    errors.Enqueue($"{category}: {formatter(state, exception)}");
            }
        }
    }
}
//#if (Database != "mssql")

[TestFixture]
[Category(TestCategories.Integration)]
internal sealed class MigrationRunnerOnPostgresTests : MigrationRunnerTests
{
    protected override async Task<string> CreateDatabaseAsync(string name)
    {
        var admin = Postgres.ConnectionString();
        await using (var connection = new NpgsqlConnection(admin))
        {
            await connection.OpenAsync();
            await new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection).ExecuteNonQueryAsync();
        }

        return new NpgsqlConnectionStringBuilder(admin) { Database = name }.ToString();
    }

    protected override async Task DropDatabaseAsync(string name)
    {
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(Postgres.ConnectionString());
        await connection.OpenAsync();
        await new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", connection).ExecuteNonQueryAsync();
    }

    protected override DbContextOptionsBuilder<Db> Options(string connection) =>
        new DbContextOptionsBuilder<Db>().UseNpgsql(connection);
}
//#endif
//#if (Database != "postgres")

[TestFixture]
[Category(TestCategories.Integration)]
internal sealed class MigrationRunnerOnSqlServerTests : MigrationRunnerTests
{
    protected override async Task<string> CreateDatabaseAsync(string name)
    {
        var admin = SqlServer.ConnectionString();
        await using (var connection = new SqlConnection(admin))
        {
            await connection.OpenAsync();
            await new SqlCommand($"CREATE DATABASE [{name}]", connection).ExecuteNonQueryAsync();
        }

        return new SqlConnectionStringBuilder(admin) { InitialCatalog = name }.ConnectionString;
    }

    protected override async Task DropDatabaseAsync(string name)
    {
        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(SqlServer.ConnectionString());
        await connection.OpenAsync();
        await new SqlCommand(
            $"IF DB_ID(N'{name}') IS NOT NULL BEGIN ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]; END",
            connection).ExecuteNonQueryAsync();
    }

    protected override DbContextOptionsBuilder<Db> Options(string connection) =>
        new DbContextOptionsBuilder<Db>().UseSqlServer(connection);
}
//#endif
