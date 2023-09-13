using Cursos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilis;
using static Biblio.API;

namespace Biblio
{
    public class Gateway
    {

        SqlConnection cn = null;
        DataSet ds = null;
        public static string ExecuteLogin(Prm_Login prmIn)
        {

            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(currentDirectory, "ConsoleADONET1.ini");
            Utils.INIFile ini = new Utils.INIFile(filePath);


            string cnString = ini.ReadKey("Connection_Biblio");

            int iRC = 0;
            string sMsgErr = "";

            Result_Ext prmOutExt = null;
            Result prmOut = null;

            try
            {
                Login(prmIn, out prmOutExt, null, cnString);
                prmOut = prmOutExt;

                iRC = 1;
            }
            catch (Exception ex)
            {
                iRC = 0;
                sMsgErr = ex.Message;
            }
            finally
            {
            }

            return Utilities.ActionJsonResponse(iRC, sMsgErr, prmOut);
        }

        public static string ExecuteSQL(Prm_SQL prmIn, string cn_string = null)
        {
            int iRC = 0;
            string sMsgErr = "";

            Result_Ext prmOutExt = null;
            Result prmOut = null;

            try
            {
                Execute_SQL(prmIn, out prmOutExt, null, cn_string);
                prmOut = prmOutExt;

                iRC = 1;
            }
            catch (Exception ex)
            {
                iRC = 0;
                sMsgErr = ex.Message;
            }
            finally
            {
            }

            return Utilities.ActionJsonResponse(iRC, sMsgErr, prmOut);
        }
    }
}
