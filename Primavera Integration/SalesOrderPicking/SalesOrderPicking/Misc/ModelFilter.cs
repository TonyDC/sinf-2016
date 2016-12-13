﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;

namespace SalesOrderPicking.Misc {

    public class ModelFilter : ActionFilterAttribute {

        public override void OnActionExecuting(HttpActionContext actionContext) {
            var modelState = actionContext.ModelState;

            if (!modelState.IsValid)
                actionContext.Response = actionContext.Request.
                    CreateErrorResponse(HttpStatusCode.BadRequest, modelState);
        }
    }
}