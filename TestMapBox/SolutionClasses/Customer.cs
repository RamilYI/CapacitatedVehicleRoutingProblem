namespace TestMapBox.SolutionClasses
{
    internal class Customer
    {
        public int CustomerId { get; set; }
        public int Demand { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsRouted;

        public Customer(double x, double y) //for depot
        {
            CustomerId = 0;
            X = x;
            Y = y;
        }

        public Customer(int id, double x, double y, int demand)
        {
            CustomerId = id;
            X = x;
            Y = y;
            Demand = demand;
            IsRouted = false;
        }
    }
}
