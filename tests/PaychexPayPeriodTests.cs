using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using System.Threading.Tasks;
using FluentAssertions;
using Lcp.Data.Accounts;
using Lcp.Paychex.Api.Interfaces;
using Lcp.Paychex.Models;
using Lcp.Paychex.Models.Payroll;
using Lcp.Services.Paychex;
using Lcp.Data;
using Lcp.Paychex.Models.Common;
using Lcp.Paychex.Models.Companies;
using Lcp.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lcp.Services.Tests.Paychex
{
    [TestClass]
    public class PaychexPayPeriodTests
    {
        [TestMethod]
        public async Task RetrievePayPeriodsAsync_WorksAsExpected()
        {
            // Arrange
            var mockServices = new Mock<IServices>();
            var mockApiClient = new Mock<IPaychexApiClient>();
            var mockContext = new Mock<IContext>();
            var mockRepositoryFactory = new Mock<IRepositoryFactory>();

            mockContext.Setup(c => c.Contractors)
              .Returns(new[]
              {
                    new Contractor()
                    {
                        Id = 2,
                        PaychexCompanyId = "Test Display ID 2"
                    }
              }.AsQueryable());

            mockServices.Setup(s => s.Session.ContractorId).Returns(2);

            mockApiClient.Setup(m =>
                m.GetCompanyByDisplayIdAsync(It.Is<string>(r => r == "Test Display ID 2")))
                .ReturnsAsync(
                new ApiResponse<Company>
                {
                    content = new List<Company> { new Company { CompanyId = "Test Company 2" } }
                });

            mockApiClient.Setup(
                             x => x.GetCompanyChecksAsync(
                                 It.Is<string>(r => r == "Test Company 2"),
                                 It.Is<string>(r => r == "0"),
                                 It.IsAny<Pagination>()
                             )
                         )
                         .ReturnsAsync(
                             new ApiResponse<Check>()
                             {
                                 content = new List<Check> {
                                     new Check {
                                         Deductions = new List<Earning>(),
                                         Earnings = new List<Earning>() {
                                             new Earning {
                                                 JobId = "1", LineDate = new DateTime(2019, 7, 1)
                                             }}}}
                             });
            mockApiClient.Setup(
                             x => x.GetCompanyChecksAsync(
                                 It.Is<string>(r => r == "Test Company 2"),
                                 It.Is<string>(r => r == "2"),
                                 It.IsAny<Pagination>()
                             )
                         )
                         .ReturnsAsync(
                             new ApiResponse<Check>()
                             {
                                 content = new List<Check>
                                 {
                                     new Check
                                     {
                                         Deductions = new List<Earning>(),
                                         Earnings = new List<Earning>()
                                         {
                                             new Earning
                                             {
                                                 JobId = "1", LineDate = new DateTime(2019, 1, 7), Amount = 100
                                             }
                                         }
                                     },
                                     new Check
                                     {
                                         Deductions = new List<Earning>(),
                                         Earnings = new List<Earning>()
                                         {
                                             new Earning
                                             {
                                                 JobId = "2", LineDate = new DateTime(2019, 1, 8), Amount = 100
                                             }
                                         }
                                     },
                                     new Check
                                     {
                                         Deductions = new List<Earning>(),
                                         Earnings = new List<Earning>()
                                         {
                                             new Earning
                                             {
                                                 JobId = "3", LineDate = new DateTime(2019, 1, 9), Amount = 100
                                             }
                                         }
                                     },
                                     new Check
                                     {
                                         Deductions = new List<Earning>(),
                                         Earnings = new List<Earning>()
                                         {
                                             new Earning
                                             {
                                                 JobId = "4", LineDate = new DateTime(2019, 1, 8), Amount = 100
                                             }
                                         }
                                     },
                                     new Check
                                     {
                                         Deductions = new List<Earning>(),
                                         Earnings = new List<Earning>()
                                         {
                                             new Earning
                                             {
                                                 JobId = "5", LineDate = new DateTime(2019, 1, 7), Amount = 100
                                             }
                                         }
                                     },
                                 }
                             }
                         );
            mockApiClient.Setup(
                             x => x.GetCompanyChecksAsync(
                                 It.Is<string>(r => r == "Test Company 2"),
                                 It.Is<string>(r => r == "11"),
                                 It.IsAny<Pagination>()
                             )
                         )
                         .ReturnsAsync(
                             new ApiResponse<Check>()
                             {
                                 content = new List<Check> {
                                     new Check {
                                         Earnings = new List<Earning>() {
                                             new Earning {
                                                 JobId = "1", LineDate = new DateTime(2019, 1, 7),
                                                 Amount = 100
                                             }}},
                                     new Check
                                     {
                                         Deductions = new List<Earning>(),
                                         Earnings = new List<Earning>
                                         {
                                             new Earning
                                             {
                                                 JobId = "2", LineDate = new DateTime(2019, 1, 8)
                                             }
                                         }
                                     }

                                 }
                             });
            mockApiClient.Setup(
                             x => x.GetCompanyChecksAsync(
                                 It.Is<string>(r => r == "Test Company 2"),
                                 It.Is<string>(r => r == "5"),
                                 It.IsAny<Pagination>()
                             )
                         )
                         .ReturnsAsync(
                             new ApiResponse<Check>()
                             {
                                 content = new List<Check>()
                             }
                         );




            mockApiClient.Setup(
                             m =>
                                 m.SearchPayPeriodsAsync(
                                     It.Is<string>(r => r == "Test Company 2"),
                                     It.IsAny<string>(),
                                     It.Is<DateTime>(r => r == DateTime.Parse("1/1/2019")),
                                     It.Is<DateTime>(r => r == DateTime.Parse("1/31/2019"))
                                 )
                         )
                         .ReturnsAsync(
                             new ApiResponse<PayPeriod>
                             {
                                 content = new List<PayPeriod>
                                 {
                                     new PayPeriod 
                                     {
                                         PayPeriodId = "0",
                                         StartDate = DateTime.Parse("1/7/2019"),
                                         EndDate = DateTime.Parse("1/11/2019"),
                                         Status = PayPeriod.PayPeriodStatus.COMPLETED,
                                         CheckCount = 1,
                                         IntervalCode = PayPeriod.IntervalCodes.WEEKLY
                                     },
                                     new PayPeriod 
                                     {
                                         PayPeriodId = "2",
                                         StartDate = DateTime.Parse("1/14/2019"),
                                         EndDate = DateTime.Parse("1/25/2019"),
                                         Status = PayPeriod.PayPeriodStatus.COMPLETED,
                                         CheckCount = 5,
                                         IntervalCode = PayPeriod.IntervalCodes.WEEKLY
                                     },
                                     new PayPeriod // in range but no checks
                                     {
                                         PayPeriodId = "5",
                                         StartDate = DateTime.Parse("1/7/2019"),
                                         EndDate = DateTime.Parse("1/11/2019"),
                                         Status = PayPeriod.PayPeriodStatus.COMPLETED,
                                         CheckCount = 0,
                                         IntervalCode = PayPeriod.IntervalCodes.WEEKLY
                                     },
                                     new PayPeriod
                                     {
                                         PayPeriodId = "11",
                                         StartDate = DateTime.Parse("1/6/2019"),
                                         EndDate = DateTime.Parse("1/14/2019"),
                                         Status = PayPeriod.PayPeriodStatus.COMPLETED,
                                         CheckCount = 2,
                                         IntervalCode = PayPeriod.IntervalCodes.NONE
                                     }
                                 }
                             }
                         );

            // Act
            var target =
                new PaychexApiMapping(
                    mockApiClient.Object,
                    new HttpRetryStrategy(),
                    mockRepositoryFactory.Object
                );

            var result = await target.RetrievePayPeriodsAsync(
                "Test Company 2",
                DateTime.Parse("1/1/2019"),
                DateTime.Parse("1/31/2019")
            );

            result.Should().HaveCount(3);
            result.Select(x => x.PayPeriodId)
                  .Should()
                  .BeEquivalentTo(
                      new List<string>()
                      {
                          "0", "2", "11"
                      }
                  );
        }
    }
}

