using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class Wave<T> {

        private byte waveType;
        private string id;
        private int funcionarioID;
        private Dictionary<string, List<T>> linhas;

        [JsonProperty("tipo")]
        public byte WaveType { get { return this.waveType; } }
        [JsonProperty("id")]
        public string ID { get { return this.id; } }
        [JsonProperty("funcionario")]
        public int FuncionarioID { get { return this.funcionarioID; } }
        [JsonProperty("linhas")]
        public Dictionary<string, List<T>> Linhas { get { return this.linhas; } }

        public Wave(string id, int funcionarioID, Dictionary<string, List<T>> linhas) {
            if (typeof(T) == typeof(ReplenishmentLine))
                waveType = 0;
            else
                waveType = 1;

            this.id = id;
            this.funcionarioID = funcionarioID;
            this.linhas = linhas;
        }
        
    }
}