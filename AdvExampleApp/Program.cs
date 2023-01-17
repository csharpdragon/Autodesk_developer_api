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


            /////create your nickname
            //  designAutomation.CreateNickName("pdragon");
            /////

            var nickname = "pdragon";

            ///////task 4

            var existiedBundle = false;
            var appId = "CountSampleApp3";
            var alias = "test1";
            if (designAutomation.RegisterAppBundle(appId, "Autodesk.Revit+2018", "Count AppBundle based on Revit 2023"))
            {
                if (designAutomation.UploadAppBundle("D:\\CountSampeApp.7z"))
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

            //*update part*//
            if (existiedBundle)
            {
                if(designAutomation.UpdateExistingAppBundle(appId, "Autodesk.Revit+2022", "Count AppBundle based on Revit 2022 Update", "D:\\CountSampeApp.7z", alias))
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
            if (designAutomation.CreateNewActivity(nickname, appId, alias, "countActivity", "D:\\DeleteWalls.rvt"))
            {

            }

            ////end for task 5
        }
    }
    }
