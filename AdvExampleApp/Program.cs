using Newtonsoft.Json.Linq;
using System.Net;
using AdvLibrary.ForgeApi;
using AdvLibrary.ForgeApi.Model;

class AutodeskForge {
    

    // Entry point
    static void Main(string[] args)
    {
        // Get the client id and client secret from 3rd party
        string client_id = "z9AnxOhryxcTSTzyA2oRRJCaiYGIMr6g";
        string client_secret = "Lq96viASKfBHq83e";

        // Create an instance of the class
        ForgeApi forge = new(client_id, client_secret);

        var hubs = new List<AutoHub>();
        var projects = new List<AutoProject>();
        var topFolders = new List<AutoFolder>();

        //Here I will get a list of hubs for the credentials
        hubs = new List<AutoHub>(forge.GetHubsAsync().Result);

        //Here I will select myHub based on some properties
        AutoHub myHub = hubs.Where(x => x.HubName == "ADV").First();

        //Here I will pass myHub to get a list of all projects in the Hub
        projects = new List<AutoProject>(forge.GetProjectsAsync(myHub.HubId);





    }
}