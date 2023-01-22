using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
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
        public void DeleteNickName()
        {
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri($"https://developer.api.autodesk.com/");

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"da/us-east/v3/forgeapps/me");
            HttpResponseMessage result1 = null;
            try
            {
                result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if (result1.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Your nickname and previous data deleted successfully");
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public string GetNickName()
        {
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri($"https://developer.api.autodesk.com/");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"da/us-east/v3/forgeapps/me");
            HttpResponseMessage result1 = null;
            try
            {
                result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if (result1.StatusCode == HttpStatusCode.OK)
                {
                    return jsonResponse.Replace("\"", "");
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }
        #endregion

        #region ForTask4

        public string Policy;
        public string x_amz_signature;
        public string x_amz_credential;
        public string x_amz_algorithm;
        public string x_amz_date;
        public string x_amz_security_token;
        public string AppId;
        public string AppEngine;
        public string EndPointUrl;
        public string version;
        public string AppKey;
        public string AppaliasName;
        public List<string> GetAllBundles(string nickname)
        {
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri($"https://developer.api.autodesk.com/");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/da/us-east/v3/appbundles");
            HttpResponseMessage result1 = null;
            try
            {
                result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if (result1.StatusCode == HttpStatusCode.OK)
                {
                    List<string> resultArray = new List<string>();
                    JObject json = JObject.Parse(jsonResponse);
                    JArray dataArray = (JArray)json["data"];
                    JArray includedArray = (JArray)dataArray;
                    for(var i = 0; i < includedArray.Count; i++)
                    {
                        if (includedArray[i].ToString().Contains($"{nickname}.") && !includedArray[i].ToString().Contains("$LATEST"))
                            resultArray.Add(includedArray[i].ToString());
                    }
                    return resultArray;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }

        public bool RegisterAppBundle(string appId,string appDescription)
        {

            var url = "https://developer.api.autodesk.com/da/us-east/v3/appbundles";
            string jsonResponse = string.Empty;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.Headers["Authorization"] = "Bearer " + token;
            httpRequest.ContentType = "application/json";

            var data = new { 
                id = appId,
                engine = "Autodesk.Revit+2022",
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
                Console.WriteLine("You have that name app. Cann't create again");
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
        
        public bool CreateAliasForAppBundle(string appId,string aliasname)
        {
            object sedingData = new
            {
                version=1,
                id= aliasname
            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");            
  //          AppId = "DeleteWallsApp5";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"da/us-east/v3/appbundles/{appId}/aliases");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if (result1.StatusCode == HttpStatusCode.OK)
                {
                    AppaliasName = aliasname;
                    return true;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return true;
        }


        #region Task 4 Step 5 - Update an existing AppBundle

        public bool getParametersForUpdating(string appId, string engineName, string updatedescription)
        {
            object sedingData = new
            {
                engine = engineName,
                description = updatedescription
            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri($"https://developer.api.autodesk.com/");
            
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var tempId = appId;


            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"da/us-east/v3/appbundles/{tempId}/versions");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage result1=null;
            try
            {
                result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if (result1.StatusCode == HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(jsonResponse);
                    if (json["errors"] != null)
                    {
                        throw new Exception(json["errors"][0]["detail"].ToString());
                    }
                    appId= json["id"].ToString();
                    version = json["version"].ToString();
                    AppEngine = json["engine"].ToString();
                    EndPointUrl = json["uploadParameters"]["endpointURL"].ToString();
                    AppKey= json["uploadParameters"]["formData"]["key"].ToString();
                    Policy = json["uploadParameters"]["formData"]["policy"].ToString();
                    x_amz_signature = json["uploadParameters"]["formData"]["x-amz-signature"].ToString();
                    x_amz_credential = json["uploadParameters"]["formData"]["x-amz-credential"].ToString();
                    x_amz_algorithm = json["uploadParameters"]["formData"]["x-amz-algorithm"].ToString();
                    x_amz_date = json["uploadParameters"]["formData"]["x-amz-date"].ToString();
                    x_amz_security_token = json["uploadParameters"]["formData"]["x-amz-security-token"].ToString();
                    return true;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                    return false;
                }
            }
            catch (Exception e)
            {
               if(result1.StatusCode == HttpStatusCode.Conflict)
                {
                    
                }
                Console.WriteLine(e);
            }

            return false;
        }

        /// <summary>
        /// Assign an existing alias to the updated AppBundle
        /// </summary>
        public bool ReAssignAlias(string appId,string aliasName)
        {
            var url = $"https://developer.api.autodesk.com/da/us-east/v3/appbundles/{appId}/aliases/{aliasName}";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "PATCH";

            httpRequest.Headers["Authorization"] = "Bearer " + token;
            httpRequest.ContentType = "application/json";

            var data = new { version = version };

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
                    AppaliasName = aliasName;
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("You have data for your nickname. Cann't patch. You have to delete first");
                return false;
            }
            return false;
        }
        public bool UpdateExistingAppBundle(string appId,string description, string updateFilepath)
        {
            if (getParametersForUpdating(appId, "Autodesk.Revit+2022", description))
            {
                if (UploadAppBundle(updateFilepath))
                {
                    return true;
                }
            }
            return false;
        }

        public bool UpdateExistingAppBundle(string appId, string description, string updateFilepath,string updateAlias)
        {
            if (getParametersForUpdating(appId, "Autodesk.Revit+2022", description))
            {
                if (UploadAppBundle(updateFilepath))
                {
                    try { ReAssignAlias(appId, updateAlias); } catch(Exception e) { Console.WriteLine(e.ToString()); }
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion

        #region For Task 5
        public List<string> GetActivitys(string nickname)
        {
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri($"https://developer.api.autodesk.com/");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/da/us-east/v3/activities");
            
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;

                List<string> resultArray = new List<string>();
                JObject json = JObject.Parse(jsonResponse);
                JArray dataArray = (JArray)json["data"];
                JArray includedArray = (JArray)dataArray;
                for (var i = 0; i < includedArray.Count; i++)
                {
                    if (includedArray[i].ToString().Contains($"{nickname}.") && !includedArray[i].ToString().Contains("$LATEST"))
                        resultArray.Add(includedArray[i].ToString());
                }
                return resultArray;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }
        public bool CreateNewActivity(string nickname,string appid,string alias,string activityId)
        {
            object sedingData = new
            {
                id = activityId,
                commandLine = new object[] { $"$(engine.path)\\\\revitcoreconsole.exe /i \"$(args[rvtFile].path)\" /al \"$(appbundles[{appid}].path)\"" },
                parameters = new
                {
                    rvtFile = new
                    {
                        zip = false,
                        ondemand = false,
                        verb = "get",
                        description = "Input Revit model",
                        required = true,
                        localName = "input.rvt",
                    },
                    result = new
                    {
                        zip = false,
                        ondemand =false,
                        verb = "put",
                        description ="Result",
                        required ="true",
                        localName = "result.txt"
                    }
                },
                engine = "Autodesk.Revit+2022",
                appbundles =new object[] {$"{nickname}.{appid}+{alias}"}
            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"da/us-east/v3/activities");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if(result1.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            
            
            return false;

        }

        public bool CreateActivtyAlias(string activityid,string aliasName)
        {
            object sedingData = new
            {
                version=1,
                id=aliasName
            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"da/us-east/v3/activities/{activityid}/aliases");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if(result1.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

//            JObject json = JObject.Parse(jsonResponse);

            return false;
        }



        public string existingActivityVersion;
        public bool UpdateExistingActivity(string nickname,string appid, string alias, string activityid)
        {
            object sedingData = new
            {
                commandLine = new object[] { $"$(engine.path)\\\\revitcoreconsole.exe /i \"$(args[rvtFile].path)\" /al \"$(appbundles[{appid}].path)\"" },
                parameters = new
                {
                    rvtFile = new
                    {
                        zip = false,
                        ondemand = false,
                        verb = "get",
                        description = "Input Revit model",
                        required = true,
                        localName = "input.rvt",
                    },
                    result = new
                    {
                        zip = false,
                        ondemand = false,
                        verb = "put",
                        description = "Result",
                        required = "true",
                        localName = "result.txt"
                    }
                },
                engine = "Autodesk.Revit+2022",
                appbundles = new object[] { $"{nickname}.{appid}+{alias}" },
                description = $"{activityid} file Updated."

            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"da/us-east/v3/activities/{activityid}/versions");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if(result1.StatusCode == HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(jsonResponse);
                    existingActivityVersion = json["version"].ToString();
                    return true;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            
            return false;
        }

        public bool AssignAliasToUpdatedActivity(string activityId,string activityAlias)
        {
            var url = $"https://developer.api.autodesk.com/da/us-east/v3/activities/{activityId}/aliases/{activityAlias}";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "PATCH";

            httpRequest.Headers["Authorization"] = "Bearer " + token;
            httpRequest.ContentType = "application/json";

            var data = new { version = existingActivityVersion };

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
                    return true;
                }
                if (httpResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    /// in case of conflict, so change the name
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("You have data for your nickname. Cann't patch. You have to delete first");
                return false;
            }
            return false;
        }
        #endregion

        #region For Task 6

        public string BucketKey;
//        public string 
        public bool CreateBucket(string bucketkey)
        {
            object sedingData = new
            {
                bucketKey = bucketkey,
                access ="full",
                policyKey = "transient"
            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://developer.api.autodesk.com/oss/v2/buckets");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if(result1.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine(jsonResponse);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            JObject json = JObject.Parse(jsonResponse);

            return false;

        }

        public string uploadkey;
        public string uploadExpiration;
        public string urlExpiration;
        public List<string> uploadUrls;

        /// <summary>
        /// for single file
        /// </summary>
        /// <param name="bucketname"></param>
        /// <param name="objectkey"></param>
        /// <returns>signed url string</returns>
        public string GenerateSignedS3Url(string bucketname,string objectkey,out string uploadUrlResponseKey)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            string jsonResponse = string.Empty;
            HttpResponseHeaders headers = null;

            try
            {
                HttpResponseMessage result1 = client.GetAsync($"https://developer.api.autodesk.com/oss/v2/buckets/{bucketname}/objects/{objectkey}/signeds3upload").GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                headers = result1.Headers;
                if(result1.StatusCode == HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(jsonResponse);
                    uploadkey = json["uploadKey"].ToString();
                    uploadExpiration = json["uploadExpiration"].ToString();
                    urlExpiration = json["urlExpiration"].ToString();
                    uploadUrls = new List<string>();
                    JArray dataArray = (JArray)json["urls"];
                    for(var i=0;i<dataArray.Count;i++)
                    {
                        uploadUrls.Add(dataArray[i].ToString());
                    }
                    uploadUrlResponseKey = uploadkey;
                    return uploadUrls[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            uploadUrlResponseKey = "";
            return "";
        }
        /*
        public void GenerateSignedS3Urls(string bucketName)
        {
            var url = $"https://developer.api.autodesk.com/oss/v2/buckets/{bucketName}/objects/random_file.bin/signeds3upload";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.Headers["Authorization"] = "Bearer eYeL5gYxAT2j3u9TEerxoJoToNbi";
            httpRequest.ContentType = "application/json";
            httpRequest.Headers["x-ads-meta-Content-Type"] = "application/octet-stream";

            var data = @"{
    ""uploadKey"": ""{UPLOAD_KEY_PROVIDED_FROM_GET_UPLOAD_URLS_RESPONSE}""
  }";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            Console.WriteLine(httpResponse.StatusCode);

        }*/

        public bool UploadFileToSignedUrl(string signedurl,string filePath)
        {
            var url = signedurl;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "PUT";

            httpRequest.ContentType = "text/plain";

            Stream rs = httpRequest.GetRequestStream();
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();
            rs.Close();
            /*
            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                FileStream fileStream = File.OpenRead(filePath);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    streamWriter.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();


                var streamContent = new StreamContent(fileStream);
                var fileContent = new ByteArrayContent(streamContent.ReadAsByteArrayAsync().Result);
                streamWriter.Write(fileContent);
            }*/

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            return true;
        }

        public string CompleteUploading(string bucketKey,string objectKey,string uploadUrlResponseKey)
        {
            var url = $"https://developer.api.autodesk.com/oss/v2/buckets/{bucketKey}/objects/{objectKey}/signeds3upload";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.Headers["Authorization"] = "Bearer "+token;
            httpRequest.ContentType = "application/json";

            var data = new { uploadKey = uploadUrlResponseKey  };

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data).ToString());
            }

            
            try {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if(httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    JObject json;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        json = JObject.Parse(result);
                    }
                    var objectId = json["objectId"].ToString();
                    return objectId;
                }
                
            }
            catch (Exception e)
            {

            }

            return "";
        }


        public string GetDownloadUrl(string bucketkey, string objectkey)
        {
            object sedingData = new {};

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"oss/v2/buckets/{bucketkey}/objects/{objectkey}/signed");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if(result1.StatusCode == HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(jsonResponse);
                    return json["signedUrl"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return "";
        }

        public string GetUploadUrl(string bucketkey, string objectkey)
        {
            object sedingData = new { };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"oss/v2/buckets/{bucketkey}/objects/{objectkey}/signed?access=readwrite");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
                if (result1.StatusCode == HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(jsonResponse);
                    return json["signedUrl"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return "";
        }
        #endregion

        
        #region For Task 7

        //return workItemId
        public string CreateWorkItem(string nickname, string activityid, string activityAlias,string downloadUrl, string uploadUrl, string pathInzip, out string status)
        {
            var url = "https://developer.api.autodesk.com/da/us-east/v3/workitems";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.Headers["Authorization"] = "Bearer " + token;
            httpRequest.ContentType = "application/json";

            var data = new {
                activityId =  $"{nickname}.{activityid}+{activityAlias}",
                arguments = new
                {
                    rvtFile = new
                    {
                        url= downloadUrl,
                        pathInZip= pathInzip
                    },
                    result = new
                    {
                        verb = "put",
                        url = uploadUrl
                    }
                }
            };

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data).ToString());
            }


            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    JObject json;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        json = JObject.Parse(result);
                    }
                    var id = json["id"].ToString();
                    status=json["status"].ToString();
                    return id;
                }

            }
            catch (Exception e)
            {

            }
            status = "";
            return "";
        }

        public string CreateWorkItem(string nickname, string activityid, string activityAlias, string downloadUrl, string uploadUrl, out string status)
        {
            var url = "https://developer.api.autodesk.com/da/us-east/v3/workitems";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.Headers["Authorization"] = "Bearer " + token;
            httpRequest.ContentType = "application/json";

            var data = new
            {
                activityId = $"{nickname}.{activityid}+{activityAlias}",
                arguments = new
                {
                    rvtFile = new
                    {
                        url = downloadUrl,
                    },
                    result = new
                    {
                        verb = "put",
                        url = uploadUrl
                    }
                }
            };

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data).ToString());
            }


            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    JObject json;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        json = JObject.Parse(result);
                    }
                    var id = json["id"].ToString();
                    status = json["status"].ToString();
                    return id;
                }

            }
            catch (Exception e)
            {

            }
            status = "";
            return "";
        }

        public string CheckStatusOfItem(string workItemId)
        {
            var url = $"https://developer.api.autodesk.com/da/us-east/v3/workitems/{workItemId}";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "GET";

            httpRequest.Headers["Authorization"] = "Bearer " + token;
            httpRequest.ContentType = "application/json";
/*
            var data = new {};

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data).ToString());
            }*/
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    JObject json;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        json = JObject.Parse(result);
                    }
                    var status = json["status"].ToString();
                    if(status == "success")
                    {
                        var a = 1;
                    }
                    return status;
                }

            }
            catch (Exception e)
            {

            }
            return "";
        }


        public string GetResultString(string buketkey, string objectkey)
        {
            var url = $"https://developer.api.autodesk.com/oss/v2/buckets/{buketkey}/objects/{objectkey}";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Headers["Authorization"] = $"Bearer "+token;
            httpRequest.ContentType = "application/json";
            httpRequest.Headers["cache-control"] = "no-cache";

            var resultstring = "";
            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                resultstring = result;
            }
            return resultstring;
        }
        #endregion*/
        #endregion
    }
}