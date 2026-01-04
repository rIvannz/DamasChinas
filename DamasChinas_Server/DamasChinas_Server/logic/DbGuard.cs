using DamasChinas_Server.Common;
using System;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace DamasChinas_Server.Logic
{
    public static class DbGuard
    {
        private const string EfConnectionName = "damas_chinasEntities";

        public static void EnsureDatabaseAvailable()
        {
            string sqlCs = GetSqlConnectionStringFromEf(EfConnectionName);
            if (string.IsNullOrWhiteSpace(sqlCs))
            {
                throw new RepositoryValidationException(MessageCode.DatabaseUnavailable);
            }

            var csb = new SqlConnectionStringBuilder(sqlCs)
            {
                ConnectTimeout = 2
            };

            using (var conn = new SqlConnection(csb.ToString()))
            using (var cmd = new SqlCommand("SELECT 1", conn))
            {
                cmd.CommandTimeout = 2;
                conn.Open();
                cmd.ExecuteScalar();
            }
        }



        private static string GetSqlConnectionStringFromEf(string efName)
        {
            string ef = ConfigurationManager.ConnectionStrings[efName]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(ef))
            {
                return null;
            }

            var builder = new EntityConnectionStringBuilder(ef);
            return builder.ProviderConnectionString;
        }
    }
}
