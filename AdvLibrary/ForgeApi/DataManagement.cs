using Newtonsoft.Json.Linq;
using System;
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
        private string token;
        private string redirecturl = "http://localhost:8080/api/auth/callback";
        private int retryafter = 0;
        #endregion

        #region Public Properties
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
        #endregion

        #region Constructor
        public DataManagement(string token)
        {
            this.token = token;
        }
        #endregion

        #region public functions
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

        public async Task<IList<AutoFolder>> GetTopFoldersAsync(string hubId, string projectId)
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
                AutoFolder folder = new AutoFolder(hubId, projectId, folderId, folderName, folderType, folderName);
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
            HttpResponseHeaders headers = null;
            bool passed = false;
            while (!passed)
            {
                try
                {
                    HttpResponseMessage result1 = client.GetAsync($"https://developer.api.autodesk.com/data/v1/projects/{projectId}/folders/{folderId}/contents?filter[attributes.extension.type]=items%3Aautodesk.bim360%3AC4RModel").GetAwaiter().GetResult();
                    jsonResponse = result1.Content.ReadAsStringAsync().Result;
                    headers = result1.Headers;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                if (headers.RetryAfter != null && headers.RetryAfter.ToString() != "")
                {
                    retryafter = int.Parse(headers.RetryAfter.ToString());
                    Console.WriteLine($"Quota limit exceed. Retry after {retryafter} seconds");
                    await Task.Delay(retryafter * 1000);
                }
                else
                {
                    passed = true;
                }
            }

            JObject json = JObject.Parse(jsonResponse);
            JArray dataArray = (JArray)json["data"];
            JArray includedArray = (JArray)json["included"];
            for (int i = 0; i < dataArray.Count; i++)
            {
                string contentId = dataArray[i]["id"].ToString();
                string contentName = dataArray[i]["attributes"]["displayName"].ToString();
                string contentType = dataArray[i]["type"].ToString();
                string createtime = includedArray[i]["attributes"]["createTime"].ToString();
                string createUserId = includedArray[i]["attributes"]["createUserId"].ToString();
                string createUserName = includedArray[i]["attributes"]["createUserName"].ToString();
                string lastModifiedTime = includedArray[i]["attributes"]["lastModifiedTime"].ToString();
                string lastModifiedUserId = includedArray[i]["attributes"]["lastModifiedUserId"].ToString();
                string lastModifiedUserName = includedArray[i]["attributes"]["lastModifiedUserName"].ToString();
                string versionNumber = includedArray[i]["attributes"]["versionNumber"].ToString();
                string mimeType = includedArray[i]["attributes"]["mimeType"].ToString();
                string storageSize = includedArray[i]["attributes"]["storageSize"].ToString();
                string fileType = includedArray[i]["attributes"]["fileType"].ToString();
                AutoFile content = new AutoFile();

                content.ContentId = contentId;
                content.ProjectId = projectId;
                content.FolderId = folderId;
                content.Name = contentName;
                content.Type = contentType;
                content.CreateTime = createtime;
                content.CreateUserId = createUserId;
                content.CreateUserName = createUserName;
                content.LastModifiedTime = lastModifiedTime;
                content.LastModifiedUserId = lastModifiedUserId;
                content.LastModifiedUserName = lastModifiedUserName;
                content.VersionNumber = versionNumber;
                content.MimeType = mimeType;
                content.StorageSize = storageSize;
                content.FileType = fileType;
                
                AutoFileExtension extension = new AutoFileExtension();
                extension.version = includedArray[i]["attributes"]["extension"]["version"].ToString();
                extension.modelVersion = includedArray[i]["attributes"]["extension"]["data"]["modelVersion"].ToString();
                extension.projectGuid = includedArray[i]["attributes"]["extension"]["data"]["projectGuid"].ToString();
                extension.originalItemUrn = includedArray[i]["attributes"]["extension"]["data"]["originalItemUrn"].ToString();
                extension.isCompositeDesign = includedArray[i]["attributes"]["extension"]["data"]["isCompositeDesign"].ToString();
                extension.modelType = includedArray[i]["attributes"]["extension"]["data"]["modelType"].ToString();
                extension.mimeType = includedArray[i]["attributes"]["extension"]["data"]["mimeType"].ToString();
                extension.modelGuid = includedArray[i]["attributes"]["extension"]["data"]["modelGuid"].ToString();
                extension.processState = includedArray[i]["attributes"]["extension"]["data"]["processState"].ToString();
                extension.extractionState = includedArray[i]["attributes"]["extension"]["data"]["extractionState"].ToString();
                extension.splittingState = includedArray[i]["attributes"]["extension"]["data"]["splittingState"].ToString();
                extension.reviewState = includedArray[i]["attributes"]["extension"]["data"]["reviewState"].ToString();
                extension.revisionDisplayLabel = includedArray[i]["attributes"]["extension"]["data"]["revisionDisplayLabel"].ToString();
                extension.sourceFileName = includedArray[i]["attributes"]["extension"]["data"]["sourceFileName"].ToString();
                extension.conformingStatus = includedArray[i]["attributes"]["extension"]["data"]["conformingStatus"].ToString();

                content.Extension = extension;

                nodes.Add(content);
            }
            return nodes;
        }

        public async Task<IList<AutoFolder>> GetFoldersAsync(string hubId, string projectId)
        {
            IList<AutoFolder> nodes = await GetTopFoldersAsync(hubId, projectId);
            int index = 0;
            while (index < nodes.Count)
            {
                var subfolders = GetSubFoldersAsync(hubId,projectId, nodes[index].FolderId, nodes[index].FolderPath).Result;
                for(var i=0;i<subfolders.Count; i++)
                    nodes.Add(subfolders[i]);
                index++;
            }
            return nodes;
        }

        public async Task<IList<AutoFolder>> GetSubFoldersAsync(string hubId, string projectId, string folderId, string parentfolderPath)
        {
            IList<AutoFolder> nodes = new List<AutoFolder>();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            string jsonResponse = string.Empty;
            HttpResponseHeaders headers=null;
            bool passed = false;
            while (!passed)
            {
                try
                {
                    HttpResponseMessage result1 = client.GetAsync($"https://developer.api.autodesk.com/data/v1/projects/{projectId}/folders/{folderId}/contents").GetAwaiter().GetResult();
                    jsonResponse = result1.Content.ReadAsStringAsync().Result;
                    headers = result1.Headers;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (headers.RetryAfter != null && headers.RetryAfter.ToString() != "")
                {
                    retryafter = int.Parse(headers.RetryAfter.ToString());
                    Console.WriteLine($"Quota limit exceed. Retry after {retryafter} seconds");
                    await Task.Delay(retryafter * 1000);
                }
                else
                {
                    passed = true;
                }
            }

            JObject json = JObject.Parse(jsonResponse);
            JArray array = (JArray)json["data"];
            for (int i = 0; i < array.Count; i++)
            {
                if(array[i]["type"].ToString() == "folders")
                {
                    string contentId = array[i]["id"].ToString();
                    string contentName = array[i]["attributes"]["displayName"].ToString();
                    string contentType = array[i]["type"].ToString();
                    AutoFolder content = new AutoFolder(hubId, projectId, contentId, contentName, contentType, parentfolderPath+ "/contentName");
                    nodes.Add(content);
                }
            }
            return nodes;
        }

        // to verify whether a model needs to be published, item Type should be 'items'
        public bool IsNeedPublishAsync(string projectId, string folderId, string fileId)
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
            if (string.IsNullOrEmpty(json["data"].ToString()))
            {
                return true;
            }

            if (json["errors"] != null)
            {
                throw new Exception(json["errors"][0]["detail"].ToString());
            }
           
            if (json["data"]["attributes"]["status"].ToString()== "processing" || json["data"]["attributes"]["status"].ToString() == "complete")
            {
                return false;
            }
            return true;

        }

        public string DoPublishLatestAsync(string projectId, string folderId, string fileId)
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

        public bool HasFinishedPublishingAsync(string projectId, string folderId, string fileId)
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
                   if (json["included"][i]["attributes"]["extension"]["data"]["processState"].ToString()!= "PROCESSING_COMPLETE")
                        return false;
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.ToString());
            }
                
            return true;
        }

        #endregion
    }
}
