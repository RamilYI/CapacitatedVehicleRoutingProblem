using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TestMapBox.HelperClasses;

namespace TestMapBox.SolutionClasses
{
    internal class Solution
    {
        private int NoOfVehicles, NoOfCustomers;
        private Vehicle[] Vehicles;
        private decimal Cost;

        private decimal greedySolCost;
        //Tabu Search variables
        decimal BestSolutionCost;
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
        }

        public void TabuSearch(decimal[,] distanceMatrix)
        {
            List<Customer> RouteFrom;
            List<Customer> RouteTo;

            int MovingNodeDemand = 0;

            int VehIndexFrom, VehIndexTo;
            decimal BestNCost, NeightborCost;

            int SwapIndexA = -1, SwapIndexB = -1, SwapRouteFrom = -1, SwapRouteTo = -1;

            int MAX_ITERATIONS = 400;
            int iteration_number = 0;
            bool neightboor = false;
            int dimensionCustomer = distanceMatrix.GetLength(0);
            int TABU_Horizon = NoOfCustomers;
            int[,] tabuMatrix = new int[dimensionCustomer + 1, dimensionCustomer + 1];
            BestSolutionCost = this.Cost;

            while (true)
            {
                iteration_number++;
                BestNCost = decimal.MaxValue;

                for (VehIndexFrom = 0; VehIndexFrom < this.Vehicles.Length; VehIndexFrom++)
                {
                    RouteFrom = this.Vehicles[VehIndexFrom].Route;
                    int routeFromLength = RouteFrom.Count;

                    for (int i = 1; i < routeFromLength - 1; i++)
                    {
                        for (VehIndexTo = 0; VehIndexTo < this.Vehicles.Length; VehIndexTo++)
                        {
                            RouteTo = this.Vehicles[VehIndexTo].Route;
                            int routeToLength = RouteTo.Count;
                            for (int j = 0; j < routeToLength - 1; j++)
                            {
                                MovingNodeDemand = RouteFrom[i].Demand;

                                if ((VehIndexFrom == VehIndexTo) ||
                                    this.Vehicles[VehIndexTo].CheckIfFits(MovingNodeDemand))
                                {
                                    if (((VehIndexFrom == VehIndexTo) && ((j == i) || (j == i - 1))) == false)
                                    {
                                        if ((tabuMatrix[RouteFrom[i - 1].CustomerId, RouteFrom[i + 1].CustomerId] != 0)
                                            || (tabuMatrix[RouteTo[j].CustomerId, RouteFrom[i].CustomerId] != 0)
                                            || (tabuMatrix[RouteFrom[i].CustomerId, RouteTo[j + 1].CustomerId] != 0))
                                            break;

                                        decimal minusCost1 = distanceMatrix[RouteFrom[i - 1].CustomerId,
                                            RouteFrom[i].CustomerId];
                                        decimal minusCost2 = distanceMatrix[RouteFrom[i].CustomerId,
                                            RouteFrom[i + 1].CustomerId];
                                        decimal minusCost3 = distanceMatrix[RouteTo[j].CustomerId,
                                            RouteTo[j + 1].CustomerId];

                                        decimal addedCost1 = distanceMatrix[RouteFrom[i - 1].CustomerId,
                                            RouteFrom[i + 1].CustomerId];
                                        decimal addedCost2 = distanceMatrix[RouteTo[j].CustomerId,
                                            RouteFrom[i].CustomerId];
                                        decimal addedCost3 = distanceMatrix[RouteFrom[i].CustomerId,
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
                    }
                }
                if (BestNCost == decimal.MaxValue && iteration_number == MAX_ITERATIONS) break;
                if (BestNCost == decimal.MaxValue) continue;


                for (int o = 0; o < tabuMatrix.GetLength(1); o++)
                    {
                        for (int p = 0; p < tabuMatrix.GetLength(1); p++)
                        {
                            if (tabuMatrix[o, p] > 0)
                            {
                                tabuMatrix[o, p]--;
                            }
                        }
                    }

                    RouteFrom = this.Vehicles[SwapRouteFrom].Route;
                    RouteTo = this.Vehicles[SwapRouteTo].Route;
                    this.Vehicles[SwapRouteTo].Route = null;
                    this.Vehicles[SwapRouteFrom].Route = null;

                    Customer swapNode = RouteFrom[SwapIndexA];
                    int nodeIdBefore = RouteFrom[SwapIndexA - 1].CustomerId;
                    int nodeIdAfter = RouteFrom[SwapIndexA + 1].CustomerId;
                    int nodeId_F = RouteTo[SwapIndexB].CustomerId;
                    int nodeId_G = RouteTo[SwapIndexB + 1].CustomerId;

                    Random tabuRandom = new Random();

                    int randomDelay1 = tabuRandom.Next(5);
                    int randomDelay2 = tabuRandom.Next(5);
                    int randomDelay3 = tabuRandom.Next(5);

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

                    this.Vehicles[SwapRouteFrom].Route = RouteFrom;
                    this.Vehicles[SwapRouteFrom].Load -= MovingNodeDemand;

                    this.Vehicles[SwapRouteTo].Route = RouteTo;
                    this.Vehicles[SwapRouteTo].Load += MovingNodeDemand;

                    pastSolutions.Add(this.Cost);
                    this.Cost += BestNCost;
                    if (this.Cost < BestSolutionCost)
                    {
                    neightboor = true;
                    SaveBestSolution();
                    }
                

                if (iteration_number == MAX_ITERATIONS) break;
            }
            if (neightboor) this.Vehicles = vehiclesForBestSolution;
            this.Cost = BestSolutionCost;
        }

        public void SaveBestSolution()
        {
            BestSolutionCost = Cost;

            for (int i = 0; i < NoOfVehicles; i++)
            {
                vehiclesForBestSolution[i].Route.Clear();
                if (Vehicles[i].Route.Count != 0)
                {
                    int RouteSize = Vehicles[i].Route.Count;

                    for (int k = 0; k < RouteSize; k++)
                    {
                        Customer n = Vehicles[i].Route[k];
                        vehiclesForBestSolution[i].Route.Add(n);
                    }
                }
            }
        }

        public void PrintSolution()
        {
            StringBuilder text = new StringBuilder();
            for (var j = 0; j < NoOfVehicles; j++)
            {
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
            }

            text.Append("\n\n\nTabuSearch Cost " + Cost + "\n");
            text.Append("\n\n\nGreedy Cost " + greedySolCost + "\n");
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Results.txt")))
            {
                outputFile.Write(text);
            }

            var results = File.Open(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Results.txt"), FileMode.Open);
            results.Close();
        }
    }
}
