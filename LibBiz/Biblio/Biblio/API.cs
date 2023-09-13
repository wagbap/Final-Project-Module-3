using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblio
{
    public class API
    {

        public class Result
        {
            public int retCode { get; set; }
            public string retError { get; set; }
            public string retActionDateTime { get; set; }
        }
        public class Result_Ext : Result
        {
            public DataSet ds { get; set; }
        }
        public class Prm_Login
        {
            public string User { get; set; }
            public string Password { get; set; }
        }
        public class Prm_SQL
        {
            public string[] SQL { get; set; }
        }

        public static void Login(Prm_Login prmIn, out Result_Ext prmOut, SqlConnection cn = null, string cn_string = null)
        {
            prmOut = new Result_Ext();
            prmOut.ds = new DataSet("Tables");

            SqlConnection connection = null;
            bool bCloseConnection = false;

            try
            {
                connection = cn;
                if (connection == null)
                {
                    connection = new SqlConnection(cn_string);
                    bCloseConnection = true;
                    connection.Open();
                }

                using (SqlCommand cmd = new SqlCommand("SistemaLogin", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@User", prmIn.User);
                    cmd.Parameters.AddWithValue("@Password", prmIn.Password);
                    using (DataSet ds = new DataSet())
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(prmOut.ds);

                            if (prmOut.ds.Tables[0].Rows.Count != 1)
                            {
                                throw new Exception("Username/Password not valid.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (bCloseConnection && connection != null) { connection.Close(); }
            }
        }

        public static void Execute_SQL(Prm_SQL prmIn, out Result_Ext prmOut, SqlConnection cn = null, string cn_string = null)
        {
            prmOut = new Result_Ext();
            prmOut.ds = new DataSet();

            SqlConnection connection = null;
            bool bCloseConnection = false;

            try
            {
                connection = cn;
                if (connection == null)
                {
                    connection = new SqlConnection(cn_string);
                    bCloseConnection = true;
                    connection.Open();
                }

                foreach (string sql in prmIn.SQL)
                {
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            using (var da = new SqlDataAdapter(cmd)) { da.Fill(dt); prmOut.ds.Tables.Add(dt); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (bCloseConnection && connection != null) { connection.Close(); }
            }
        }
    }
}
