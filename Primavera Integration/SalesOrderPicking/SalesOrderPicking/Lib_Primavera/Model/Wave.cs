using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class Wave<T> {

        private string id;
        private int funcionarioID;
        private Dictionary<string, List<T>> linhas;

        public string ID { get { return this.id; } }
        public int FuncionarioID { get { return this.funcionarioID; } }
        public Dictionary<string, List<T>> Linhas { get { return this.linhas; } }

        public Wave(string id, int funcionarioID, Dictionary<string, List<T>> linhas) {
            this.id = id;
            this.funcionarioID = funcionarioID;
            this.linhas = linhas;
        }
        
    }
}