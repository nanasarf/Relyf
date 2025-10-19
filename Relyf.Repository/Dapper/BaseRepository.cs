using System.Data;
using System.Data.Common;              // <-- add this
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public abstract class BaseRepository
{
    private readonly IDbConnectionFactory _factory;
    protected BaseRepository(IDbConnectionFactory factory) => _factory = factory;

    protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> work)
    {
        using var conn = _factory.Create();
        await OpenAsyncSafe(conn);
        return await work(conn);
    }

    protected async Task WithConnection(Func<IDbConnection, Task> work)
    {
        using var conn = _factory.Create();
        await OpenAsyncSafe(conn);
        await work(conn);
    }

    private static async Task OpenAsyncSafe(IDbConnection conn)
    {
        if (conn is DbConnection db)            // SqlConnection, NpgsqlConnection, etc.
            await db.OpenAsync();
        else
            conn.Open();                         // fallback for providers without async
    }
    protected static string EscapeLike(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        // escape SQL LIKE metacharacters: \ % _ [ ]
        return input
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_")
            .Replace("[", @"\[")
            .Replace("]", @"\]");
    }
}
