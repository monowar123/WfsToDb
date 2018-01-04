using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WfsToDb
{
    public class DbHandler
    {
        string connectionString = string.Empty;
        public DbHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int ExecuteNonQueryWithParameter(string sqlString, List<NpgsqlParameter> parameter)
        {
            int result = 0;
            try
            {
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString))
                {
                    if (dbConnection.State == ConnectionState.Closed)
                        dbConnection.Open();

                    NpgsqlCommand command = new NpgsqlCommand(sqlString, dbConnection);                 

                    foreach (NpgsqlParameter pr in parameter)
                    {
                        command.Parameters.Add(pr);
                    }

                    result = command.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public int ExecuteNonQueryWithParameter(string sqlString, List<List<NpgsqlParameter>> parameterDictionary)
        {
            int result = 0;
            try
            {
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString))
                {
                    if (dbConnection.State == ConnectionState.Closed)
                        dbConnection.Open();

                    foreach (var parameter in parameterDictionary)
                    {
                        NpgsqlCommand command = new NpgsqlCommand(sqlString, dbConnection);

                        foreach (NpgsqlParameter pr in parameter)
                        {
                            command.Parameters.Add(pr);
                        }

                        result = command.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }
    }
}
