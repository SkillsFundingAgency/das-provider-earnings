﻿using System.Globalization;
using Microsoft.SqlServer.Dac;
using Polly;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public static class SqlDatabaseModel
{
//#if DEBUG
    public const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
//#else
//    public const string ConnectionString = @"Data Source=127.0.0.1\sql_server_container,1433;Initial Catalog=master;User Id=sa;Password=P1peline;TrustServerCertificate=True";
//#endif
    public const string DatabaseProjectName = "SFA.DAS.Funding.ApprenticeshipEarnings.Database";
    private static string _dacpacFileLocation;

    public static void Update()
    {
        SetDacpacLocation();

        var modelNeedsUpdating = false;
        try
        {
            modelNeedsUpdating = DacpacFileHasBeenModified();
            Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(Update)}: ModelNeedsUpdating={modelNeedsUpdating}");
        }
        catch (Exception ex)
        {
            modelNeedsUpdating = true;
            Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(Update)}: Exception={ex.Message}");
        }

        if (!modelNeedsUpdating) return;

        PublishModel();
        SaveModifiedDateTime();
    }

    private static bool DacpacFileHasBeenModified()
    {
        var current = File.GetLastWriteTime(_dacpacFileLocation);
        var previous = GetSavedModifiedDateTime();

        return Math.Floor((current - previous).TotalSeconds) != 0;
    }

    private static DateTime GetSavedModifiedDateTime()
    {
        var stored = File.ReadAllText(_dacpacFileLocation + ".tmp");

        return DateTime.TryParse(stored, out var result) ? result : DateTime.MinValue;
    }

    private static void SaveModifiedDateTime()
    {
        try
        {
            var current = File.GetLastWriteTime(_dacpacFileLocation);
            File.WriteAllText(_dacpacFileLocation + ".tmp", current.ToString(CultureInfo.CurrentCulture));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(SaveModifiedDateTime)}: Exception={ex.Message}");
        }
    }

    private static void SetDacpacLocation()
    {
#if DEBUG
        const string environment = "debug";
#else
            const string environment = "release";
#endif
        _dacpacFileLocation = Path.Combine(
            Directory.GetCurrentDirectory().Substring(0,
                Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)),
            "src", "Database", "bin", environment, $"{DatabaseProjectName}.dacpac");

        if (!File.Exists(_dacpacFileLocation))
            throw new FileNotFoundException($"DACPAC file not found in: {_dacpacFileLocation}.  Rebuild the database project.");
    }

    private static void PublishModel()
    {
        using (var dbPackage = DacPackage.Load(_dacpacFileLocation))
        {
            var services = new DacServices(ConnectionString);

            var policy = Policy
                .Handle<DacServicesException>()
                .WaitAndRetry(Enumerable.Repeat(TimeSpan.FromMilliseconds(250), 40));

            policy.Execute(() =>
            {
                Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(PublishModel)} attempted");
                var options = new DacDeployOptions() { BlockOnPossibleDataLoss = false };
                services.Deploy(dbPackage, "model", upgradeExisting: true, options);
            });
        }
    }
}