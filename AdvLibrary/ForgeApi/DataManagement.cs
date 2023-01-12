using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AdvLibrary.ForgeApi.Model;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Autodesk.Forge.Client;
using System.Net.Http;
using System.Net.Http.Headers;

using RestSharp;

namespace AdvLibrary.ForgeApi
{
    public class DataManagement
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

        #region constants
        const string baseURL = "https://developer.api.autodesk.com";
        
        #endregion

        #region Private Members
        private string client_id;
        private string client_secret;
        private string token;
        private string refresh_token;
        private string code;
        private string redirecturl = "http://localhost:8080/api/auth/callback";
        #endregion

        #region Public Properties
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
        #endregion


        public static HttpListener listener;
        public static string url = "http://localhost:8080/";
        public static int pageViews = 0;
        public static int requestCount = 0;

        public static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
                "<h1>You authorized</h1>" +
            "  </body>" +
            "</html>";
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

                
                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "GET" && req.Url.AbsolutePath=="/api/auth/callback"))
                {
                    runServer = false;
                    code=req.QueryString["code"];
                }

                byte[] data = Encoding.UTF8.GetBytes(pageData);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
                //                resp.Close();
            }
            return code;
        }

        #region Constructor
        public DataManagement(string client_id, string client_secret)
        {
            this.client_id = client_id;
            this.client_secret = client_secret;
            // Get the token
            //
  //          this.token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IlU3c0dGRldUTzlBekNhSzBqZURRM2dQZXBURVdWN2VhIn0.eyJzY29wZSI6WyJkYXRhOmNyZWF0ZSIsImRhdGE6d3JpdGUiLCJkYXRhOnJlYWQiXSwiY2xpZW50X2lkIjoiejlBbnhPaHJ5eGNUU1R6eUEyb1JSSkNhaVlHSU1yNmciLCJhdWQiOiJodHRwczovL2F1dG9kZXNrLmNvbS9hdWQvYWp3dGV4cDYwIiwianRpIjoiSDltMllETFJJM0JpTVhVQk9OcnJ2ejNGbWJya2w5MkJUYnFWcExycndRbGhoOFlZazRTVWpsa0wxMHBSMHlQeSIsInVzZXJpZCI6IkhGUEpGV1FIS0RQU1Y3OFoiLCJleHAiOjE2NzM0ODA4MTJ9.FHvexKe53MPwhre6kRBpeffM80LkJBqQ7oNm3RzhTiKmyYhimw1WB8h-ChhKaCm7CtVn8DSrsmnK6JnZgkMcvBfFx6Ah53VqbRlQv1OalDCQK2TX7ANG0Mq7Hh0n1VhVDC-vBxZEfz6GtUBW4qJ4gMeokwr-V52Ju6kZ4CLGPKv0dwHGpw1vS_Kxv86hogRUgXyErDoCrxNrLWALpERqMY1WN-tArmYdTAc5wVb_1eXUGwE0P-Ilbxm6oAaOQg_e512wKc79ofjOoi8up2RtZ1I42Za_X4TufTWpM1Fe1G5v1R0JuMlLFaSucHzv0lkpBENf9jPJ8E3PM9AsAMCTCw";
            this.code=GetCode().Result;

        }
        #endregion
        public async Task<string> GetCode()
        {
            
            var requesturl = $"https://developer.api.autodesk.com/authentication/v1/authorize?response_type=code&client_id={client_id}&redirect_uri={redirecturl}&scope=data:create%20data:read%20data:write";
            System.Diagnostics.Process.Start(requesturl);

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
        public async Task GetToken()
        {
            string grantType = "authorization_code";
            
            List<KeyValuePair<string, string>> postData=new List<KeyValuePair<string, string>>();

            postData.Add(new KeyValuePair<string, string>("client_id",client_id));
            postData.Add(new KeyValuePair<string, string>("client_secret", client_secret));
            postData.Add(new KeyValuePair<string, string>("grant_type", grantType));
            postData.Add(new KeyValuePair<string, string>("redirect_uri", redirecturl));
            postData.Add(new KeyValuePair<string, string>("code", code));
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            try
            {
                HttpResponseMessage result1=client.PostAsync("https://developer.api.autodesk.com/authentication/v1/gettoken", new FormUrlEncodedContent(postData)).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            JObject json = JObject.Parse(jsonResponse);
            this.token = json["access_token"].ToString();
            this.refresh_token= json["refresh_token"].ToString();
        }

        public async Task<IList<AutoHub>> GetHubsAsync()
        {
            IList<AutoHub> nodes = new List<AutoHub>();

            HubsApi hubsApi = new HubsApi();
            hubsApi.Configuration.AccessToken = Token;

            var hubs = await hubsApi.GetHubsAsync();
            foreach (KeyValuePair<string, dynamic> hubInfo in new DynamicDictionaryItems(hubs.data))
            {
                string hubId = hubInfo.Value.id;
                string hubName = hubInfo.Value.attributes.name;
                string hubType = hubInfo.Value.type;
                AutoHub hub = new AutoHub(hubId, hubName, hubType);
                nodes.Add(hub);
            }

            return nodes;
        }

        public async Task<IList<AutoProject>> GetProjectsAsync(string hubId)
        {
            IList<AutoProject> nodes = new List<AutoProject>();

            ProjectsApi projectsApi = new ProjectsApi();
            projectsApi.Configuration.AccessToken = Token;

            var projects = await projectsApi.GetHubProjectsAsync(hubId);
            foreach (KeyValuePair<string, dynamic> projectInfo in new DynamicDictionaryItems(projects.data))
            {
                string projectId = projectInfo.Value.id;
                string projectName = projectInfo.Value.attributes.name;
                string projectType = projectInfo.Value.type;
                AutoProject project = new AutoProject(hubId, projectId, projectName, projectType);
                nodes.Add(project);
            }

            return nodes;
        }

        public async Task<IList<AutoFolder>> GetFoldersAsync(string hubId, string projectId)
        {

            IList<AutoFolder> nodes = new List<AutoFolder>();
            
            ProjectsApi projectsApi = new ProjectsApi();
            projectsApi.Configuration.AccessToken = Token;

            var folders = await projectsApi.GetProjectTopFoldersAsync(hubId, projectId);
            foreach (KeyValuePair<string, dynamic> folderInfo in new DynamicDictionaryItems(folders.data))
            {
                string folderId = folderInfo.Value.id;
                string folderName = folderInfo.Value.attributes.displayName;
                string folderType = folderInfo.Value.type;
                AutoFolder folder = new AutoFolder(hubId, projectId, folderId, folderName, folderType);
                nodes.Add(folder);
            }

            return nodes;
        }

        public async Task<IList<AutoFile>> GetFilesAsync(string projectId, string folderId)
        {
            IList<AutoFile> nodes = new List<AutoFile>();
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization",$"Bearer {token}");
            string jsonResponse = string.Empty;
            try
            {
                HttpResponseMessage result1 = client.GetAsync($"https://developer.api.autodesk.com/data/v1/projects/{projectId}/folders/{folderId}/contents?filter[attributes.extension.type]=items%3Aautodesk.bim360%3AC4RModel").GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            JObject json = JObject.Parse(jsonResponse);
            JArray array = (JArray)json["data"];
            for(int i = 0; i < array.Count; i++)
            {
                string contentId = array[i]["id"].ToString();
                string contentName = array[i]["attributes"]["displayName"].ToString();
                string contentType = array[i]["type"].ToString();
                AutoFile content = new AutoFile(contentId, projectId, folderId, contentName, contentType);
                nodes.Add(content);
            }
            return nodes;
        }

        // to verify whether a model needs to be published, item Type should be 'items'
        public async Task<bool> IsNeedPublishAsync(string projectId, string folderId, string fileId)
        {
            //Your code here will Verify Whether a Model Needs to Be Published, return true/false
            if (projectId == null)
            {
                throw new ApiException(400, "Missing required parameter 'projectId' when calling IsNeedPublishAsync");
            }

            if (folderId == null)
            {
                throw new ApiException(400, "Missing required parameter 'folderId' when calling IsNeedPublishAsync");
            }

            if (fileId == null)
            {
                throw new ApiException(400, "Missing required parameter 'fileId' when calling IsNeedPublishAsync");
            }
        
            object sedingData = new
            {
                jsonapi = new
                {
                    version = "1.0"
                },
                data = new
                {
                    type = "commands",
                    attributes = new
                    {
                        extension = new
                        {
                            type = "commands:autodesk.bim360:C4RModelGetPublishJob",
                            version = "1.0.0"
                        }
                    },
                    relationships = new
                    {
                        resources = new
                        {
                            data = new object[] { new { type = "items", id = fileId } }
                        }
                    }
                }
            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress= new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"data/v1/projects/{projectId}/commands");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            JObject json = JObject.Parse(jsonResponse);
            if (json["errors"] != null)
            {
                throw new Exception(json["errors"][0]["detail"].ToString());
            }
            if (json["data"] != null)
            {
                return false;
            }
            if (json["data"]["attributes"]["status"].ToString()== "processing" || json["data"]["attributes"]["status"].ToString() == "complete")
            {
                return false;
            }
            return true;

        }

        public async Task<string> DoPublishLatestAsync(string projectId, string folderId, string fileId)
        {
            //if sucess return "committed" string

            if (projectId == null)
            {
                throw new ApiException(400, "Missing required parameter 'projectId' when calling IsNeedPublishAsync");
            }

            if (folderId == null)
            {
                throw new ApiException(400, "Missing required parameter 'folderId' when calling IsNeedPublishAsync");
            }

            if (fileId == null)
            {
                throw new ApiException(400, "Missing required parameter 'fileId' when calling IsNeedPublishAsync");
            }

            object sedingData = new
            {
                jsonapi = new
                {
                    version = "1.0"
                },
                data = new
                {
                    type = "commands",
                    attributes = new
                    {
                        extension = new
                        {
                            type = "commands:autodesk.bim360:C4RModelPublish",
                            version = "1.0.0"
                        }
                    },
                    relationships = new
                    {
                        resources = new
                        {
                            data = new object[] { new { type = "items", id = fileId } }
                        }
                    }
                }
            };

            var sendJsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sedingData).ToString();
            string jsonResponse = string.Empty;
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://developer.api.autodesk.com/");
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"data/v1/projects/{projectId}/commands");
            request.Content = new StringContent(sendJsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage result1 = client.SendAsync(request).GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            JObject json = JObject.Parse(jsonResponse);
            if (json["errors"] != null)
            {
                throw new Exception(json["errors"][0]["detail"].ToString());
            }
            return json["data"]["attributes"]["status"].ToString();
        }

        public async Task<bool> HasFinishedPublishingAsync(string projectId, string folderId, string fileId)
        {
            //Your code here Verify the Model Has Finished Publishing, return true/false

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            string jsonResponse = string.Empty;
            try
            {
                HttpResponseMessage result1 = client.GetAsync($"https://developer.api.autodesk.com/data/v1/projects/{projectId}/items/{fileId}").GetAwaiter().GetResult();
                jsonResponse = result1.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            JObject json = JObject.Parse(jsonResponse);
            JArray array = (JArray)json["included"];
            try
            {
                for (int i = 0; i < array.Count; i++)
                {
                   if (json["included"][0]["attributes"]["extension"]["data"]["extractionState"].ToString()!= "SUCCESS")
                        return false;
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.ToString());
   //             return false;
            }

            return true;
        }


    }
    
    
}
