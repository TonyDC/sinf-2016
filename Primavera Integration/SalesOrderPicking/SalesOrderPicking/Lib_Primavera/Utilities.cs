using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text;

namespace SalesOrderPicking.Lib_Primavera {

    public class Utilities {

        public static List<Dictionary<string, object>> performQuery(string connectionString, string queryString, params string[] elements) {

            for (int i = 0; i < elements.Length; i++)
                queryString = queryString.Replace("@" + i + "@", elements.ElementAt(i));

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

            using (var connection = new SqlConnection(connectionString)) {

                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                // Call Read before accessing data.
                while (reader.Read()) {

                    Dictionary<string, object> row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                        row.Add(reader.GetName(i), reader.GetValue(i));

                    rows.Add(row);
                }

                // Call Close when done reading (by using 'using', the connection is automatically closed)
                // reader.Close();
            }

            return rows;
        }

    }
}