using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SalesOrderPicking.Lib_Primavera;

namespace SalesOrderPicking.Auth {

    public class Auth {
        
        private const string FUNCIONARIO_STR = "Funcionario";
        private const string GERENTE_STR = "Gerente";
        private const int SALT_ROUNDS = 12;

        public static bool LoginWorker(string username, string password) {

            Pair<string, string> credentials = RetrieveCredentials(username, FUNCIONARIO_STR);
            if (credentials == null)
                return false;
            return BCrypt.Net.BCrypt.Verify(password, credentials.Second);
        }

        public static bool LoginManager(string username, string password) {

            Pair<string, string> credentials = RetrieveCredentials(username, FUNCIONARIO_STR);
            if (credentials == null)
                return false;
            return BCrypt.Net.BCrypt.Verify(password, credentials.Second);
        }




        public static int RegisterWorker(string username, string password) {
            int userID = RegisterUser(username, password, FUNCIONARIO_STR);
            password = null;
            return userID;
        }

        public static int RegisterManager(string username, string password) {
            int userID = RegisterUser(username, password, GERENTE_STR);
            password = null;
            return userID;
        }




        private static int RegisterUser(string username, string password, string type) {

            if ((String.Compare(type, FUNCIONARIO_STR) == 0 && String.Compare(type, GERENTE_STR) == 0) || (String.Compare(type, FUNCIONARIO_STR) != 0 && String.Compare(type, GERENTE_STR) != 0))
                throw new InvalidOperationException("Bad user type");

            // Hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(SALT_ROUNDS));
            password = null;

            // Check if the username already exists
            if (RetrieveCredentials(username, type) != null)
                throw new InvalidOperationException("The username already exists");

            List<Dictionary<string, object>> insertedUser = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "INSERT INTO Utilizador(username, pass) OUTPUT INSERTED.id VALUES('@0@', '@1@')",
                username, hashedPassword);

            hashedPassword = null;
            int userID = Convert.ToInt32(insertedUser.ElementAt(0)["id"]);

            if (String.Compare(type, FUNCIONARIO_STR) == 0)
                DBQuery.performQuery(PriEngine.PickingDBConnString,
                    "INSERT INTO Funcionario VALUES(@0@)",
                    userID.ToString());
            else
                DBQuery.performQuery(PriEngine.PickingDBConnString,
                    "INSERT INTO Gerente VALUES(@0@)",
                    userID.ToString());

            return userID;
        }



        private static Pair<string, string> RetrieveCredentials(string username, string type) {

            if ((String.Compare(type, FUNCIONARIO_STR) == 0 && String.Compare(type, GERENTE_STR) == 0) || (String.Compare(type, FUNCIONARIO_STR) != 0 && String.Compare(type, GERENTE_STR) != 0))
                throw new InvalidOperationException("Bad user type");

            List<Dictionary<string, object>> rows = DBQuery.performQuery(PriEngine.PickingDBConnString,
                "SELECT Utilizador.username, Utilizador.pass FROM " + type + " INNER JOIN Utilizador ON (" + type + ".id = Utilizador.id) WHERE Utilizador.username = '@0@'",
                username);

            if (rows.Count < 1)
                return null;

            Dictionary<string, object> row = rows.ElementAt(0);
            if (!row.ContainsKey("username") || !row.ContainsKey("pass"))
                return null;

            return new Pair<string, string> { First = rows.ElementAt(0)["username"] as string, Second = rows.ElementAt(0)["pass"] as string };
        }
         
    }

}