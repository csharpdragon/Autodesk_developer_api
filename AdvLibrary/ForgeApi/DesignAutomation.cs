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
        #region ForTask3
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
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    this.nickName = nickname;
                    return true;
                }
                if (httpResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    /// in case of conflict, so change the name
                    return false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("You have data for your nickname. Cann't patch. You have to delete first");
                return false;
            }
            return false;
        }
        #endregion

        #region ForTask4

        private string Policy;
        private string x_amz_signature;
        private string x_amz_credential;
        private string x_amz_algorithm;
        private string x_amz_date;
        private string x_amz_security_token;
        private string AppId;
        private string AppEngine;
        private string EndPointUrl;
        private string version;
        private string AppKey;
        public bool RegisterAppBundle(string appId, string appEngine,string appDescription)
        {

            var url = "https://developer.api.autodesk.com/da/us-east/v3/appbundles";
            string jsonResponse = string.Empty;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.Headers["Authorization"] = "Bearer " + token;
            httpRequest.ContentType = "application/json";

            var data = new { 
                id = appId,
                engine = appEngine,
                description = appDescription
            };

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data).ToString());
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    jsonResponse = streamReader.ReadToEnd();
                }

                JObject json = JObject.Parse(jsonResponse);
                if (!string.IsNullOrEmpty(json["id"]?.ToString()))
                {
                    Policy = json["uploadParameters"]["formData"]["policy"].ToString();
                    x_amz_signature = json["uploadParameters"]["formData"]["x-amz-signature"].ToString();
                    x_amz_credential = json["uploadParameters"]["formData"]["x-amz-credential"].ToString();
                    x_amz_algorithm = json["uploadParameters"]["formData"]["x-amz-algorithm"].ToString();
                    x_amz_date = json["uploadParameters"]["formData"]["x-amz-date"].ToString();
                    x_amz_security_token = json["uploadParameters"]["formData"]["x-amz-security-token"].ToString();
                    AppKey = json["uploadParameters"]["formData"]["key"].ToString();
                    AppId = json["id"].ToString();
                    AppEngine = json["engine"].ToString();
                    EndPointUrl = json["uploadParameters"]["endpointURL"].ToString();
                    version = json["version"].ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("You have that name app. Can't create again");
            }
            return false;

        }
        
        public bool  UploadAppBundle(string filepath)
        {
            var content =new MultipartFormDataContent();
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            
            postData.Add(new KeyValuePair<string, string>("key", AppKey));
            postData.Add(new KeyValuePair<string, string>("content-type", "application/octet-stream"));
            postData.Add(new KeyValuePair<string, string>("policy", Policy));
            postData.Add(new KeyValuePair<string, string>("success_action_status", "200"));
            postData.Add(new KeyValuePair<string, string>("success_action_redirect", ""));
            postData.Add(new KeyValuePair<string, string>("x-amz-signature", x_amz_signature));
            postData.Add(new KeyValuePair<string, string>("x-amz-credential", x_amz_credential));
            postData.Add(new KeyValuePair<string, string>("x-amz-algorithm", x_amz_algorithm));
            postData.Add(new KeyValuePair<string, string>("x-amz-date", x_amz_date));
            postData.Add(new KeyValuePair<string, string>("x-amz-server-side-encryption", "AES256"));
            postData.Add(new KeyValuePair<string, string>("x-amz-security-token", x_amz_security_token));

            for (var i=0;i<postData.Count;i++)
            {
                string inputName = postData[i].Key;
                string value = postData[i].Value;

                content.Add(new StringContent(value), inputName);
            }
            
            FileStream fileStream = File.OpenRead(filepath);
            var streamContent = new StreamContent(fileStream);
            var fileContent = new ByteArrayContent(streamContent.ReadAsByteArrayAsync().Result);
            content.Add(fileContent, "file", Path.GetFileName(filepath));
            
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            try
            {
                HttpResponseMessage result1 = client.PostAsync("https://dasprod-store.s3.amazonaws.com", content).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if(result1.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("You have that name app. Can't create again");
            }
            return false;
        }
        #endregion
        #endregion
    }
}