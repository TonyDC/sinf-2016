﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text;

namespace SalesOrderPicking.Lib_Primavera {

    public class DBQuery {

        public SqlTransaction Transaction { get; set; }
        private SqlConnection Connection { get; set; }

        public void Commit() {
            if (Transaction == null)
                return;

            Transaction.Commit();
            Connection.Close();
        }

        public void Rollback() {
            if (Transaction == null)
                return;

            Transaction.Rollback();
            Connection.Close();
        }


        public static List<Dictionary<string, object>> performQuery(string connectionString, string queryString, params object[] elements) {

            System.Diagnostics.Debug.WriteLine(queryString);
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

            using (var connection = new SqlConnection(connectionString)) {

                SqlCommand command = new SqlCommand(queryString, connection);
                for (int i = 0; i < elements.Length; i++)
                    command.Parameters.AddWithValue("@" + i + "@", elements.ElementAt(i));

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader()) {

                    // Call Read before accessing data.
                    while (reader.Read()) {

                        Dictionary<string, object> row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                            row.Add(reader.GetName(i), reader.GetValue(i));

                        rows.Add(row);
                    }

                }
                // Call Close when done reading (by using 'using', the connection is automatically closed)
                // reader.Close();
            }

            return rows;
        }

        public DBQuery(string connectionString) {
            Connection = new SqlConnection(connectionString);
            Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        public List<Dictionary<string, object>> performQueryWithTransaction(string queryString, params object[] elements) {

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

            SqlCommand command = new SqlCommand(queryString, Connection);
            for (int i = 0; i < elements.Length; i++)
                command.Parameters.AddWithValue("@" + i + "@", elements.ElementAt(i));

            command.Transaction = Transaction;

            using (SqlDataReader reader = command.ExecuteReader()) {

                // Call Read before accessing data.
                while (reader.Read()) {

                    Dictionary<string, object> row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                        row.Add(reader.GetName(i), reader.GetValue(i));

                    rows.Add(row);
                }

            }

            // Call Close when done reading (by using 'using', the connection is automatically closed)
            // reader.Close();


            return rows;
        }
    }
}