using System.Data;
using System.Data.SqlClient;

namespace Blog.Data
{
    public class AppDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionstring;
        
        public AppDbContext(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.connectionstring = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(connectionstring);
    }
}
