using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Core.TideData;

namespace Core
{
    public static class TideAppDb
    {
        
        private static string getDocId(DateTime today, Location location)
        {
            return $"{today.ToShortDateString()}-{location}";
        }

        public static void StoreDailyData(DailyData data, DateTime today, Location location)
        {
            
            //var doc = DB.CreateDocument();
            //string docId = getDocId(today, location);
            //var props = new Dictionary<string, object>();
            //var serializedData = JsonConvert.SerializeObject(data);
            //props.Add("data", serializedData);
            //doc.PutProperties(props);
        }


        public static DailyData GetDailyData(DateTime today, Location location)
        {
            return null;
            //string docId = getDocId(today, location);

            
            //var doc = DB.GetExistingDocument(docId);
            //if (doc == null)
            //    return null;

            //var serializedData = doc.Properties["data"];
            //if (serializedData == null)
            //    return null;
            //return JsonConvert.DeserializeObject<DailyData>(serializedData.ToString());
        }

    }
}