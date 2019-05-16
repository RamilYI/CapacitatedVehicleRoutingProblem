using System.Collections.Generic;

namespace TestMapBox.SolutionClasses
{
    internal class Vehicle
    {
        public int VehicleId, Load, Capacity, CurrentLocation;
        public List<Customer> Route = new List<Customer>();

        public Vehicle(int id, int capacity)
        {
            VehicleId = id;
            Capacity = capacity;
            Load = 0;
            CurrentLocation = 0;
            Route.Clear();
        }

        public void AddNode(Customer customer)
        {
            Route.Add(customer);
            Load += customer.Demand;
            CurrentLocation = customer.CustomerId;
        }

        public bool CheckIfFits(int demand)
        {
            return Load + demand <= Capacity;
        }
    }
}
