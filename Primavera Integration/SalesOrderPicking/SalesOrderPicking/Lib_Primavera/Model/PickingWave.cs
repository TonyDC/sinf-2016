using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class PickingWave {

        private string id;
        private int funcionarioID;
        private Dictionary<string, List<PickingLine>> linhas;

        public string ID { get { return this.id; } }
        public int FuncionarioID { get { return this.funcionarioID; } }
        public Dictionary<string, List<PickingLine>> Linhas { get { return this.linhas; } }

        public PickingWave(string id, int funcionarioID, Dictionary<string, List<PickingLine>> linhas) {
            this.id = id;
            this.funcionarioID = funcionarioID;
            this.linhas = linhas;
        }
        
    }
}