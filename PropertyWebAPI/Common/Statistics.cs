using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MathNet.Numerics.Statistics;

namespace PropertyWebAPI.Common
{
    public class DoubleList: List<double>
    {
        public DoubleList(): base()
        {
                
        }

        public DoubleList(List<double> a)
        {
            AddRange(a);
        }

        public DoubleList SubList(Func<double, bool> filter)
        {
            DoubleList retList = new Common.DoubleList();

            foreach (var val in this)
            {
                if (filter(val))
                    retList.Add(val);
            }
            return retList;
        }


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

        public double Min(Func<double, bool> filter)
        {
            double? minValue=null;

            foreach (var val in this)
            {
                if (filter(val))
                {
                    if (minValue==null)
                        minValue = val;
                    else if (minValue > val)
                        minValue = val;
                }
            }
            return minValue.GetValueOrDefault();
        }

        public double Max(Func<double, bool> filter)
        {
            double? maxValue = null;

            foreach (var val in this)
            {
                if (filter(val))
                {
                    if (maxValue == null)
                        maxValue = val;
                    else if (maxValue < val)
                        maxValue = val;
                }
            }
            return maxValue.GetValueOrDefault();
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

        public static double Guassian(int x, double sigma)
        {
            double c = 2.0 * sigma * sigma;
            return Math.Exp(-(x * x) / c) / Math.Sqrt(c * Math.PI);
        }
        public static double[] GuassianTerms(int kernalSize, double sigma)
        {
            var terms = new double[kernalSize];
            for (int i = 0; i < kernalSize; ++i)
            {
                terms[i] = Guassian(i - kernalSize / 2, sigma);
            }
            return terms;
        }

        public static double[] DiscreteNormalizedGuassianTerms(int kernalSize, double sigma)
        {
            var terms = GuassianTerms(kernalSize, sigma);
            var sum = terms.Aggregate((t1, t2) => t1 + t2); // aggregate to normalize result
            for (int i = 0; i < kernalSize; i++)
                terms[i] = terms[i] / sum;
            return terms;
        }

        public static double[] ApplyGaussianKDE(double[] values, int kernalSize, double sigma)
        {
            int inputArraySize = values.Count();
            int kernelPad = kernalSize / 2;

            double[] paddedValues = new double[inputArraySize + 2 * kernelPad];
            for (int i = 0; i < kernelPad; i++)
                paddedValues[i] = values[0];

            for (int i = kernelPad + inputArraySize; i < inputArraySize + 2 * kernelPad; i++)
                paddedValues[i] = values[inputArraySize-1];

            for (int i = kernelPad; i < inputArraySize + kernelPad; i++)
                paddedValues[i] = values[i- kernelPad];

            var terms = DiscreteNormalizedGuassianTerms(kernalSize, sigma);
            double newValue;
            for (int i = kernelPad; i < inputArraySize + kernelPad; i++)
            {
                newValue = 0.0;
                for (int j = 0; j < kernalSize; j++)
                    newValue = newValue + terms[j] * paddedValues[i - kernelPad + j];
                values[i - kernelPad] = newValue;
            }

            return values;
        }

        public static double[] DiscreteDerivative(double[] values)
        {
            int inputArraySize = values.Count();
            double[] derivatives = new double[inputArraySize-1];

            for (int i = 0; i < inputArraySize - 1; i++)
                 derivatives[i] = Math.Abs((values[i + 1] - values[i]) / values[i]);
            return derivatives;
        }

        public static List<int> FindLocalMinima(double[] derivatives)
        {
            int inputArraySize = derivatives.Count();
            double? lastValue = null;
            double? direction = null;

            List<int> localMinima = new List<int>();

            for (int i =0; i< inputArraySize; i++)
            {                
                double delta = derivatives[i];
                if (lastValue == null)
                    lastValue = delta;
                else if (lastValue > delta)
                {
                    lastValue = delta;
                    direction = 0;
                }
                else if (lastValue < delta)
                {
                    if (direction != null && direction == 0)
                        localMinima.Add(i - 1);
                    direction = 1;
                    lastValue = delta;
                }
            }
            if (direction==0)
                localMinima.Add(inputArraySize - 1);
            return localMinima;
        }

        public static List<int> FindClusters(double[] derivatives, double threshold)
        {
            int inputArraySize = derivatives.Count();
            int count = 0;
            int startlocation = -1;

            List<int> localMinima = new List<int>();

            for (int i = 0; i < inputArraySize; i++)
            {
                if (derivatives[i] <= threshold)
                {   count++;
                    if (startlocation==-1)
                        startlocation = i;
                }
                else
                {   if (count >= 2)
                        localMinima.Add(startlocation);
                    count = 0;
                    startlocation = -1;
                }
            }
            if (count>=2)
                localMinima.Add(startlocation);
            return localMinima;
        }
    }
}