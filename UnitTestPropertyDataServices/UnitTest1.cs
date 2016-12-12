using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestPropertyDataServices
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            PropertyWebAPI.Common.Logs.log().Info("Test");
        }
    }
}
