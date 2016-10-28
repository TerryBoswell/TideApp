using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core
{
    public static class TideData
    {
        
        public enum Location
        {
            BowleysBar = 1112367,
            RockyPoint = 1112369,
            OCPier = 8570280,
            OCInlet = 8570283
        }

        public static Location GetLocationId(string location)
        {
            switch (location)
            {
                case "Bowleys Bar":
                    return Location.BowleysBar;
                case "Rocky Point":
                    return Location.RockyPoint;
                case "Ocean City Fishing Pier":
                    return Location.OCPier;
                case "Ocean City Inlet":
                    return Location.OCInlet;
                default:
                    throw new ApplicationException("Unknown location");                
            }
        }

        public static List<DailyData> Data;
        public static async Task<DailyData> LoadTideData(DateTime today, Location location)
        {
            var cachedData = FileCache.GetDailyData(today, location);
            if (cachedData != null)
                return cachedData;

            List<DailyData> data = new List<DailyData>();

            using (var client = new System.Net.WebClient())
            {
                var startDate = today;
                var values = new System.Collections.Specialized.NameValueCollection();
                values["site"] = String.Format("Maryland&station_number={0}", ((Int32)location));
                values["month"] = String.Format("{0}&year={1}", today.Month, today.Year);
                values["start_date"] = startDate.Day.ToString();
                values["maximum_days"] = "14";
                // get response
                var response = client.UploadValues("http://www.saltwatertides.com/cgi-local/maryland.cgi", values);
                HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(Encoding.Default.GetString(response));
                var root = html.DocumentNode;
                var content = root.Descendants("PRE");
                System.IO.StringReader reader = new StringReader(content.First().InnerText);

                
                var line = string.Empty;

                while (line != null)
                {
                    line = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        continue;
                    if (line.StartsWith("Day"))
                        continue;
                    if (line.Contains("Sunset"))
                        continue;

                    int day = 0;
                    if (!int.TryParse(line.Substring(4, 2), out day))
                        throw new ApplicationException("Cannot parse day");

                    DailyData.TideType type = DailyData.TideType.Low;
                    if (!Enum.TryParse<DailyData.TideType>(line.Substring(11, 4), out type))
                        throw new ApplicationException("Cannot parse tide type");
                    DateTime timeOfTide = new DateTime();
                    if (!DateTime.TryParse(line.Substring(17, 8), out timeOfTide))
                        throw new ApplicationException("Cannot parse time of tide");

                    DateTime? timeOfSun = null;
                    if (line.Length > 42)
                    {
                        DateTime sun;
                        if (!DateTime.TryParse(line.Substring(35, 8), out sun))
                            throw new ApplicationException("Cannot parse time of sun");
                        timeOfSun = sun;
                    }
                    DailyData dailyData;
                    //First Record
                    if (!data.Any(x => x.DayOfMonth == day))
                        data.Add(new DailyData(day, startDate));

                    dailyData = data.First(x => x.DayOfMonth == day);

                    if (type == DailyData.TideType.Low)
                    {
                        if (dailyData.FirstLowTide.HasValue)
                            dailyData.SecondLowTide = timeOfTide;
                        else
                            dailyData.FirstLowTide = timeOfTide;
                    }
                    else
                    {
                        if (dailyData.FirstHighTide.HasValue)
                            dailyData.SecondHighTide = timeOfTide;
                        else
                            dailyData.FirstHighTide = timeOfTide;
                    }

                    if (timeOfSun.HasValue)
                    {
                        if (!dailyData.SunRise.HasValue)
                            dailyData.SunRise = timeOfSun;
                        else
                            dailyData.SunSet = timeOfSun;
                    }


                }

                Data = data; 
            }
            if (data.Any())
                foreach (var d in data)
                    FileCache.StoreDailyData(d, d.Date, location);
            return data.First();
        }
    }


    public class DailyData
    {
        public enum TideType
        {
            Low,
            High
        }

        public List<DateTime?> Times = new List<DateTime?>();
        public readonly int DayOfMonth;
        public readonly DateTime Date;

        private DateTime? firstHighTide;
        public DateTime? FirstHighTide
        {
            get
            {
                return firstHighTide;
            }
            set
            {
                firstHighTide = setDate(value);
                Times.Add(firstHighTide);
            }
        }

        private DateTime? setDate(DateTime? date)
        {
            if (!date.HasValue)
                return date;
            var v = date.Value;
            if (v.Date != Date.Date)
            {
                var timeDiff = v.Subtract(v.Date);
                date = Date.Add(timeDiff);
            }
            return date;
        }

        private DateTime? firstLowTide;
        public DateTime? FirstLowTide
        {
            get { return firstLowTide; }
            set {
                firstLowTide = setDate(value);
                Times.Add(firstLowTide);
            }
        }
        private DateTime? secondHighTide;
        public DateTime? SecondHighTide
        {
            get { return secondHighTide; }
            set {
                secondHighTide = setDate(value);
                Times.Add(secondHighTide);
            }
        }
        private DateTime? secondLowTide;
        public DateTime? SecondLowTide
        {
            get { return secondLowTide; }
            set {
                secondLowTide = setDate(value);
                Times.Add(secondLowTide);
            }
        }

        private DateTime? sunRise;
        public DateTime? SunRise
        {
            get { return sunRise; }
            set { sunRise = setDate(value); }
        }

        private DateTime? sunSet;
        public DateTime? SunSet
        {
            get { return sunSet; }
            set { sunSet = setDate(value); }
        }

        public DailyData(int dayOfMonth, DateTime startDate)
        {
            DayOfMonth = dayOfMonth;
            var dayDiff = dayOfMonth - startDate.Day;
            Date = startDate.Date.AddDays(dayDiff);
            if (dayDiff < 0)
                Date = Date.AddMonths(1);
        }
    }
}