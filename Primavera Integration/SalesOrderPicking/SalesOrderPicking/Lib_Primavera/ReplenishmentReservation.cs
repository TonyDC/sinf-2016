using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera {

    public class ReplenishmentReservation {

        // artigo, localizacao, quantidade (int), unidade, serie
        public string Artigo { get; set; }
        public string Localizacao { get; set; }
        public int Quantidade { get; set; }
        public string Unidade { get; set; }
        public string Serie { get; set; }
    }

}