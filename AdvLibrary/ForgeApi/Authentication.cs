using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace AdvLibrary.ForgeApi
{
    public class Authentication
    {
        #region Private Members
        private string client_id;
        private string client_secret;
        private string token;
        private string refresh_token = "";
        private string code;
        private string redirecturl = "http://localhost:8080/api/auth/callback";
        private static HttpListener listener;
        private static string url = "http://localhost:8080/";


        /// <summary>
        /// //for design automation
        /// </summary>
        private string designToken="";
        private int designexpiresIn = 0;
        private DateTime lastGotTokenTime;

        private static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
                "<h1>You authorized</h1>" +
            "  </body>" +
            "</html>";

        #endregion

        #region Public Properties

        public string DesignToken
        {
            get { return designToken; }
            set { designToken = value; }
        }

        public int DesignTokenExpiresIn
        {
            get { return designexpiresIn; }
            set { designexpiresIn = value; }
        }

        public string ClientId
        {
            get { return client_id; }
            set { client_id = value; }
        }
        public string ClientSecret
        {
            get { return client_secret; }
            set { client_secret = value; }
        }
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
        public string Refresh_token
        {
            get { return refresh_token; }
            set { refresh_token = value; }
        }
        #endregion

        #region Constructor
        public Authentication(string client_id, string client_secret)
        {
            this.client_id = client_id;
            this.client_secret = client_secret;
            this.code=GetCode();
        }

        public Authentication(string client_id, string client_secret, string rfresh_token)
        {
            this.client_id = client_id;
            this.client_secret = client_secret;
            if (string.IsNullOrEmpty(rfresh_token))
            {
                this.code = GetCode();
            }
            else
            {
                this.refresh_token = rfresh_token;
                GetTokenFromRefreshToken(this.refresh_token);
            }
        }
        #endregion

        #region public functions
        public async Task<String> HandleIncomingConnections()
        {
            bool runServer = true;
            String code = "";
            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if ((req.HttpMethod == "GET" && req.Url.AbsolutePath == "/api/auth/callback"))
                {
                    runServer = false;
                    code = req.QueryString["code"];
                }

                byte[] data = Encoding.UTF8.GetBytes(pageData);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
            return code;
        }

        public string GetCode()
        {

            var requesturl = $"https://developer.api.autodesk.com/authentication/v1/authorize?response_type=code&client_id={client_id}&redirect_uri={redirecturl}&scope=data:create%20data:read%20data:write";
            Process.Start(new ProcessStartInfo(requesturl) { UseShellExecute = true });
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task<string> listenTask = HandleIncomingConnections();

            this.code = listenTask.Result;

            GetToken();
            // Close the listener
            listener.Close();
            return code;
        }

        public void GetTokenFromRefreshToken(string rfresh_token)
        {
            string grantType = "refresh_token";

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

            postData.Add(new KeyValuePair<string, string>("client_id", client_id));
            postData.Add(new KeyValuePair<string, string>("client_secret", client_secret));
            postData.Add(new KeyValuePair<string, string>("grant_type", grantType));
            postData.Add(new KeyValuePair<string, string>("refresh_token", rfresh_token));
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            try
            {
                HttpResponseMessage result1 = client.PostAsync("https://developer.api.autodesk.com/authentication/v1/refreshtoken", new FormUrlEncodedContent(postData)).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            JObject json = JObject.Parse(jsonResponse);

            try
            {

                this.token = json["access_token"]?.ToString();
                this.refresh_token = json["refresh_token"]?.ToString();
                if(string.IsNullOrEmpty(this.token)) { 
                    this.code = GetCode(); 
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(json["developerMessage"].ToString())) {
                    this.code = GetCode();
                }
            }

        }

        public void GetToken()
        {
            string grantType = "authorization_code";

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

            postData.Add(new KeyValuePair<string, string>("client_id", client_id));
            postData.Add(new KeyValuePair<string, string>("client_secret", client_secret));
            postData.Add(new KeyValuePair<string, string>("grant_type", grantType));
            postData.Add(new KeyValuePair<string, string>("redirect_uri", redirecturl));
            postData.Add(new KeyValuePair<string, string>("code", code));
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            try
            {
                HttpResponseMessage result1 = client.PostAsync("https://developer.api.autodesk.com/authentication/v1/gettoken", new FormUrlEncodedContent(postData)).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;

                //                var responseValue = result1.Headers.GetValues("Content-Type").(i => i.Key == "X-BB-SESSION").Value.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            JObject json = JObject.Parse(jsonResponse);
            this.token = json["access_token"].ToString();
            this.refresh_token = json["refresh_token"].ToString();
        }


        public string GetDesignAutomationToken()
        {
            string grantType = "client_credentials";

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

            postData.Add(new KeyValuePair<string, string>("client_id", client_id));
            postData.Add(new KeyValuePair<string, string>("client_secret", client_secret));
            postData.Add(new KeyValuePair<string, string>("grant_type", grantType));
            postData.Add(new KeyValuePair<string, string>("scope", "code:all data:create data:write data:read bucket:create bucket:delete"));
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            try
            {
                HttpResponseMessage result1 = client.PostAsync("https://developer.api.autodesk.com/authentication/v1/authenticate", new FormUrlEncodedContent(postData)).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            JObject json = JObject.Parse(jsonResponse);

            try
            {
                this.designToken = json["access_token"]?.ToString();
                this.designexpiresIn = int.Parse(json["expires_in"]?.ToString());
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(json["developerMessage"].ToString()))
                {
                    throw new Exception(json["developerMessage"].ToString());
                }
                else { throw new Exception(e.ToString()); }
            }

            this.lastGotTokenTime = DateTime.Now;

            return this.designToken;
        }

        
        #endregion
    }
}
