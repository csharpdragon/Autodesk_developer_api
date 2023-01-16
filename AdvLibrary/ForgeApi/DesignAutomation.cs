using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AdvLibrary.ForgeApi.Model;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Autodesk.Forge.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using System.Net;
namespace AdvLibrary.ForgeApi
{
    public class DesignAutomation
    {
        private ExceptionFactory _exceptionFactory = (string name, RestResponse response) => null;

        public ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }

                return _exceptionFactory;
            }
            set
            {
                _exceptionFactory = value;
            }
        }


        #region Private Members
        private string token;
        private DateTime lastTokenTime;
        private int expiresIn;

        private string nickName;
        #endregion

        #region Public Properties
        public string Token
        {
            get { return token; }
            set { token = value; }
        }

        public int ExpiresIn
        {
            get { return expiresIn; }
            set { expiresIn = value; }
        }

        public DateTime LastTokenTime
        {
            get { return lastTokenTime; }
            set { lastTokenTime = value; }
        }
        #endregion

        #region Constructor
        public DesignAutomation()
        {

        }

        public DesignAutomation(string token)
        {
            this.token = token;
        }

        public DesignAutomation(string token, int expiresIn, DateTime lasttime)
        {
            this.token = token;
            this.expiresIn = expiresIn;
            this.lastTokenTime = lasttime;
        }
        #endregion

        #region Public functions

        public bool IsTokenExpired()
        {
            var passedSeconds = (DateTime.Now - this.lastTokenTime).TotalSeconds;
            if (passedSeconds >= this.ExpiresIn)
            {
                return true;
            }

            return false;
        }

        ///create Nickname for app
        public bool CreateNickName(string nickname)
        {
            var url = "https://developer.api.autodesk.com/da/us-east/v3/forgeapps/me";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "PATCH";

            httpRequest.Headers["Authorization"] = "Bearer "+token;
            httpRequest.ContentType = "application/json";

            var data = new { nickname = nickname };

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data).ToString());
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
            if(httpResponse.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            if(httpResponse.StatusCode == HttpStatusCode.Conflict)
            {
                /// in case of conflict, so change the name
                return false;
            }

            return false;
        }
        #endregion
    }
}