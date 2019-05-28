using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestMapBox.HelperClasses
{
    // возможно, будут дополнения
    public static class ForecastingDemand
    {
        public static int[] GenerateRandomDemand(int maxDemand)
        {
            Random random = new Random();
            const int COUNT = 300;
            int[] series = new int[COUNT];
            for (var i = 0; i < COUNT; i++)
            {
                series[i] = random.Next((int)maxDemand/2,maxDemand);
            }

            return series;
        }

        public static int DoubleExponentialSmoothing(int[] series, double alpha, double beta)
        {
            var level = 0.0;
            var trend = 0.0;
            var value = 0.0;
            var result = new List<double>();
            result.Add(series[0]);
            for (var i = 1; i < series.Length + 1; i++)
            {
                if (i == 1)
                {
                    level = series[0];
                    trend = series[1] - series[0];
                }

                value = i >= series.Length ? result.Last() : series[i];
                var lastLevel = level;
                level = alpha * value + (1 - alpha) * (level + trend);
                trend = beta * (level - lastLevel) + (1 - beta) * trend;
                result.Add(level + trend);

            }

            return (int)Math.Ceiling(result.Last());
        }
    }
}
