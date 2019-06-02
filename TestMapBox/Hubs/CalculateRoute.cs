using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TestMapBox.HelperClasses;
using TestMapBox.Models;
using TestMapBox.SolutionClasses;

namespace TestMapBox.Hubs
{
    public class CalculateRoute : Hub
    {
        public async Task SendMessage(string vehicle, string maxDemand, string jsonObj,
            string distanceSend)
        {
            List<int[]> demandStorage;
            var coordinates = new List<Coordinate>();
            JsonProcessing(coordinates, jsonObj);
            var demandArr = JsonConvert.DeserializeObject<string[]>(maxDemand);
            try
            {
                CreateDemandArrays(demandArr, out demandStorage);
            }
            catch (Exception)
            {
                await Clients.All.SendAsync("ReceiveMessage","has no demand", 0);
                return;
            }
            var customers = new Customer[coordinates.Count];
            var random = new Random();
            CreateCustomers(coordinates, customers, random, demandStorage);
            var vehicleArr = JsonConvert.DeserializeObject<int[]>(vehicle);
            var s = new Solution(coordinates.Count, vehicleArr.Length, vehicleArr);
            var distanceMatrix = new decimal[coordinates.Count, coordinates.Count];
            CalcDistancies(coordinates, distanceMatrix, distanceSend);

            try
            {
                s.GreedySolution(customers, distanceMatrix);
            }
            catch (Exception)
            {
                await Clients.All.SendAsync("ReceiveMessage", "not solved", 0);
                return;
            }

            s.TabuSearch(distanceMatrix);
            s.PrintSolution();
            var coordinatearrays = new List<List<decimal>>();
            var vehicles = s.GetVehicles();
            CreateResultArray(vehicles, coordinates, coordinatearrays);
            var cost = s.GetCost();
            var resultCoords = JsonConvert.SerializeObject(coordinatearrays);
            await Clients.All.SendAsync("ReceiveMessage", resultCoords,
                Math.Round(cost, 2).ToString(CultureInfo.InvariantCulture));
        }

        private static void CreateDemandArrays(string[] demands, out List<int[]> demandStorage)
        {
            demandStorage = new List<int[]>();
            for (int i = 0; i < demands.Length; i++)
            {
                demandStorage.Add(demands[i].Split(",").Select(int.Parse).ToArray());
            }
        }

        private static void CreateResultArray(Vehicle[] vehicles, List<Coordinate> coordinates,
            List<List<decimal>> coordinatearrays)
        {
            for (var j = 0; j < vehicles.Length; j++)
                if (vehicles[j].Route.Count != 0)
                {
                    var routeSize = vehicles[j].Route.Count;
                    var arraybuf = new List<decimal>();
                    for (var k = 0; k < routeSize; k++)
                    {
                        arraybuf.Add(coordinates[vehicles[j].Route[k].CustomerId].lng);
                        arraybuf.Add(coordinates[vehicles[j].Route[k].CustomerId].lat);
                    }

                    coordinatearrays.Add(arraybuf);
                }
        }

        private void CalcDistancies(List<Coordinate> coordinates, decimal[,] distanceMatrix, string distanceSend)
        {
            var mapboxDistancies = distanceSend.Split(',').Select(x => decimal.Parse(x, CultureInfo.InvariantCulture))
                .ToArray();
            int v;
            for (var i = 0; i < coordinates.Count; i++)
            for (var j = 0; j < coordinates.Count; j++)
            {
                v = i * coordinates.Count + j;
                distanceMatrix[i, j] = mapboxDistancies[v];
            }
        }

        private void CreateCustomers(List<Coordinate> coordinates, Customer[] customers, Random random, List<int[]> demand)
        {
            customers[0] = new Customer(coordinates[0].lng, coordinates[0].lat);

            for (var i = 1; i < coordinates.Count; i++)
                customers[i] = new Customer(i, coordinates[i].lng, coordinates[i].lat,
                    ForecastingDemand.DoubleExponentialSmoothing(demand[i - 1], 0.9,
                        0.9));
        }

        private static void JsonProcessing(List<Coordinate> coordinates, string json)
        {
            var jsonBuf = new List<string>();
            var jsonCnt = 0;
            var regex = new Regex("\"[0-9]");
            var match = regex.Match(json);
            while (match.Success)
            {
                jsonCnt++;
                match = match.NextMatch();
            }

            for (var i = 1; i <= jsonCnt; i++)
            {
                var buf = "\"" + $"{i}" + "\"" + ":";
                json = json.Replace(buf, ";");
            }

            jsonBuf.AddRange(json.Substring(1, json.Length - 2).Split(",;"));
            jsonBuf[0] = jsonBuf[0].Substring(4);
            for (var i = 0; i < jsonBuf.Count; i++)
                coordinates.Add(JsonConvert.DeserializeObject<Coordinate>(jsonBuf[i]));
        }
    }
}