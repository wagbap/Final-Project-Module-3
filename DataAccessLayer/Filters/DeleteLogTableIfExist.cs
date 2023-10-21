using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Filters
{
    public class DeleteLogTableIfExist
    {

        public static void CheckAndDeleteLogTableIfExist(string connectionString)
        {
            const string tableName = "Logs";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Verificar se a tabela existe
                var tableExistsCommand = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName", connection);
                tableExistsCommand.Parameters.AddWithValue("@TableName", tableName);
                var reader = tableExistsCommand.ExecuteReader();

                // Se a tabela existir, apagá-la
                if (reader.HasRows)
                {
                    reader.Close();
                    var dropTableCommand = new SqlCommand($"DROP TABLE {tableName}", connection);
                    dropTableCommand.ExecuteNonQuery();
                }
            }
        }

    }
}
