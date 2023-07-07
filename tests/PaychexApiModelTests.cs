// LcpMvc.Lcp.Services.Tests.PaychexApiModelTests.cs
// Craig Venz - 10/01/2019 - 3:37 PM

using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Lcp.Paychex.Api;
using Lcp.Paychex.Models;
using Lcp.Paychex.Models.Authentication;
using Lcp.Paychex.Models.Companies;
using Lcp.Paychex.Models.Payroll;
using Lcp.Paychex.Models.Workers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkerCommunication = Lcp.Paychex.Models.Workers.Communication;
using CompanyCommunication = Lcp.Paychex.Models.Companies.Communication;

namespace Lcp.Services.Tests.Paychex
{
    [TestClass]
    public class PaychexApiModelTests : TestBase
    {
        [TestMethod]
        public void Communication_ToStringMethod_PhoneNumbers()
        {
            var testObject = new WorkerCommunication
            {
                Type = Lcp.Paychex.Models.Common.ContactType.MOBILE_PHONE,
                DialArea = "123",
                DialCountry = "1",
                DialNumber = "4567890",
                DialExtension = "11"
            };
            testObject.ToString()
                      .Should()
                      .BeEquivalentTo(testObject.AsPhoneNumber());
            testObject.Type = Lcp.Paychex.Models.Common.ContactType.PHONE;
            testObject.ToString()
                      .Should()
                      .Be(testObject.AsPhoneNumber())
                      .And
                      .Be("+1 (123) 456-7890 ext 11");
        }

        [TestMethod]
        public void Communication_ToStringMethod_Emails()
        {
            var testObject = new WorkerCommunication
            {
                Type = Lcp.Paychex.Models.Common.ContactType.EMAIL, Uri = "test@test.com"
            };
            testObject.ToString()
                      .Should()
                      .Be(testObject.Uri);
        }

        [TestMethod]
        public void Communication_ToStringMethod_StreetAddresses()
        {
            var testObject = new WorkerCommunication
            {
                Type = Lcp.Paychex.Models.Common.ContactType.STREET_ADDRESS,
                UsageType = Lcp.Paychex.Models.Common.ContactUsageType.BUSINESS,
                StreetLineOne = "1 test st",
                StreetLineTwo = "apt 111",
                City = "testville",
                CountrySubdivisionCode = "CA",
                CountryCode = "USA",
                PostalCode = "92000"
            };

            testObject.ToString()
                      .Should()
                      .Be(
                          "Business Contact - 1 test st apt 111 testville CA, USA - 92000"
                      );

            testObject.UsageType = null;
            testObject.ToString()
                      .Should()
                      .Be(
                          "1 test st apt 111 testville CA, USA - 92000"
                      );

            testObject.StreetLineTwo = string.Empty;
            testObject.ToString()
                      .Should()
                      .Be(
                          "1 test st testville CA, USA - 92000"
                      );

            testObject.PostOfficeBox = "PO box 1234";
            testObject.ToString()
                      .Should()
                      .Be(
                          "1 test st testville CA, USA - 92000"
                      );

            testObject.Type = Lcp.Paychex.Models.Common.ContactType.PO_BOX_ADDRESS;
            testObject.ToString()
                      .Should()
                      .Be(
                          "PO box 1234 testville CA, USA - 92000"
                      );
        }

        [TestMethod]
        public void Communication_CopyFrom_WorksAsExpected()
        {
            var firstObject = PaychexDataGenerator.GenerateAddress().Generate();
            
            var second = new WorkerCommunication(firstObject);
            second.Type.Should()
                  .Be(Lcp.Paychex.Models.Common.ContactType.STREET_ADDRESS);
            second.UsageType.Should()
                  .Be(firstObject.UsageType);
            second.City.Should()
                  .Be(firstObject.City);
            second.StreetLineOne.Should()
                  .Be(firstObject.StreetLineOne);
            second.StreetLineTwo.Should()
                  .Be(firstObject.StreetLineTwo);
            second.CountryCode.Should()
                  .Be(firstObject.CountryCode);
            second.CountrySubdivisionCode.Should()
                  .Be(firstObject.CountrySubdivisionCode);
        }

        [TestMethod]
        public void Job_ToStringMethod_ProducesAsExpected()
        {
            var testJob = PaychexDataGenerator.GenerateJob()
                                              .Generate();

            testJob.ToString()
                   .Should()
                   .Be(testJob.JobName);

            testJob.JobName = string.Empty;
            testJob.ToString()
                   .Should()
                   .Be(testJob.JobNumber.ToString());

            testJob.JobNumber.Segment1 = "111";
            testJob.JobNumber.Segment2 = "222";
            testJob.JobNumber.Segment3 = "333";

            testJob.ToString()
                   .Should()
                   .Be("111-222-333");
            testJob.JobNumber.Segment3 = string.Empty;
            testJob.ToString()
                   .Should()
                   .Be("111-222");
            testJob.JobNumber.Segment2 = string.Empty;
            testJob.ToString()
                   .Should()
                   .Be("111");
            testJob.JobNumber.Segment3 = "333";
            testJob.ToString()
                   .Should()
                   .Be("111-333");
            testJob.JobNumber.Segment1 = string.Empty;
            testJob.ToString()
                   .Should()
                   .Be("333");
            testJob.JobNumber.Segment3 = string.Empty;
            testJob.ToString()
                   .Should()
                   .Be(string.Empty);
        }

        [TestMethod]
        public void Check_ToStringMethod_ProducesAsExpected()
        {
            var newCheck = new Check
            {
                CheckNumber = "1234",
                CheckDate = new DateTime(2019,1,1),
                NetPay = 1000
            };

            newCheck.ToString()
                    .Should()
                    .Be($"Check 1234, 1/1/2019 - $1,000.00");

            var check2 = new Check
            {
                CheckNumber = "1234",
                NetPay = 2000,
                CheckDate = DateTime.Today
            };

            newCheck.Should()
                    .Be(check2);
        }

        [TestMethod]
        public void Check_Equality_IsBy_CheckNumber_Only()
        {
            var newCheck = new Check
            {
                CheckNumber = "1234",
                CheckDate = new DateTime(2019,1,1),
                NetPay = 1000
            };
            var check2 = new Check
            {
                CheckNumber = "1234",
                NetPay = 2000,
                CheckDate = DateTime.Today
            };
            newCheck.Should()
                    .Be(check2);
        }

        [TestMethod]
        public void Earning_ToStringMethod_ProducesAsExpected()
        {
            var earning = new Earning
            {
                CheckComponentId = "12345678",
                Name = "Hourly",
                LineDate = new DateTime(2019,1,1),
                Rate = 50,
                Hours = 10,
                Amount = 1000,
                JobName = "test job name",
                Job = new Job
                {
                    JobName = "something else"
                },
                Assignment = new Lcp.Paychex.Models.Common.LaborAssignment
                {
                    Name = "assignment thingy",
                },
                Organization = new Lcp.Paychex.Models.Common.Organization
                {
                    Name = "an org",
                    Number = "999"
                }
            };

            earning.ToString()
                   .Should()
                   .Be(
                       "12345678: Hourly 1/1/2019 10hrs @ $50.00 = $1,000.00 something else assignment thingy an org (999)"
                   );
            // cbf doing permutations of this right now
        }

        [TestMethod]
        public void PropertyCoverage_Of_SimpleModels()
        {
            // common
            TestCoverageOf<Lcp.Paychex.Models.Common.Communication>();
            TestCoverageOf<Lcp.Paychex.Models.Common.LaborAssignment>();
            TestCoverageOf<Lcp.Paychex.Models.Common.LegalId>();
            TestCoverageOf<Lcp.Paychex.Models.Common.Link>();
            TestCoverageOf<Lcp.Paychex.Models.Common.Metadata>();
            TestCoverageOf<Lcp.Paychex.Models.Common.Organization>();
            TestCoverageOf<Lcp.Paychex.Models.Common.Pagination>();

            // companies
            TestCoverageOf<Job>();
            TestCoverageOf<JobNumber>();
            TestCoverageOf<JobSegment>();
            TestCoverageOf<OccurrenceIntervals>();
            TestCoverageOf<PayComponentFrequency>();
            TestCoverageOf<PayFrequencies>();

            // payroll
            TestCoverageOf<Check>();
            TestCoverageOf<Earning>();
            TestCoverageOf<PayPeriod>();
            TestCoverageOf<PayrollComponent>();
            TestCoverageOf<Tax>();

            // workers
            TestCoverageOf<Name>();
            TestCoverageOf<CurrentStatus>();
            TestCoverageOf<JobTitle>();

            // other
            TestCoverageOf<PaychexAuthToken>();
            TestCoverageOf<ApiError>();
            TestCoverageOf<ApiResponse<Check>>();
        }

        [TestMethod]
        public void Company_PropertyCoverage()
        {
            var f = new Fixture();

            ExerciseProperties(f.Build<Company>()
                                 .With(c => c.Communications,
                                     f.Build<CompanyCommunication>()
                                      .Without(cc=>cc.Company)
                                      .CreateMany()
                                      .ToList())
                                );
        }

        [TestMethod]
        public void Worker_PropertyCoverage()
        {
            var f = new Fixture();

            ExerciseProperties(f.Build<Worker>()
                                .With(x => x.Communications,
                                    f.Build<WorkerCommunication>()
                                     .Without(c => c.Worker)
                                     .CreateMany()
                                     .ToList() ));
        }
        [TestMethod]
        public void WorkerCommunication_PropertyCoverage()
        {
            var f = new Fixture();
            ExerciseProperties(
                f.Build<WorkerCommunication>()
                 .With(
                     c => c.Worker,
                     f.Build<Worker>()
                      .Without(w => w.Communications)
                      .Create()
                 )
            );
        }

    }
}