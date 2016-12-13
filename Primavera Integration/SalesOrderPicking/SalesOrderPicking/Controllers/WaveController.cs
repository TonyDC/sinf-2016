using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SalesOrderPicking.Lib_Primavera;
using SalesOrderPicking.Lib_Primavera.Model;
using SalesOrderPicking.Lib_Primavera.Model.Requests;
using System.Web.Http.Results;

namespace SalesOrderPicking.Controllers {

    public class WaveController : ApiController {

        // POST: api/wave/generate
        /*
         * {
         *      "filial": string,
         *      "serie": string,
         *      "encomendas": [
         *         uint, uint, ...
         *      ]
         * }
         */
        [Route("api/wave/generate")]
        public IHttpActionResult PostNewWaves(GenerateWavesRequest request) {

            bool result = false;
            try {
                result = PriIntegration.GerarPickingOrders(request.Filial, request.Serie, request.Encomendas);

            } catch (InvalidOperationException invalidOperation) {
                return BadRequest(invalidOperation.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }


            if (!result)
                return NotFound();
            else
                return Ok();
        }

        [Route("api/wave/assign")]
        public IHttpActionResult GetAssign(int funcionario) {
            
            try {
                // Verificar se este funcionario ainda tem waves pendentes
                // Se tem, retornar essa wave
                dynamic pendingWave = PriIntegration.GetWaveActual(funcionario);
                if (pendingWave != null)
                    return Ok(pendingWave);

                // Verificar se existem replenishment lines pendentes
                // Se sim, retorna uma nova replenishment wave
                Wave<ReplenishmentLine> pendingReplenishmentWave = PriIntegration.GetProximaReplenishmentOrder(funcionario);
                if (pendingReplenishmentWave != null)
                    return Ok(pendingReplenishmentWave);

                // Por fim, verificar se existem picking waves pendentes
                // Se sim, retorna uma nova picking wave
                Wave<PickingLine> pendingPickingWave = PriIntegration.GetProximaPickingWave(funcionario);
                if (pendingPickingWave != null)
                    return Ok(pendingPickingWave);

            } catch (InvalidOperationException invalidOperation) {
                return BadRequest(invalidOperation.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/wave/terminate/picking
        /*
         * {
         *      "funcionario": uint,
         *      "wave": string,             // Wave ID
         *      "linhas": [
         *          {   
         *              "id": string,       // Wave line ID
         *              "quantidade": uint    // Quantidade satisfeita
         *          }
         *      ]
         * }
         */
        [Route("api/wave/terminate/picking")]
        public IHttpActionResult PostTerminatePicking(WaveCompletion wave) {

            try {
                PriIntegration.TerminarPickingOrder(wave.IDFuncionario, wave.WaveID, wave.Lines);
            
            } catch (InvalidOperationException invalidOperation) {
                return BadRequest(invalidOperation.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            return Ok();
        }

        // POST: api/wave/terminate/replenishment
        /*
         * {
         *      "funcionario": uint,
         *      "wave": string,               // Wave ID
         *      "linhas": [
         *          {   
         *              "id": string,         // Wave line ID
         *              "quantidade": uint    // Quantidade satisfeita
         *          }
         *      ]
         * }
         */
        [Route("api/wave/terminate/replenishment")]
        public IHttpActionResult PostTerminateReplenishment(WaveCompletion wave) {

            try {
                PriIntegration.TerminarReplenishmentOrder(wave.IDFuncionario, wave.WaveID, wave.Lines);

            } catch (InvalidOperationException invalidOperation) {
                return BadRequest(invalidOperation.Message);

            } catch (Exception e) {
                return InternalServerError(e);
            }

            return Ok();
        }

    }
}
