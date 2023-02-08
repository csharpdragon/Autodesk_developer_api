using System;
using System.Collections.Generic;
using System.Linq;
using AdvLibrary.ForgeApi;
using AdvLibrary.ForgeApi.Model;
namespace AdvExampleApp
{
    internal class Program
    {
        public static int ReadInputFromConsole(int max)
        {
            
            var index = 0;
            var passed = false;
            while (!passed)
            {
                var indexString = Console.ReadLine();
                try
                {
                    index = int.Parse(indexString);
                    if (index < 0 || index > max)
                    {
                        passed = false;
                    }
                    else passed = true;
                }
                catch (Exception e)
                {
                    passed = false;
                    Console.WriteLine("Plz enter the number correctly");
                }
            }
            return index;
        }
        static void Main(string[] args)
        {

            string client_id = "z9AnxOhryxcTSTzyA2oRRJCaiYGIMr6g";
            string client_secret = "Mc7fd0f574c8b4e2";
            var authenticate = new Authentication(client_id, client_secret, "V5MzzOzwgIPMbC3NDzlkyKSPsUYo48I25cUIMzkcPb");

            

            

            ////////////////here, start the design automation
            var dtoken = authenticate.GetDesignAutomationToken();
            var designAutomation = new DesignAutomation(dtoken);
            
            //            designAutomation.DeleteNickName();

            var nickname = designAutomation.GetNickName();

            var tempBundles = designAutomation.GetAllBundles(nickname);
            var tempActivities = designAutomation.GetActivitys(nickname);
            if (string.IsNullOrEmpty(nickname) || (tempActivities.Count==0 || tempActivities==null || tempBundles.Count == 0 || tempBundles == null)) ///if user didn't create nickname
            {
                Console.WriteLine("Please enter your nickname:");
                var tempNickname=Console.ReadLine();

                ////you can create nickname when you have no previous data
                /// in other case, you have to delete nickname first using designAutomation.DeleteNickName();, then you can create nickname what you want again
                if (designAutomation.CreateNickName(tempNickname))
                {
                    nickname = tempNickname;
                    Console.WriteLine("Your Nickname created successfully");
                }
            }

            ///////for task 4
            List<string> allBundles = designAutomation.GetAllBundles(nickname);
            var existiedBundle = false;


            ////bundle part
            var appId = "";
            var alias = "";

            bool needRegisterAppBundle=false;
            if (allBundles == null || allBundles.Count ==0)
                needRegisterAppBundle = true;
            else
            {
                Console.Write("Do you want to register new app bundle?(if no, you have to use registered bundle or update already existed) [y/n]:");
                var  enteredString=Console.ReadLine();
                if (enteredString.ToLower().Contains("y"))
                    needRegisterAppBundle = true;
                else needRegisterAppBundle = false;
            }

            if (needRegisterAppBundle)
            {
                Console.WriteLine("Registering App Bundle...");
                Console.Write("Enter your app bundle name:");
                appId = Console.ReadLine();

                Console.Write("Enter your bundle alias name:");
                alias = Console.ReadLine();

                Console.Write("Enter your App Description:");
                var description = Console.ReadLine();
                
                if (designAutomation.RegisterAppBundle(appId, description))
                {
                    if (designAutomation.UploadAppBundle("D:\\ttttt\\AdvAutoCount.zip"))
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
            }
            else
            {
                Console.WriteLine("Do you want to update existing bundle?[y/n]");
                var enteredString = Console.ReadLine();
                bool needToupdateBundle=false;
                if (enteredString.ToLower().Contains("y"))
                    needToupdateBundle = true;
                else needToupdateBundle = false;

                if (needToupdateBundle)
                {
                    for(var i = 0; i < allBundles.Count; i++)
                    {
                        Console.WriteLine($"[{i + 1}] {allBundles[i]}");
                    }
                    Console.WriteLine("Plz select the index of app bundle");
                    
                    var index = ReadInputFromConsole(allBundles.Count);
                    var bundle = allBundles[index - 1];
                    appId = bundle.Replace($"{nickname}.", "").Split('+')[0];
                    alias = bundle.Replace($"{nickname}.", "").Split('+')[1];

                    Console.WriteLine("Plz write your updating description");
                    var updatingDescription = Console.ReadLine();
                    //update part//
                    if (designAutomation.UpdateExistingAppBundle(appId, updatingDescription, "D:\\ttttt\\CountIt.zip", alias))
                    {
                        Console.WriteLine("updated");
                    }
                    else
                    {
                        Console.WriteLine("failed updating");
                    }
                }
                else
                {
                    for (var i = 0; i < allBundles.Count; i++)
                    {
                        Console.WriteLine($"[{i + 1}] {allBundles[i]}");
                    }
                    Console.WriteLine("Plz select the index of app bundle which you want to use:");
                    var index = ReadInputFromConsole(allBundles.Count);
                    var bundle = allBundles[index - 1];
                    appId = bundle.Replace($"{nickname}.", "").Split('+')[0];
                    alias = bundle.Replace($"{nickname}.", "").Split('+')[1];

                }
            }

            //////end task 4

            /////start task 5 activity part
            var activities=designAutomation.GetActivitys(nickname);
            var activityAlias = "";
            var activityId = "";
            bool needActivityCreate = false;
            if(activities==null || activities.Count == 0)
                needActivityCreate = true;
            else
            {
                Console.Write("Do you want to register new Activity?(if no, you have to use registered activity or update already existed) [y/n]:");
                var enteredString = Console.ReadLine();
                if (enteredString.ToLower().Contains("y"))
                    needActivityCreate = true;
                else needActivityCreate = false;
            }
            if (needActivityCreate)
            {
                Console.WriteLine("Registering Activity...");
                Console.WriteLine("Activity Name:");
                activityId = Console.ReadLine();
                Console.WriteLine("Activity Alias:");
                activityAlias = Console.ReadLine();
                if (designAutomation.CreateNewActivity(nickname, appId, alias, activityId))
                {
                    ////create alias for activity
                    if (designAutomation.CreateActivtyAlias(activityId, activityAlias))
                    {
                        Console.WriteLine("activity and activity alias created");
                    }
                    else
                    {
                        Console.WriteLine("activity alias failed to create");
                    }
                }
            }
            else
            {
                Console.WriteLine("Do you want to update your existing activity with selected or uploaded bundle? [y/n]");
                var enteredString = Console.ReadLine();
                if (enteredString.ToLower().Contains("y"))
                {
                    for (var i = 0; i < activities.Count; i++)
                    {
                        Console.WriteLine($"[{i + 1}] {activities[i]}");
                    }
                    Console.WriteLine("Plz select the index of activity to update");
                    var index = ReadInputFromConsole(activities.Count);
                    var bundle = allBundles[index - 1];
                    activityId = bundle.Replace($"{nickname}.", "").Split('+')[0];
                    activityAlias = bundle.Replace($"{nickname}.", "").Split('+')[1];

                    if (designAutomation.UpdateExistingActivity(nickname, appId, alias, activityId))
                    {
                        Console.WriteLine($"activity name: {activityId}, acitivity alias: {alias}");
                        Console.WriteLine("Do you want to update alias name? [y/n]");
                        var enteredaliasString = Console.ReadLine();
                        var newaliasname = activityAlias;
                        if (enteredaliasString.ToLower().Contains("y"))
                        {
                            Console.Write("Enter your alias name:");
                            newaliasname = Console.ReadLine();
                        }

                        if (designAutomation.AssignAliasToUpdatedActivity(activityId, newaliasname))
                        {
                            Console.WriteLine("activity alias updated");
                            activityAlias = newaliasname;
                        }
                        else
                        {
                            Console.WriteLine("uptivity alias updating failed");
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < activities.Count; i++)
                    {
                        Console.WriteLine($"[{i + 1}] {activities[i]}");
                    }
                    Console.WriteLine("Plz select the index of activity to execute");
                    var index = ReadInputFromConsole(activities.Count);
                    var bundle = allBundles[index - 1];
                    activityId = bundle.Replace($"{nickname}.", "").Split('+')[0];
                    activityAlias = bundle.Replace($"{nickname}.", "").Split('+')[1];

                }
            }

            ////end for task 5

            ///
            ///The Bucket Key must be unique throughout all of the OSS service. 
            string bucketname = "pdragon0512bucket"; //// must be [a-z],[0-9],[_]
            string signedUrlForUpload = "";
            string uploadUrlResponseKey = "";
            var objectKey = "result";
            designAutomation.CreateBucket(bucketname);

            signedUrlForUpload = designAutomation.GenerateSignedS3Url(bucketname, objectKey, out uploadUrlResponseKey);

            /////generating s3 url part for addin
            ///

            DataManagement forge = new DataManagement(authenticate.Token);

            var hubs = new List<AutoHub>();
            var projects = new List<AutoProject>();
            var topFolders = new List<AutoFolder>();
            var files = new List<AutoFile>();
            //Here I will get a list of hubs for the credentials
            hubs = new List<AutoHub>(forge.GetHubsAsync().Result);

            AutoHub myHub = hubs.Where(x => x.HubName == "ADV").First();

            projects = new List<AutoProject>(forge.GetProjectsAsync(myHub.HubId).Result);

            AutoProject myProject = projects.Where(x => x.ProjectName == "Test Project").First();
            files = new List<AutoFile>(forge.GetFilesAsync(myProject.ProjectId, "urn:adsk.wipprod:fs.folder:co.TyuUCyO5TyyaRVYASgm-qQ").Result);
            AutoFile myFile = files.Where(x => x.Name == "SHIBOLET_IT.rvt").First();

            var link = forge.FindStorageLocation(myProject.ProjectId, myFile.ContentId);

            var s3url = forge.GetS3Url(link);

            /////// end generating s3 url
            ///

            var downloadUrl = s3url;
            var uploadUrl = "";

            uploadUrl = designAutomation.GetUploadUrl(bucketname, objectKey);
 

            var itemstatus = "";

            var workId = "";
            if (!string.IsNullOrEmpty(downloadUrl) && !string.IsNullOrEmpty(uploadUrl))
            {
                workId = designAutomation.CreateWorkItem(nickname, activityId, activityAlias, downloadUrl, uploadUrl, out itemstatus);
            }

           
            if (!string.IsNullOrEmpty(workId))
            {
                Console.WriteLine("activity started. running...");
                while (itemstatus != "success" && !itemstatus.Contains("fail"))
                {
                    itemstatus = designAutomation.CheckStatusOfItem(workId);
                }
            }

            if(itemstatus == "success")
            {
                var finalresponse=designAutomation.GetResultString(bucketname, objectKey);
                Console.WriteLine(finalresponse);
                Result a = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(finalresponse);
            }

        }
    }
    }
