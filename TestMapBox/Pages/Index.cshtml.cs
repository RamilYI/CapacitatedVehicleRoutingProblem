using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TestMapBox.HelperClasses;
using TestMapBox.Models;
using TestMapBox.SolutionClasses;

namespace TestMapBox.Pages
{
    public class IndexModel : PageModel
    {
        public static List<PrimaryData> PrimaryDatas = new List<PrimaryData>();
        private readonly IHostingEnvironment _hostingEnvironment;
        public string MapboxAccessToken { get; }

        public IndexModel(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            MapboxAccessToken = configuration["Mapbox:AccessToken"];
        }
        [BindProperty]
        public PrimaryData PrimaryData { get; set; }
        public Coordinate Coordinate { get; set; }
        public IActionResult OnGetAirports()
        {
            var configuration = new Configuration
            {
                BadDataFound = context => { }
            };

            using (var sr = new StreamReader(Path.Combine(_hostingEnvironment.WebRootPath, "airports.dat")))
                using (var reader = new CsvReader(sr, configuration))
                {
                    FeatureCollection featureCollection = new FeatureCollection();
                    while (reader.Read())
                    {
                        var buf = reader.GetField(0).Replace(", ", " ").Split(",", StringSplitOptions.None);
                        string name = buf[1];
                        string iataCode = buf[4];
                        double latitude = Convert.ToDouble(buf[6], CultureInfo.InvariantCulture);
                        double longitude = Convert.ToDouble(buf[7], CultureInfo.InvariantCulture);
                        
                        featureCollection.Features.Add(new Feature(
                            new Point(new Position(latitude, longitude)),
                            new Dictionary<string,object>
                            {
                                {"name", name },
                                {"iataCode", iataCode }
                            }));
                    }

                    return new JsonResult(featureCollection);
                }
        }


        public JsonResult Add(List<Coordinate> coord)
        {
            Coordinate = coord[0];
            return JsonResult(Coordinate);
        }

        private JsonResult JsonResult(Coordinate coord)
        {
            throw new NotImplementedException();
        }

        public IActionResult OnPostPrimaryData()
        {
            var coordinates = new List<Coordinate>();
            Thread.Sleep(10000); //костыль
            JsonProcessing(coordinates);
            Customer[] customers = new Customer[coordinates.Count];
            Random random = new Random();
            CreateCustomers(coordinates, customers, random);

            var s = new Solution(coordinates.Count, PrimaryData.NumberVehicles, PrimaryData.MaxVehicle);
            var distanceMatrix = new double[coordinates.Count,coordinates.Count];
            CalcDistancies(coordinates, distanceMatrix);
            s.GreedySolution(customers, distanceMatrix);
            s.PrintSolution();
            //s.TabuSearch(10, distanceMatrix);
            
            return RedirectToPage("");
        }

        private void CalcDistancies(List<Coordinate> coordinates, double[,] distanceMatrix)
        {
            double R = 6371e3;
            for (int i = 0; i < coordinates.Count; i++)
            {
                for (int j = i + 1; j < coordinates.Count; j++)
                {
                    var deltaF = toRadian(coordinates[i].lat - coordinates[j].lat);
                    var deltaL = toRadian(coordinates[i].lng - coordinates[j].lng);
                    var a = Math.Pow(Math.Sin(deltaF / 2), 2) +
                            Math.Cos(coordinates[j].lat) * Math.Cos(coordinates[i].lat)
                                                         * Math.Pow(Math.Sin(deltaL / 2), 2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    distanceMatrix[i, j] = distanceMatrix[j, i] = R * c;
                }
            }
        }

        private void CreateCustomers(List<Coordinate> coordinates, Customer[] customers, Random random)
        {
            for (int i = 0; i < coordinates.Count; i++)
            {
                customers[i] = new Customer(i, coordinates[i].lng, coordinates[i].lat,
                    random.Next(1, PrimaryData.NumberDemand));
            }
        }

        private static void JsonProcessing(List<Coordinate> coordinates)
        {
            using (StreamReader r = new StreamReader(KnownFolders.GetPath(KnownFolder.Downloads) + "\\data.json"))
            {
                List<string> jsonBuf = new List<string>();
                var json = r.ReadToEnd();
                var jsonCnt = 0;
                Regex regex = new Regex("\"[0-9]");
                Match match = regex.Match(json);
                while (match.Success)
                {
                    jsonCnt++;
                    match = match.NextMatch();
                }

                for (int i = 1; i <= jsonCnt; i++)
                {
                    var buf = "\"" + $"{i}" + "\"" + ":";
                    json = json.Replace(buf, ";");
                }

                jsonBuf.AddRange(json.Substring(1, json.Length - 2).Split(",;"));
                jsonBuf[0] = jsonBuf[0].Substring(1);
                for (var i = 0; i < jsonBuf.Count; i++) coordinates.Add(JsonConvert.DeserializeObject<Coordinate>(jsonBuf[i]));
            }
        }

        private double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }
    }
}
