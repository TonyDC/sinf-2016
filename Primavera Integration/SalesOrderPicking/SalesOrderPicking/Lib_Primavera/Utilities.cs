using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text;

namespace SalesOrderPicking.Lib_Primavera {

    public class Utilities {

        public SqlTransaction Transaction { get; set; }
        private SqlConnection Connection { get; set; }

        public void Commit() {
            Transaction.Commit();
            Connection.Close();
        }

        public void Rollback() {
            Transaction.Rollback();
            Connection.Close();
        }

        /*
        private static bool useTransaction = false;
        private static SqlTransaction transaction = null;

        public static bool useTransactionMode() {
            if (useTransaction)
                return false;

            useTransaction = true;
            return true;
        }

        public static bool commitTransaction() {
            if (!useTransaction || transaction == null)
                return false;

            transaction.Commit();
            useTransaction = false;
            transaction = null;
            return true;
        }

        public static bool rollbackTransaction() {
            if (!useTransaction || transaction == null)
                return false;

            transaction.Rollback();
            useTransaction = false;
            transaction = null;
            return true;
        }*/
        // TODO: passar a ter uma parametrizacao
        public static List<Dictionary<string, object>> performQuery(string connectionString, string queryString, params object[] elements) {

            //for (int i = 0; i < elements.Length; i++)
            //    queryString = queryString.Replace("@" + i + "@", elements.ElementAt(i));
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

        public List<Dictionary<string, object>> performQueryWithTransaction(string connectionString, string queryString, params object[] elements) {

            //for (int i = 0; i < elements.Length; i++)
            //    queryString = queryString.Replace("@" + i + "@", elements.ElementAt(i));
            System.Diagnostics.Debug.WriteLine(queryString);
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

            if(Connection == null) {
                Connection = new SqlConnection(connectionString);
                Connection.Open();
                Transaction = Connection.BeginTransaction();
            }

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