using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model.Requests {
    
    public class GenerateWavesRequest {

        [Required]
        [JsonProperty("filial", Required = Required.Always)]
        public string Filial { get; set; }

        [Required]
        [JsonProperty("serie", Required = Required.Always)]
        public string Serie { get; set; }

        [Required]
        [JsonProperty("encomendas", Required = Required.Always)]
        public List<uint> Encomendas { get; set; }

    }

}