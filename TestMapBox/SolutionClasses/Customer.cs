namespace TestMapBox.SolutionClasses
{
    internal class Customer
    {
        public int CustomerId { get; set; }
        public int Demand { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }

        public bool IsRouted;

        public Customer(decimal x, decimal y) //for depot
        {
            CustomerId = 0;
            X = x;
            Y = y;
        }

        public Customer(int id, decimal x, decimal y, int demand)
        {
            CustomerId = id;
            X = x;
            Y = y;
            Demand = demand;
            IsRouted = false;
        }
    }
}
