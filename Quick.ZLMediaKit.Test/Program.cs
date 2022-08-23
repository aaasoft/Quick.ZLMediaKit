using Newtonsoft.Json;
using Quick.ZLMediaKit.HttpApi;

var options = new ZLMediaKitClientOptions()
{
    ApiUrl = "http://192.168.31.126/index/api",
    ApiSecret = "035c73f7-bb6b-4889-a715-d9eb2d1925cc"
};

var client = new ZLMediaKitClient(options);
var serverConfig=await client.GetServerConfig();
Console.WriteLine(JsonConvert.SerializeObject(serverConfig));