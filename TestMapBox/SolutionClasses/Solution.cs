using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TestMapBox.HelperClasses;

namespace TestMapBox.SolutionClasses
{
    internal class Solution
    {
        private readonly int NoOfVehicles;
        private readonly int NoOfCustomers;
        private Vehicle[] Vehicles;
        private decimal Cost;
        private DateTime? start { get; set; }
        private DateTime? end { get; set; }

        private TimeSpan? greedyTime;

        private TimeSpan? tabuTime;

        private decimal greedySolCost;

        //Tabu Search variables
        private decimal BestSolutionCost;
        public Vehicle[] vehiclesForBestSolution;
        public List<decimal> pastSolutions = new List<decimal>();

        public Solution(int customerNum, int vehicleNum, int[] vehicles)
        {
            NoOfCustomers = customerNum;
            NoOfVehicles = vehicleNum;
            Cost = 0;
            Vehicles = new Vehicle[NoOfVehicles];

            vehiclesForBestSolution = new Vehicle[NoOfVehicles];

            InitVehicles(vehicles);
        }

        private void InitVehicles(int[] vehicles)
        {
            for (var i = 0; i < NoOfVehicles; i++)
            {
                Vehicles[i] = new Vehicle(i + 1, vehicles[i]);
                vehiclesForBestSolution[i] = new Vehicle(i + 1, vehicles[i]);
            }
        }

        public bool CheckCustomers(Customer[] customers)
        {
            for (var i = 1; i < customers.Length; i++)
                if (!customers[i].IsRouted)
                    return true;

            return false;
        }

        public Vehicle[] GetVehicles()
        {
            return Vehicles;
        }

        public decimal GetCost()
        {
            return Cost;
        }

        public void GreedySolution(Customer[] customers, decimal[,] distanceMatrix)
        {
            start = DateTime.Now;
            decimal endCost;
            var vehicleIndex = 0;

            while (CheckCustomers(customers))
            {
                var customersIndex = 0;
                Customer candidate = null;
                var minCost = decimal.MaxValue;

                if (Vehicles[vehicleIndex].Route.Count == 0) Vehicles[vehicleIndex].AddNode(customers[0]);

                for (var i = 1; i < NoOfCustomers; i++)
                    if (customers[i].IsRouted == false)
                        if (Vehicles[vehicleIndex].CheckIfFits(customers[i].Demand))
                        {
                            var candCost = distanceMatrix[Vehicles[vehicleIndex].CurrentLocation, i];
                            if (minCost > candCost)
                            {
                                minCost = candCost;
                                customersIndex = i;
                                candidate = customers[i];
                            }
                        }

                if (candidate == null)
                {
                    if (vehicleIndex < Vehicles.Length)
                    {
                        if (Vehicles[vehicleIndex].CurrentLocation != 0)
                        {
                            endCost = distanceMatrix[Vehicles[vehicleIndex].CurrentLocation, 0];
                            Vehicles[vehicleIndex].AddNode(customers[0]);
                            Cost += endCost;
                        }

                        vehicleIndex++;
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Vehicles[vehicleIndex].AddNode(candidate);
                    customers[customersIndex].IsRouted = true;
                    Cost += minCost;
                }
            }

            endCost = distanceMatrix[Vehicles[vehicleIndex].CurrentLocation, 0];
            Vehicles[vehicleIndex].AddNode(customers[0]);
            Cost += endCost;
            greedySolCost = Cost;
            greedyTime =  DateTime.Now - start;
        }

        public void TabuSearch(decimal[,] distanceMatrix)
        {
            start = DateTime.Now;
            List<Customer> RouteFrom;
            List<Customer> RouteTo;

            var MovingNodeDemand = 0;

            int VehIndexFrom, VehIndexTo;
            decimal BestNCost, NeightborCost;

            int SwapIndexA = -1, SwapIndexB = -1, SwapRouteFrom = -1, SwapRouteTo = -1;

            var MAX_ITERATIONS = 400;
            var iteration_number = 0;
            var neightboor = false;
            var dimensionCustomer = distanceMatrix.GetLength(0);
            var TABU_Horizon = NoOfCustomers;
            var tabuMatrix = new int[dimensionCustomer + 1, dimensionCustomer + 1];
            BestSolutionCost = Cost;

            while (true)
            {
                iteration_number++;
                BestNCost = decimal.MaxValue;

                for (VehIndexFrom = 0; VehIndexFrom < Vehicles.Length; VehIndexFrom++)
                {
                    RouteFrom = Vehicles[VehIndexFrom].Route;
                    var routeFromLength = RouteFrom.Count;

                    for (var i = 1; i < routeFromLength - 1; i++)
                    for (VehIndexTo = 0; VehIndexTo < Vehicles.Length; VehIndexTo++)
                    {
                        RouteTo = Vehicles[VehIndexTo].Route;
                        var routeToLength = RouteTo.Count;
                        for (var j = 0; j < routeToLength - 1; j++)
                        {
                            MovingNodeDemand = RouteFrom[i].Demand;

                            if (VehIndexFrom == VehIndexTo ||
                                Vehicles[VehIndexTo].CheckIfFits(MovingNodeDemand))
                                if ((VehIndexFrom == VehIndexTo && (j == i || j == i - 1)) == false)
                                {
                                    if (tabuMatrix[RouteFrom[i - 1].CustomerId, RouteFrom[i + 1].CustomerId] != 0
                                        || tabuMatrix[RouteTo[j].CustomerId, RouteFrom[i].CustomerId] != 0
                                        || tabuMatrix[RouteFrom[i].CustomerId, RouteTo[j + 1].CustomerId] != 0)
                                        break;

                                    var minusCost1 = distanceMatrix[RouteFrom[i - 1].CustomerId,
                                        RouteFrom[i].CustomerId];
                                    var minusCost2 = distanceMatrix[RouteFrom[i].CustomerId,
                                        RouteFrom[i + 1].CustomerId];
                                    var minusCost3 = distanceMatrix[RouteTo[j].CustomerId,
                                        RouteTo[j + 1].CustomerId];

                                    var addedCost1 = distanceMatrix[RouteFrom[i - 1].CustomerId,
                                        RouteFrom[i + 1].CustomerId];
                                    var addedCost2 = distanceMatrix[RouteTo[j].CustomerId,
                                        RouteFrom[i].CustomerId];
                                    var addedCost3 = distanceMatrix[RouteFrom[i].CustomerId,
                                        RouteTo[j + 1].CustomerId];


                                    NeightborCost = addedCost1 + addedCost2 + addedCost3
                                                    - minusCost1 - minusCost2 - minusCost3;

                                    if (NeightborCost < BestNCost)
                                    {
                                        BestNCost = NeightborCost;
                                        SwapIndexA = i;
                                        SwapIndexB = j;
                                        SwapRouteFrom = VehIndexFrom;
                                        SwapRouteTo = VehIndexTo;
                                    }
                                }
                        }
                    }
                }

                if (BestNCost == decimal.MaxValue && iteration_number == MAX_ITERATIONS) break;
                if (BestNCost == decimal.MaxValue) continue;


                for (var o = 0; o < tabuMatrix.GetLength(1); o++)
                for (var p = 0; p < tabuMatrix.GetLength(1); p++)
                    if (tabuMatrix[o, p] > 0)
                        tabuMatrix[o, p]--;

                RouteFrom = Vehicles[SwapRouteFrom].Route;
                RouteTo = Vehicles[SwapRouteTo].Route;
                Vehicles[SwapRouteTo].Route = null;
                Vehicles[SwapRouteFrom].Route = null;

                var swapNode = RouteFrom[SwapIndexA];
                var nodeIdBefore = RouteFrom[SwapIndexA - 1].CustomerId;
                var nodeIdAfter = RouteFrom[SwapIndexA + 1].CustomerId;
                var nodeId_F = RouteTo[SwapIndexB].CustomerId;
                var nodeId_G = RouteTo[SwapIndexB + 1].CustomerId;

                var tabuRandom = new Random();

                var randomDelay1 = tabuRandom.Next(5);
                var randomDelay2 = tabuRandom.Next(5);
                var randomDelay3 = tabuRandom.Next(5);

                tabuMatrix[nodeIdBefore, swapNode.CustomerId] = TABU_Horizon + randomDelay1;
                tabuMatrix[swapNode.CustomerId, nodeIdAfter] = TABU_Horizon + randomDelay2;
                tabuMatrix[nodeId_F, nodeId_G] = TABU_Horizon + randomDelay3;

                RouteFrom.RemoveAt(SwapIndexA);

                if (SwapRouteFrom == SwapRouteTo)
                {
                    if (SwapIndexA < SwapIndexB) RouteTo.Insert(SwapIndexB, swapNode);
                    else RouteTo.Insert(SwapIndexB + 1, swapNode);
                }
                else
                {
                    RouteTo.Insert(SwapIndexB + 1, swapNode);
                }

                Vehicles[SwapRouteFrom].Route = RouteFrom;
                Vehicles[SwapRouteFrom].Load -= MovingNodeDemand;

                Vehicles[SwapRouteTo].Route = RouteTo;
                Vehicles[SwapRouteTo].Load += MovingNodeDemand;

                pastSolutions.Add(Cost);
                Cost += BestNCost;
                if (Cost < BestSolutionCost)
                {
                    neightboor = true;
                    SaveBestSolution();
                }


                if (iteration_number == MAX_ITERATIONS) break;
            }

            if (neightboor) Vehicles = vehiclesForBestSolution;
            Cost = BestSolutionCost;
            tabuTime =    DateTime.Now - start;
        }

        public void SaveBestSolution()
        {
            BestSolutionCost = Cost;

            for (var i = 0; i < NoOfVehicles; i++)
            {
                vehiclesForBestSolution[i].Route.Clear();
                if (Vehicles[i].Route.Count != 0)
                {
                    var RouteSize = Vehicles[i].Route.Count;

                    for (var k = 0; k < RouteSize; k++)
                    {
                        var n = Vehicles[i].Route[k];
                        vehiclesForBestSolution[i].Route.Add(n);
                    }
                }
            }
        }

        public void PrintSolution()
        {
            var text = new StringBuilder();
            for (var j = 0; j < NoOfVehicles; j++)
                if (Vehicles[j].Route.Count != 0)
                {
                    text.Append("Vehicle " + j + ":");
                    var routeSize = Vehicles[j].Route.Count;
                    for (var k = 0; k < routeSize; k++)
                        if (k == routeSize - 1)
                            text.Append("(" + Vehicles[j].Route[k].X + ";" + Vehicles[j].Route[k].Y + ")");
                        else
                            text.Append("(" + Vehicles[j].Route[k].X + ";" + Vehicles[j].Route[k].Y + ")" + "->");
                    text.Append('\n');
                }

            text.Append("\n\n\nTabuSearch Cost " + Cost + "\n");
            text.Append("tabuTime: " + tabuTime + "\n");
            text.Append("\n\n\nGreedy Cost " + greedySolCost + "\n");
            text.Append("greedy TIme: " + greedyTime + "\n");

            using (var outputFile =
                new StreamWriter(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Results.txt")))
            {
                outputFile.Write(text);
            }

            var results = File.Open(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Results.txt"),
                FileMode.Open);
            results.Close();
        }
    }
}