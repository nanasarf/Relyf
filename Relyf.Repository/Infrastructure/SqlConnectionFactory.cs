using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Relyf.Repository.Infrastructure;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _conn;

    public SqlConnectionFactory(IOptions<DbConnectionOptions> opts)
    {
        _conn = opts.Value.Default ?? throw new ArgumentNullException(nameof(opts));
    }

    public IDbConnection Create()
    {
        // Do NOT Open() here; let callers open/close in a using block.
        return new SqlConnection(_conn);
    }
}
