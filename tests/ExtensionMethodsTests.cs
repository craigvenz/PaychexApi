using System;
using FluentAssertions;
using Lcp.Paychex.Models.Workers;
using Lcp.Services.Paychex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Lcp.Services.Tests.Paychex.PaychexDataGenerator;

namespace Lcp.Services.Tests.Paychex
{
    [TestClass]
    public class ExtensionMethodsTests : TestBase
    {
        [TestMethod]
        public void GenderStrings()
        {
            Gender.MALE.TranslateString()
                  .Should()
                  .Be("M");

            Gender.FEMALE.TranslateString()
                  .Should()
                  .Be("F");

            ((Gender)3).TranslateString()
                       .Should()
                       .Be(string.Empty);
        }

        [TestMethod]
        public void EthnicityStrings()
        {
            EthnicityType.AMERICAN_INDIAN_OR_ALASKAN_NATIVE.TranslateString()
                         .Should()
                         .Be("NATIVE AMERICAN");
            EthnicityType.ASIAN.TranslateString()
                         .Should()
                         .Be("ASIAN");
            EthnicityType.ASIAN_OR_PACIFIC_ISLANDER.TranslateString()
                         .Should()
                         .Be("ASIAN");
            EthnicityType.NATIVE_HAWAIIAN_OR_OTHER_PACIFIC_ISLAND.TranslateString()
                         .Should()
                         .Be("ASIAN");
            EthnicityType.BLACK_OR_AFRICAN_AMERICAN.TranslateString()
                         .Should()
                         .Be("AFRICAN AMERICAN");
            EthnicityType.HISPANIC_OR_LATINO.TranslateString()
                         .Should()
                         .Be("HISPANIC");
            EthnicityType.TWO_OR_MORE_RACES.TranslateString()
                         .Should()
                         .Be("TWO OR MORE RACES");
            EthnicityType.WHITE_NOT_OF_HISPANIC_ORIGIN.TranslateString()
                         .Should()
                         .Be("CAUCASIAN");
            ((EthnicityType)999).TranslateString()
                                .Should()
                                .Be(string.Empty);
        }

        [TestMethod]
        public void BetweenTests()
        {
            10.Between(1, 11)
              .Should()
              .BeTrue();

            20.Between(-1, 1)
              .Should().BeFalse();

            5.Between(1, 5, PaychexConvenienceExtensions.RangeBoundaryType.InclusiveLowerBoundaryOnly)
             .Should().BeFalse();

            10.0m.Between(9m, 15m, PaychexConvenienceExtensions.RangeBoundaryType.Exclusive)
                 .Should()
                 .BeTrue();

            DateTime.Today.Between(DateTime.Today.AddDays(-1), DateTime.Today.AddHours(1))
                    .Should()
                    .BeTrue();

            true.Between(false, true, PaychexConvenienceExtensions.RangeBoundaryType.InclusiveUpperBoundaryOnly)
                .Should()
                .BeTrue();

            true.Between(false, true, PaychexConvenienceExtensions.RangeBoundaryType.InclusiveLowerBoundaryOnly)
                .Should()
                .BeFalse();

            Action failTest = () => 10.Between(9m, 12m);
            failTest.Should()
                    .Throw<ArgumentException>();
        }

        [TestMethod]
        public void GetHireDateTests()
        {
            var job = GenerateJob(
                (_, o) => o.StartDate = new DateTime(2017, 1, 1)).Generate();

            var worker = GenerateWorker(null, null,
                    (__, o) =>
                    {
                        o.HireDate = new DateTime(2011, 1, 1);
                        o.HomeJob = GenerateJob(
                            (_, oo) => oo.StartDate = new DateTime(2015, 1, 1)).Generate();
                        o.Job = new JobTitle
                        {
                            StartDate = new DateTime(2014, 2, 10),
                            Title = "test",
                            JobTitleId = GeneratePaychexSubId()
                                         .Generate()
                                         .Id
                        };
                        o.HomeAssignment = GenerateLaborAssignment(1,
                            (_, oo) => oo.StartDate = new DateTime(2016, 1, 1)
                        )[0];
                        o.LaborAssignmentId = o.HomeAssignment.LaborAssignmentId;
                    }
                )
                .Generate();

            var data = new CheckLine()
            {
                Amount = 1,
                CheckComponentId = GeneratePaychexSubId()
                                                       .Generate()
                                                       .Id,
                CheckLineType = LineType.Earning,
                Hours = 1,
                LineDate = DateTime.Today,
                Name = "Hourly",
                Job = job,
                Rate = 10,
                ContainingCheck = new Lcp.Paychex.Models.Payroll.Check
                {
                    CheckDate = DateTime.Today,
                    CheckNumber = "1234",
                    CheckType = "Regular",
                    NetPay = 10,
                    PaycheckId = GeneratePaychexSubId().Generate().Id,
                    Payee = worker,
                }
            };

            data.GetHireDate()
                .Should()
                .Be(new DateTime(2017, 1, 1));

            data.Job = null;
            data.Assignment = new AssignmentAndTitle(
                GenerateLaborAssignment(1, (_, o) => o.StartDate = new DateTime(2017, 2, 3))[0]
            );

            data.GetHireDate()
                .Should()
                .Be(new DateTime(2017, 2, 3));

            data.Assignment = null;

            data.GetHireDate()
                .Should()
                .Be(new DateTime(2015, 1, 1));

            worker.HomeJob = null;

            data.GetHireDate()
                .Should()
                .Be(new DateTime(2016, 1, 1));

            worker.HomeAssignment = null;

            data.GetHireDate()
                .Should()
                .Be(new DateTime(2014, 2, 10));

            worker.Job = null;

            data.GetHireDate()
                .Should()
                .BeNull();
        }
    }
}