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
            //  designAutomation.CreateNickName("pdragon0512");
            /////


            ///////task 4

            var existiedBundle = false;
            var appId = "DeleteWallsApp15";
            if (designAutomation.RegisterAppBundle(appId, "Autodesk.Revit+2018", "Delete Walls AppBundle based on Revit 2018"))
            {
                if (designAutomation.UploadAppBundle("C:\\Users\\SteerC\\Music\\delete\\DeleteWallsApp.zip"))
                {
                    Console.WriteLine("uploaded");
                }
                else { Console.WriteLine("failed"); }

                if (existiedBundle = designAutomation.CreateAliasForAppBundle(appId,"test1"))
                {
                    Console.WriteLine("alias created");
                }
                else { Console.WriteLine("alias create failed"); }
            }

            //*update part*//
            if (existiedBundle)
            {
                if(designAutomation.UpdateExistingAppBundle(appId, "Autodesk.Revit+2018", "Delete Walls AppBundle based on Revit 2018 Update", "C:\\Users\\SteerC\\Music\\delete\\DeleteWallsApp.zip", "test1"))
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

        }
    }
    }
