using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera.Model {

    public class UserLine {

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public byte Type { get; set; }          // 0 -> User; 1 -> Worker; 2 -> Manager
    }
}