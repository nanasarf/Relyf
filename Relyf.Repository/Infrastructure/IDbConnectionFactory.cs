using System.Data;

namespace Relyf.Repository.Infrastructure;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
