using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model.Requests {

    public class LinhaWave {

        [JsonProperty("id", Required = Required.Always)]
        public string First { get; set; }
        [JsonProperty("quantidade", Required = Required.Always)]
        public int Second { get; set; }

    }

    public class WaveCompletion {

        [JsonProperty("funcionario", Required = Required.Always)]
        public int IDFuncionario { get; set; }
        [JsonProperty("wave", Required = Required.Always)]
        public string WaveID { get; set; }
        [JsonProperty("linhas", Required = Required.Always)]
        public List<LinhaWave> Lines { get; set; }

    }
}