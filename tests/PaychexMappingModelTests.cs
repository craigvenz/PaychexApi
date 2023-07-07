using FluentAssertions;
using Lcp.Services.Paychex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lcp.Services.Tests.Paychex
{
    [TestClass]
    public class PaychexMappingModelTests : TestBase
    {
        [TestMethod]
        public void PropertyCoverageOfModels()
        {
            TestCoverageOf<ApiDataSet>();
            TestCoverageOf<AssignmentAndTitle>();
            TestCoverageOf<CheckLine>();
            TestCoverageOf<ApiMappingInternals.CheckData>();
            TestCoverageOf<ApiMappingInternals.DataRowGlob>();
            TestCoverageOf<ApiMappingInternals.WorkerChecks>();
        }

        [TestMethod]
        public void EqualityTests()
        {
            TestHashCodes<ApiMappingInternals.WorkerChecks>();
            TestHashCodes<AssignmentAndTitle>();
        }

        [TestMethod]
        public void CheckLineCalculatedAmountReturnsCorrectly()
        {
            var cl = new CheckLine
            {
                Amount = 100,
                Hours = 10,
                Rate = 5
            };

            cl.CalculatedAmount.Should()
              .Be(100);

            cl.Amount = null;
            cl.CalculatedAmount.Should()
              .Be(50);

            cl.Rate = null;
            cl.CalculatedAmount.Should()
              .Be(0);

            cl.Rate = 5;
            cl.Hours = null;
            cl.CalculatedAmount.Should()
              .Be(0);
        }
    }
}
