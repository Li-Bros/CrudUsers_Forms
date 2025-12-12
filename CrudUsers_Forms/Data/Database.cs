using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CrudUsers_Forms.Data;

internal static class Database
{
    private static readonly Lazy<IConfigurationRoot> Configuration = new(() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build());

    private static string ConnectionString =>
        Configuration.Value.GetConnectionString("UsersCrudConnection")
        ?? throw new InvalidOperationException("Connection string UsersCrudConnection not found.");

    public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);
}
