using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PropertyWebAPI;
using Newtonsoft.Json;
using PropertyWebAPI.BAL;
using PropertyWebAPI.Common;
using System.Collections.Generic;

namespace UnitTestPropertyDataServices
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void TestPortalCallBackMethodForDataServices()
        {
            Logs.Init();
            Logs.log().Info("Integration Test");

            var str = @"{ ""taxBill"":{ ""externalReferenceId"":""19857"",""status"":""Success"",""BBL"":""1010901183"",""billAmount"":6776.44},""mortgageServicer"":{ ""externalReferenceId"":""19857"",""status"":""Success"",""BBL"":""1010901183"",""servicerName"":""CALIBER HOME LOANS""},""dobPenaltiesAndViolationsSummary"":{ ""externalReferenceId"":""19857"",""status"":""Success"",""BBL"":""1010901183"",""civilPenaltyAmount"":0.00,""violationAmount"":-200.00},""zillowPorperty"":{ ""externalReferenceId"":""19857"",""status"":""Success"",""BBL"":""1010901183"",""zEstimate"":1713069.00} }";
            var resultObj =  JsonConvert.DeserializeObject<Results>(str);

            CallingSystem.PostCallBack("http://104.130.40.122:8080/", "api/Dataservice/completed", 
                                "6F43717752E839FD9034B6C426770488A7BA624E9E6D018E26D52451C7A4BCFE56338E92D621F826AC8B8228DDFEC4D7628AAC4917A06F3AE6CD56C978A691CA",
                                "Portal", resultObj);
            return;
        }

        [TestMethod]
        public void TestToDecimalConversion()
        {
            decimal? resultObj = Conversions.ToDecimalorNull("192,000");
        }


        [TestMethod]
        public void TestAverage()
        {
            DoubleList values = new DoubleList { 1, 2, 2, 3, 4, 5, 6, 6.3, 7, 7.4, 8, 9, 10 };

            double a = 4.0, b = 8.0;

            double val = values.Average(x => x >= a && x <= b);

            return;
        }

        [TestMethod]
        public void TestClustering()
        {
            DoubleList values1 = new DoubleList { 525.49, 521.86, 509.38, 319.55, 375.00 };
            values1.Sort();
            double[] smoothValues = Statistics.ApplyGaussianKDE(values1.ToArray(), 3, 0.45);
            double[] derivatives = Statistics.DiscreteDerivative(smoothValues);
            List<int> mins = Statistics.FindLocalMinima(derivatives);
            mins = Statistics.FindClusters(derivatives, 0.06);

            values1 = new DoubleList { 525.49, 521.86, 509.38, 319.55, 375.00, 325.00, 342.00 };
            values1.Sort();
            smoothValues = Statistics.ApplyGaussianKDE(values1.ToArray(), 3, 0.45);
            derivatives = Statistics.DiscreteDerivative(smoothValues);
            mins = Statistics.FindLocalMinima(derivatives);
            mins = Statistics.FindClusters(derivatives, 0.06);

            return;
        }
    }
}
