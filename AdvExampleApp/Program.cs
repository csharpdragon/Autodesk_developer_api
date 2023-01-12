using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvLibrary.ForgeApi;
using AdvLibrary.ForgeApi.Model;

namespace AdvExampleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Get the client id and client secret from 3rd party
            string client_id = "z9AnxOhryxcTSTzyA2oRRJCaiYGIMr6g";
            string client_secret = "Lq96viASKfBHq83e";

            // Create an instance of the class
            var forge = new DataManagement(client_id, client_secret);

            
            var hubs = new List<AutoHub>();
            var projects = new List<AutoProject>();
            var folders = new List<AutoFolder>();
            var files = new List<AutoFile>();

            //Here I will get a list of hubs and select myHub based on some properties
            hubs = new List<AutoHub>(forge.GetHubsAsync().Result);
            AutoHub myHub = hubs.Where(x => x.HubName == "ADV").First();


           
            //Here I will pass myHub to get a list of all projects in the Hub and select a project
            projects = new List<AutoProject>(forge.GetProjectsAsync(myHub.HubId).Result);
            AutoProject myProject = projects.Where(x => x.ProjectName == "Emek Hospital").First();
         
           

             //Here I will pass myProject to get list of all folder paths in the project
             folders = new List<AutoFolder>(forge.GetFoldersAsync(myHub.HubId,myProject.ProjectId).Result);


             AutoFolder myFolder = folders.Where(x => x.FolderName == "Project Files").First();
           
            //Here I will pass myFolder to get list of all files in the folder and select a revit model
            files = new List<AutoFile>(forge.GetFilesAsync(myProject.ProjectId,myFolder.FolderId).Result);
            AutoFile myFile = files.Where(x => x.Name == "Emek_Hospital_Superposition_2021_V2.rvt").First();

            /*
            var flag = false;
            while (!flag)
            {
                flag = forge.HasFinishedPublishingAsync(myProject.ProjectId, myFolder.FolderId, myFile.ContentId).Result;
            }*/
            try
            {
                bool needPublish = forge.IsNeedPublishAsync(myProject.ProjectId, myFolder.FolderId, myFile.ContentId).Result;
          /*      if (needPublish)
                {
                      var result = forge.DoPublishLatestAsync(myProject.ProjectId, myFolder.FolderId, myFile.ContentId).Result;
                        //if committed
                      if(result== "committed")
                      {
                        var flag = false;
                        while (!flag)
                        {
                            flag=forge.HasFinishedPublishingAsync(myProject.ProjectId, myFolder.FolderId, myFile.ContentId).Result;
                        }
                      }
                 }*/
             }
            catch(Exception e)
            {
                var error = e.ToString();
                Console.WriteLine(error);
            }

        }
    }
}
