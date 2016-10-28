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
    public static class FileCache
    {

        private static string getDocId(DateTime today, Location location)
        {
            return $"{today.Year}_{today.Month}_{today.Day}_{location}.txt".Replace(' ', '_');
        }

        public static void StoreDailyData(DailyData data, DateTime today, Location location)
        {
            if (GetDailyData(today, location) != null)
                return;
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string filename = Path.Combine(path, getDocId(today, location));

                using (var streamWriter = new StreamWriter(filename, true))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(data));
                }
            }
            catch (Exception ex)
            {
                
            }
            
        }


        public static DailyData GetDailyData(DateTime today, Location location)
        {
            DailyData data = null;
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string filename = Path.Combine(path, getDocId(today, location));

                using (var streamReader = new StreamReader(filename))
                {
                    string content = streamReader.ReadToEnd();
                    data = JsonConvert.DeserializeObject<DailyData>(content);
                }
            }
            catch { }
            return data;
        }
    }
}