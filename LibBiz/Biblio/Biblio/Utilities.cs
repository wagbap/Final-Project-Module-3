using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Biblio.API;

namespace Biblio
{
    public class Utilities
    {
        #region Core procedures
        public static string ActionJsonResponse(int errCode, string errMsg, Result responseActionPrms = null)
        {
            object objRet = null;

            if (responseActionPrms == null)
            {
                var stuff = new Result();
                stuff.retCode = errCode;
                stuff.retError = errMsg;
                stuff.retActionDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                objRet = stuff;
            }
            else
            {
                responseActionPrms.retCode = errCode;
                responseActionPrms.retError = errMsg;
                responseActionPrms.retActionDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                objRet = responseActionPrms;
            }

            return JsonConvert.SerializeObject(objRet);
        }
        #endregion
    }
}
