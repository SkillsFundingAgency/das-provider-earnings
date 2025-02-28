using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public class SqlDatabase : IDisposable
{
    private bool _isDisposed;
    private const string _source = "(localdb)\\MSSQLLocalDB";
    private const string _authentication = "Integrated Security=True";

    public DatabaseInfo DatabaseInfo { get; } = new DatabaseInfo();
    public ApprenticeshipEarningsDataContext DbContext { get; private set; }

    public SqlDatabase(string? dbName = null)
    {
        DatabaseInfo.SetDatabaseName(dbName ?? Guid.NewGuid().ToString());
        CreateTestDatabase();

        var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>()
            .UseSqlServer(new SqlConnection(DatabaseInfo.ConnectionString), optionsBuilder => optionsBuilder.CommandTimeout(7200)) //7200=2hours
            .Options;
        DbContext = new ApprenticeshipEarningsDataContext(options);
    }

    public async Task<ApprenticeshipModel?> GetApprenticeship(Guid apprenticeshipKey)
    {
        var apprenticeship = await DbContext.Apprenticeships
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfile)
            .ThenInclude(y => y.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfileHistory)
            .ThenInclude(y => y.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.Prices)
            .SingleOrDefaultAsync(x => x.Key == apprenticeshipKey);

        return apprenticeship;
    }

    private void CreateTestDatabase()
    {
        Directory.CreateDirectory("C:\\temp");
        var dbConnectionString = @$"Data Source={_source};Initial Catalog={DatabaseInfo.DatabaseName};{_authentication};MultipleActiveResultSets=True;Pooling=False;Connect Timeout=90;;Trusted_Connection=True;TrustServerCertificate=True";
        var masterConnectionString = @$"Data Source={_source};Initial Catalog=master;{_authentication};MultipleActiveResultSets=true;Trusted_Connection=True;TrustServerCertificate=True";

        DatabaseInfo.SetConnectionString(dbConnectionString);

        using var dbConn = new SqlConnection(masterConnectionString);
        try
        {
            var sql = @$"CREATE DATABASE [{DatabaseInfo.DatabaseName}] ON PRIMARY
                     (NAME = [{DatabaseInfo.DatabaseName}_Data],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.mdf')
                      LOG ON (NAME = [{DatabaseInfo.DatabaseName}_Log],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.ldf')";



            using var cmd = new SqlCommand(sql, dbConn);
            dbConn.Open();
            cmd.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(CreateTestDatabase)} failed. Exception={ex}");
        }
        finally
        {
            if (dbConn.State == ConnectionState.Open)
            {
                dbConn.Close();
            }
        }
    }

    private void DeleteTestDatabase()
    {
        try
        {
            var files = new List<string>();
            using var dbConn = new SqlConnection(DatabaseInfo.ConnectionString);
            using var cmd = new SqlCommand("SELECT DB_NAME()", dbConn);
            dbConn.Open();
            var dbName = cmd.ExecuteScalar();
            cmd.CommandText = "SELECT filename FROM sysfiles";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    files.Add((string)reader["filename"]);
                }
            }
            cmd.CommandText = $"ALTER DATABASE [{dbName}] SET OFFLINE WITH ROLLBACK IMMEDIATE";
            cmd.ExecuteNonQuery();
            cmd.CommandText = $"EXEC sp_detach_db '{dbName}', 'true';";
            cmd.ExecuteNonQuery();
            dbConn.Close();

            files.ForEach(DeleteFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(SqlDatabase)}] {nameof(DeleteTestDatabase)} exception thrown: {ex.Message}");
            throw ex;
        }
    }

    private static void DeleteFile(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(SqlDatabase)}] {nameof(DeleteFile)} exception thrown: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            DeleteTestDatabase();
        }

        _isDisposed = true;
    }
}