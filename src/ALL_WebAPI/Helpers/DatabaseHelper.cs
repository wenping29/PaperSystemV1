namespace PaperSystemApi.Helpers;

/// <summary>
/// 数据库类型枚举
/// </summary>
public enum DatabaseProvider
{
    MySql,
    Sqlite,
    Oracle,
    PostgreSql,
    SqlServer
}

/// <summary>
/// 数据库配置选项
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.MySql;
    public bool EnableDetailedErrors { get; set; } = true;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

/// <summary>
/// 数据库配置帮助类
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// 根据数据库类型配置 DbContextOptionsBuilder
    /// </summary>
    public static DbContextOptionsBuilder ConfigureDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        DatabaseProvider provider,
        string connectionString,
        IServiceProvider? serviceProvider = null,
        Action<DbContextOptionsBuilder>? additionalConfiguration = null)
    {
        // 应用数据库特定的配置
        switch (provider)
        {
            case DatabaseProvider.MySql:
                optionsBuilder.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mysqlOptions => ConfigureMySqlOptions(mysqlOptions));
                break;

            case DatabaseProvider.Sqlite:
                optionsBuilder.UseSqlite(connectionString,
                    sqliteOptions => ConfigureSqliteOptions(sqliteOptions));
                break;

            case DatabaseProvider.Oracle:
                optionsBuilder.UseOracle(connectionString,
                    oracleOptions => ConfigureOracleOptions(oracleOptions));
                break;

            case DatabaseProvider.PostgreSql:
                throw new NotSupportedException("PostgreSQL is not implemented yet.");

            case DatabaseProvider.SqlServer:
                throw new NotSupportedException("SQL Server is not implemented yet.");

            default:
                throw new NotSupportedException($"Database provider {provider} is not supported.");
        }

        // 应用额外配置
        additionalConfiguration?.Invoke(optionsBuilder);

        return optionsBuilder;
    }

    /// <summary>
    /// 配置 MySQL 特定选项
    /// </summary>
    private static void ConfigureMySqlOptions(
        Microsoft.EntityFrameworkCore.Infrastructure.MySqlDbContextOptionsBuilder options)
    {
        options.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        options.CommandTimeout(30);
        options.MaxBatchSize(100);
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }

    /// <summary>
    /// 配置 SQLite 特定选项
    /// </summary>
    private static void ConfigureSqliteOptions(
        Microsoft.EntityFrameworkCore.Infrastructure.SqliteDbContextOptionsBuilder options)
    {
        // SQLite 特定配置
        options.CommandTimeout(30);
    }

    /// <summary>
    /// 配置 Oracle 特定选项
    /// </summary>
    private static void ConfigureOracleOptions(
        Oracle.EntityFrameworkCore.Infrastructure.OracleDbContextOptionsBuilder options)
    {
        // 不设置特定版本，使用默认
    }

    /// <summary>
    /// 获取 SQLite 数据库连接字符串（使用本地文件）
    /// </summary>
    public static string GetSqliteConnectionString(string dbName)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", $"{dbName}.db");
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return $"Data Source={dbPath}";
    }

    /// <summary>
    /// 获取默认的连接字符串键名
    /// </summary>
    public static string GetConnectionStringKey(string contextName)
    {
        return contextName.Replace("DbContext", "Database");
    }
}
