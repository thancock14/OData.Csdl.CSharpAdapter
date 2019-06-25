using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace EdmModelConverter 
{
    public class Fetcher
    {
        public static async Task<string> GetAsStringAsyc()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync("https://services.odata.org/V4/(S(obtfotswyh3rcbkxpyijxkbq))/TripPinServiceRW/$metadata");
                return await result.Content.ReadAsStringAsync();
            }
        }
        public static async Task<XmlDocument> GetAsXmlDocumentAsync()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync("https://services.odata.org/V4/(S(obtfotswyh3rcbkxpyijxkbq))/TripPinServiceRW/$metadata");
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(result.Content.ReadAsStreamAsync().Result);
                return xmlDoc;
            }
        }
    }
}