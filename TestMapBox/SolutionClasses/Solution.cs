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
        private double Cost;

        //Tabu Search bariables
        double BestSolutionCost;
        public Vehicle[] vehiclesForBestSolution;
        public List<double> pastSolutions = new List<double>();

        public Solution(int customerNum, int vehicleNum, int vehicleCap)
        {
            NoOfCustomers = customerNum;
            NoOfVehicles = vehicleNum;
            Cost = 0;
            Vehicles = new Vehicle[NoOfVehicles];

            vehiclesForBestSolution = new Vehicle[NoOfVehicles];

            InitVehicles(vehicleCap);
        }

        private void InitVehicles(int vehicleCap)
        {
            for (var i = 0; i < NoOfVehicles; i++)
            {
                Vehicles[i] = new Vehicle(i + 1, vehicleCap);
                vehiclesForBestSolution[i] = new Vehicle(i + 1, vehicleCap);
            }
        }

        public bool CheckCustomers(Customer[] customers)
        {
            for (var i = 0; i < customers.Length; i++)
                if (!customers[i].IsRouted)
                    return true;

            return false;
        }

        public Vehicle[] GetVehicles()
        {
            return Vehicles;
        }

        public double GetCost()
        {
            return Cost;
        }

        public void GreedySolution(Customer[] customers, double[,] distanceMatrix)
        {
            double endCost;
            var vehicleIndex = 0;

            while (CheckCustomers(customers))
            {
                var customersIndex = 0;
                Customer candidate = null;
                var minCost = double.MaxValue;

                if (Vehicles[vehicleIndex].Route.Count == 0) Vehicles[vehicleIndex].AddNode(customers[0]);

                for (var i = 0; i < NoOfCustomers; i++)
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
        }

        public void TabuSearch(int TABU_Horizon, double[,] distanceMatrix)
        {
            List<Customer> RouteFrom;
            List<Customer> RouteTo;

            int MovingNodeDemand = 0;

            int VehIndexFrom, VehIndexTo;
            double BestNCost, NeightborCost;

            int SwapIndexA = -1, SwapIndexB = -1, SwapRouteFrom = -1, SwapRouteTo = -1;

            int MAX_ITERATIONS = 400;
            int iteration_number = 0;

            int dimensionCustomer = distanceMatrix.GetLength(0);
            int[,] tabuMatrix = new int[dimensionCustomer + 1, dimensionCustomer + 1];
            BestSolutionCost = this.Cost;

            while (true)
            {
                iteration_number++;
                BestNCost = double.MaxValue;

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
                                    if (!((VehIndexFrom == VehIndexTo) && ((j == i) || (j == i - 1))))
                                    {
                                        double minusCost1 = distanceMatrix[RouteFrom[i - 1].CustomerId,
                                            RouteFrom[i].CustomerId];
                                        double minusCost2 = distanceMatrix[RouteFrom[i].CustomerId,
                                            RouteFrom[i + 1].CustomerId];
                                        double minusCost3 = distanceMatrix[RouteTo[j].CustomerId,
                                            RouteTo[j + 1].CustomerId];

                                        double addedCost1 = distanceMatrix[RouteFrom[i - 1].CustomerId,
                                            RouteFrom[i + 1].CustomerId];
                                        double addedCost2 = distanceMatrix[RouteTo[j].CustomerId,
                                            RouteFrom[i].CustomerId];
                                        double addedCost3 = distanceMatrix[RouteFrom[i].CustomerId,
                                            RouteTo[j + 1].CustomerId];

                                        if ((tabuMatrix[RouteFrom[i - 1].CustomerId, RouteFrom[i + 1].CustomerId] != 0)
                                            || (tabuMatrix[RouteTo[j].CustomerId, RouteFrom[i].CustomerId] != 0)
                                            || (tabuMatrix[RouteFrom[i].CustomerId, RouteTo[j + 1].CustomerId] != 0))
                                            break;

                                        NeightborCost = addedCost1 + addedCost2 + addedCost3
                                                        - minusCost1 - minusCost2 - minusCost3;

                                        if (!(NeightborCost < BestNCost)) continue;
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

                for (int o = 0; o < tabuMatrix.GetLength(1); o++)
                {
                    for (int p = 0; p < tabuMatrix.GetLength(1); p++)
                    {
                        if (tabuMatrix[o,p] > 0)
                        { tabuMatrix[o, p]--; }
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
                    if (SwapIndexA < SwapIndexB) RouteTo.Insert(SwapIndexB, swapNode); // [SwapIndexB] = swapNode;
                    else RouteTo.Insert(SwapIndexB + 1, swapNode); //RouteTo[SwapIndexB + 1] = swapNode;
                }
                else
                {
                    RouteTo.Insert(SwapIndexB + 1, swapNode); //RouteTo[SwapIndexB] = swapNode;
                }

                this.Vehicles[SwapRouteFrom].Route = RouteFrom;
                this.Vehicles[SwapRouteFrom].Load -= MovingNodeDemand;

                this.Vehicles[SwapRouteTo].Route = RouteTo;
                this.Vehicles[SwapRouteTo].Load += MovingNodeDemand;

                pastSolutions.Add(this.Cost);
                this.Cost += BestNCost;
                if(this.Cost < BestSolutionCost) SaveBestSolution();
                if (iteration_number == MAX_ITERATIONS) break;
            }

            this.Vehicles = vehiclesForBestSolution;
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
                        text.Append(Vehicles[j].Route[k].CustomerId);
                    else
                        text.Append(Vehicles[j].Route[k].CustomerId + "->");
                text.Append('\n');
                }
            }

            text.Append("\n\n\nSolution Cost " + Cost + "\n");
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Results.txt")))
            {
                outputFile.Write(text);
            }

            File.Open(Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), "Results.txt"), FileMode.Open);
        }
    }
}
