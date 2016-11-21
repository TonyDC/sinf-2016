using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interop.ErpBS900;         // Use Primavera interop's [Path em C:\Program Files\Common Files\PRIMAVERA\SG800]
using Interop.StdPlatBS900;
using Interop.StdBE900;
using ADODB;

namespace SalesOrderPicking.Lib_Primavera {
    
    public class PriEngine {

        private static StdPlatBS platform = null;
        private static ErpBS engine = null;
        private static string databaseConnectionString = null;

        // Gere a parte da autenticação da plataforma
        public static StdPlatBS Platform { get { return PriEngine.platform; } }

        // Gere a parte de acesso à plataforma, e garante o cumprimento das regras de negócio
        public static ErpBS Engine { get { return PriEngine.engine; } }

        public static string DBConnString { get { return PriEngine.databaseConnectionString; } }

        private PriEngine() {
        }

        public static bool IniciaTransaccao() {
            if (Engine == null)
                return false;
            
            Engine.IniciaTransaccao();
            return true;
        }

        public static bool TerminaTransaccao() {
            if (Engine == null)
                return false;

            Engine.TerminaTransaccao();
            return true;
        }

        public static bool InitializeCompany(string Company, string User, string Password) {

            StdBSConfApl objAplConf = new StdBSConfApl();
            StdPlatBS Plataforma = new StdPlatBS();
            ErpBS MotorLE = new ErpBS();

            EnumTipoPlataforma objTipoPlataforma = new EnumTipoPlataforma();
            objTipoPlataforma = EnumTipoPlataforma.tpProfissional;

            objAplConf.Instancia = "Default";
            objAplConf.AbvtApl = "GCP";
            objAplConf.PwdUtilizador = Password;
            objAplConf.Utilizador = User;
            objAplConf.LicVersaoMinima = "9.00";

            StdBETransaccao objStdTransac = new StdBETransaccao();

            // Opem platform (verifica se o utilizador tem permissões de acesso)
            try {
                Plataforma.AbrePlataformaEmpresa(ref Company, ref objStdTransac, ref objAplConf, ref objTipoPlataforma, "");
            
            } catch (Exception ex) {
                throw new Exception("Error on open Primavera Platform: " + ex.Message);
            }

            // Is plt initialized?
            if (Plataforma.Inicializada) {

                // Retuns the ptl.
                platform = Plataforma;

                bool blnModoPrimario = true;

                // Open Engine
                MotorLE.AbreEmpresaTrabalho(EnumTipoPlataforma.tpProfissional, ref Company, ref User, ref Password, ref objStdTransac, "Default", ref blnModoPrimario);
                MotorLE.set_CacheActiva(false);

                // Returns the engine.
                engine = MotorLE;

                string databaseInstance = "Default";
                string dbNomeEmpresa = platform.BaseDados.DaNomeBDdaEmpresa(engine.Contexto.CodEmp);
                databaseConnectionString = platform.BaseDados.DaConnectionStringNET(dbNomeEmpresa, databaseInstance);

                return true;

            } else {
                return false;
            }
        }
    }
}
