﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;

namespace SalesOrderPicking.Controllers {

    public class UtilController : ApiController {
        /*
        // GET: api/Util
        [Route("api/teste/series")]
        public IHttpActionResult Get() {

            List<string> result = null;
            try {
                result = PriIntegration.GetSeries();
            } catch (Exception) {
                return InternalServerError();
            }


            if (result == null)
                return NotFound();

            else
                return Ok(result);
        }

        [Route("api/teste/stock/{armazem}")]
        public IHttpActionResult GetStockActual(string armazem) {

            Dictionary<string, int> result = null;
            try {
                result = PriIntegration.GetStockActual(armazem);
            } catch (Exception e) {
                return InternalServerError(e);
            }


            if (result == null)
                return NotFound();

            else
                return Ok(result);
        }

        [Route("api/teste/picking")]
        public IHttpActionResult PostPicking(Dictionary<string, object> p) {

            bool result = false;
            try {
                result = PriIntegration.GerarPickingOrders(p["filial"].ToString(), p["serie"].ToString(), ((Newtonsoft.Json.Linq.JArray)p["encomendas"]).ToObject<List<uint>>());
            } catch (Exception e) {
                return InternalServerError(e);
            }


            if (!result)
                return NotFound();

            else
                return Ok();
        }

        [Route("api/teste/pick")]
        public IHttpActionResult PostNewPick(Dictionary<string, object> p) {

            Wave<PickingLine> result = null;
            try {
                result = PriIntegration.GetProximaPickingWave(Convert.ToInt32(p["funcionario"]));
            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (result != null)
                foreach (var item in result.Linhas) {
                    System.Diagnostics.Debug.WriteLine(item.Key + " " + item.Value);
                }

            if (result == null)
                return NotFound();

            else
                return Ok(result);
        }

        [Route("api/teste/end_pick")]
        public IHttpActionResult PostFinishPick(Dictionary<string, object> p) {

            List<Pair<string, int>> r = new List<Pair<string, int>>();

            foreach (var item in ((Newtonsoft.Json.Linq.JArray)p["linhas"]).ToObject<List<Dictionary<string, object>>>()) {
                r.Add(new Pair<string, int> { First = item["id"] as string, Second = Convert.ToInt32(item["quantidade"]) });
                //System.Diagnostics.Debug.WriteLine(item["id"] + " " + item["quantidade"]);   
            }

            bool result = false;
            try {
                result = PriIntegration.TerminarPickingOrder(
                    Convert.ToInt32(p["funcionario"]),
                    p["pickingWave"] as string,
                    r,
                    p["serie"] as string);
            } catch (Exception e) {
                return InternalServerError(e);
            }


            if (!result)
                return NotFound();

            else
                return Ok();
        }


        [Route("api/teste/replenishment")]
        public IHttpActionResult PostNewReplenishment(Dictionary<string, object> p) {

            Wave<ReplenishmentLine> result = null;
            try {
                result = PriIntegration.GetProximaReplenishmentOrder(Convert.ToInt32(p["funcionario"]));
            } catch (Exception e) {
                return InternalServerError(e);
            }

            if (result != null)
                foreach (var item in result.Linhas) {
                    System.Diagnostics.Debug.WriteLine(item.Key + " " + item.Value);
                }

            if (result == null)
                return NotFound();

            else
                return Ok(result);
        }

        [Route("api/teste/end_replenishment")]
        public IHttpActionResult PostFinishReplenishment(Dictionary<string, object> p) {

            List<Pair<string, int>> r = new List<Pair<string, int>>();

            foreach (var item in ((Newtonsoft.Json.Linq.JArray)p["linhas"]).ToObject<List<Dictionary<string, object>>>()) {
                r.Add(new Pair<string, int> { First = item["id"] as string, Second = Convert.ToInt32(item["quantidade"]) });
                //System.Diagnostics.Debug.WriteLine(item["id"] + " " + item["quantidade"]);   
            }

            bool result = false;
            try {
                result = PriIntegration.terminarReplenishmentOrder(
                    Convert.ToInt32(p["funcionario"]),
                    p["replenishmentWave"] as string,
                    r,
                    p["serie"] as string);
            } catch (Exception e) {
                return InternalServerError(e);
            }


            if (!result)
                return NotFound();

            else
                return Ok();
        }

        /*
        [Route("api/teste/satisfeita")]
        [HttpPost]
        public IHttpActionResult PostTest(Dictionary<string, object> dic) {

            System.Diagnostics.Debug.WriteLine(dic["nDoc"].GetType());
            int doc = Convert.ToInt32(dic["nDoc"]);
            double q = Convert.ToDouble(dic["quantSatisfeita"]);

            PriIntegration.testFunction(dic["filial"] as string, dic["serie"] as string, Convert.ToUInt32(doc), dic["artigo"] as string, q);

            return Ok();
        }
        */
        

    }
}