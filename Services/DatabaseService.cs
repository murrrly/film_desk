using Npgsql;
using System.Configuration;

namespace MovieApp_Adminpanel.Services
{
	public class DatabaseService
	{
		private readonly string _connectionString;

		public DatabaseService()
		{
			_connectionString = ConfigurationManager
				.ConnectionStrings["PostgresConnection"]
				.ConnectionString;
		}

		public NpgsqlConnection GetConnection()
		{
			return new NpgsqlConnection(_connectionString);
		}
	}
}