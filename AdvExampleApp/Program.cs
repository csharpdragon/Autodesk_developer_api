using System;
using System.Collections.Generic;
using System.Linq;
using AdvLibrary.ForgeApi;
using AdvLibrary.ForgeApi.Model;
namespace AdvExampleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string client_id = "z9AnxOhryxcTSTzyA2oRRJCaiYGIMr6g";
            string client_secret = "Mc7fd0f574c8b4e2";
            var authenticate = new Authentication(client_id, client_secret, "V5MzzOzwgIPMbC3NDzlkyKSPsUYo48I25cUIMzkcPb");


            var dtoken= authenticate.GetDesignAutomationToken();
            var designAutomation = new DesignAutomation(dtoken);
        //    var designAutomation = new DesignAutomation();

            /////create your nickname
            //  designAutomation.CreateNickName("pdragon");
            /////

            var nickname = "pdragon";

            ///////task 4

            var existiedBundle = false;
            var appId = "CountSampleApp6";
            var alias = "test2";
            
            if (designAutomation.RegisterAppBundle(appId, "Autodesk.Revit+2018", "Count AppBundle based on Revit 2023"))
            {
                if (designAutomation.UploadAppBundle("D:\\test\\DeleteWallsApp.zip"))
                {
                    Console.WriteLine("uploaded");
                }
                else { Console.WriteLine("failed"); }

                if (existiedBundle = designAutomation.CreateAliasForAppBundle(appId, alias))
                {
                    Console.WriteLine("alias created");
                }
                else { Console.WriteLine("alias create failed"); }
            }

            //update part//
            if (existiedBundle)
            {
                if(designAutomation.UpdateExistingAppBundle(appId, "Autodesk.Revit+2018", "Count AppBundle based on Revit 2018 Update", "D:\\test\\DeleteWallsApp.zip", alias))
                {
                    Console.WriteLine("updated");
                }
                else
                {
                    Console.WriteLine("failed updating");
                }

            }

            //  UpdateExistingAppBundle("DeleteWallsApp5")
            //////end task 4

          
            /////for task 5
            var activityAlias = "test";
            var activityId = "countActivity11";
            var activitycreated = false;
   
          if (designAutomation.CreateNewActivity(nickname, appId, alias, activityId, "Autodesk.Revit+2018"))
          {
              ////create alias for activity
              if (activitycreated=designAutomation.CreateActivtyAlias(activityId, activityAlias))
              {
                  Console.WriteLine("activity and activity alias created");
              }
              else
              {
                  Console.WriteLine("activity alias failed to create");
              }
          }

          if (activitycreated)
          {
              if(designAutomation.UpdateExistingActivity(nickname,appId,alias,activityId, "Autodesk.Revit+2018"))
              {
                  if (designAutomation.AssignAliasToUpdatedActivity(activityId,activityAlias))
                  {
                      Console.WriteLine("activity alias updated");
                  }
                  else
                  {
                      Console.WriteLine("uptivity alias updating failed");
                  }
              }
          }
          ////end for task 5
          
            ///
            ///The Bucket Key must be unique throughout all of the OSS service. 
            string bucketname = "pdragon0512bucket"; //// must be [a-z],[0-9],[_]
            string signedUrlForUpload = "";
            string uploadUrlResponseKey = "";
            var objectKey = "DeleteWalls.rvt";
//            designAutomation.CreateBucket(bucketname);

            signedUrlForUpload = designAutomation.GenerateSignedS3Url(bucketname, objectKey, out uploadUrlResponseKey);

            var ojbectId="";
            var downloadUrl = "";
            var uploadUrl = "";

            if (designAutomation.UploadFileToSignedUrl(signedUrlForUpload, "D:\\DeleteWalls.rvt"))
            {
                ojbectId = designAutomation.CompleteUploading(bucketname, objectKey, uploadUrlResponseKey);
                if (!string.IsNullOrEmpty(ojbectId))
                {
                    downloadUrl = designAutomation.GetDownloadUrl(bucketname, objectKey);
                    uploadUrl = designAutomation.GetUploadUrl(bucketname, objectKey);
                }
            }

            ////start task 7

            var itemstatus = "";

            var workId = "";
            if (!string.IsNullOrEmpty(downloadUrl) && !string.IsNullOrEmpty(uploadUrl)){
                workId = designAutomation.CreateWorkItem(nickname, activityId, activityAlias, downloadUrl, uploadUrl,out itemstatus);
            }

            if (!string.IsNullOrEmpty(workId))
            {
                while (itemstatus!="success")
                {
                    itemstatus = designAutomation.CheckStatusOfItem(workId);
                }
            }

        }
    }
    }
