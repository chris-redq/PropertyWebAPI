using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MathNet.Numerics.Statistics;

namespace PropertyWebAPI.Common
{
    public class DoubleList: List<double>
    {
        public double Average(Func<double, bool> filter)
        {
            int i = 0;
            double sumValue1 = 0.0;

            foreach (var val in this)
            {
                if (filter(val))
                {   i++;
                    sumValue1 += val;
                }
            }
            return sumValue1 / i;
        }

        public double Average()
        {
            int i = 0;
            double sumValue1 = 0.0;

            foreach (var val in this)
            {
                i++;
                sumValue1 += val;
            }
            return sumValue1 / i;
        }
    }

    public class Statistics
    {
        public static double Percentile(IEnumerable<double> values, int percentile)
        {
            return MathNet.Numerics.Statistics.Statistics.Percentile(values, percentile);
        }

        public static bool IsOutLier(double inputValue, double q1, double q3)
        {
            double iQR = q3 - q1;

            if ( inputValue >= q1 - 1.5*iQR && inputValue <= q3+1.5*iQR)
                return false;
            return true;
        }
    }
}