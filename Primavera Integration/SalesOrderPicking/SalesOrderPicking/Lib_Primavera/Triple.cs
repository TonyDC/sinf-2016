﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesOrderPicking.Lib_Primavera {

    public class Triple<T, S, U> {

        public T First { get; set; }
        public S Second { get; set; }
        public U Third { get; set; }

    }
}