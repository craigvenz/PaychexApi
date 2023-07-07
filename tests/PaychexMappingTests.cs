using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Lcp.Data.Accounts;
using Lcp.Paychex.Api.Interfaces;
using Lcp.Paychex.Models;
using Lcp.Paychex.Models.Common;
using Lcp.Paychex.Models.Companies;
using Lcp.Paychex.Models.Payroll;
using Lcp.Paychex.Models.Workers;
using Lcp.Repositories;
using Lcp.Services.Paychex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using static Lcp.Services.Tests.Paychex.PaychexDataGenerator;

namespace Lcp.Services.Tests.Paychex
{
    [TestClass]
    public class PaychexMappingTests : TestBase
    {
        #region Test Setup

        private const string DataFolder = ".\\Paychex\\Data";
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();

        static PaychexMappingTests()
        {
            Components = LoadTestData<ApiResponse<PayrollComponent>>("paycomponents.json").content;
            (Hourly, OverTime, DoubleOverTime, Fringe) = Components.GetHourlyComponents();
        }

        private static readonly ICollection<PayrollComponent> Components;
        private static readonly PayrollComponent Hourly;
        private static readonly PayrollComponent OverTime;
        private static readonly PayrollComponent DoubleOverTime;
        private static readonly PayrollComponent Fringe;

        protected static T LoadTestData<T>(string fileName)
        {
            fileName = Path.Combine(DataFolder, fileName);
            using (var reader = new JsonTextReader(new StreamReader(GetFullPathToFile(fileName))))
            {
                return Serializer.Deserialize<T>(reader);
            }
        }

        private static (PaychexApiMapping map, Mock<IPaychexApiClient> clientMock, Mock<IContext> context, Faker faker)
            CreateTestObjects([CallerMemberName] string caller = "")
        {
            var clientMock = CreateTestMock<IPaychexApiClient>(caller);
            var mockAccountContext = new Mock<IContext>();
            var mockRepositoryFactory = new Mock<IRepositoryFactory>();

            mockRepositoryFactory.Setup(f => f.Repository)
                                 .Returns(
                                    new Repositories.Accounts.Repository(mockAccountContext.Object)
                                 );

            var map = new PaychexApiMapping(
                clientMock.Object,
                new NoOpStrategy(),
                mockRepositoryFactory.Object
            );
            return (map, clientMock, mockAccountContext, new Faker());
        }

        private static void SetupDbMapping(Mock<IContext> context, Company contractor, IDictionary<string, FieldType> maps)
        {
            var tc = new Contractor()
            {
                Id = 1,
                PaychexCompanyId = contractor.DisplayId
            };
            context.SetupGet(c => c.PaychexMappings)
                   .Returns(
                       maps.Select(
                           m => new PaychexMapping
                           {
                               ComponentId = m.Key,
                               IsDeleted = false,
                               FieldType = m.Value,
                               ContractorId = tc.Id,
                               Contractor = tc
                           }
                       ).AsQueryable()
                   );
        }

        private static void SetupDefaultDbMapping(Mock<IContext> context, Company company)
        {
            SetupDbMapping(
                context,
                company,
                new Dictionary<string, FieldType>()
                {
                    { Hourly.ComponentId, FieldType.BaseHourlyRate },
                    { OverTime.ComponentId, FieldType.OvertimeHourlyRate },
                    { DoubleOverTime.ComponentId, FieldType.DoubletimeHourlyRate },
                    {Fringe.ComponentId, FieldType.WagesPaidInLieuFringes }
                }
            );
        }

        private static void SetupEmptyResponses(Mock<IPaychexApiClient> clientMock, string companyId)
        {
            clientMock.Setup(c => c.GetLaborAssignmentsAsync(It.Is<string>(x => x == companyId)))
                      .ReturnsAsync(new ApiResponse<LaborAssignment>());

            clientMock.Setup(c => c.GetJobsAsync(It.Is<string>(x => x == companyId)))
                      .ReturnsAsync(new ApiResponse<Job>());

            clientMock.Setup(c => c.GetPayrollComponentsAsync(It.Is<string>(x => x == companyId), It.IsAny<string>()))
                      .ReturnsAsync(new ApiResponse<PayrollComponent>());

            clientMock.Setup(c => c.GetJobTitlesAsync(It.Is<string>(x => x == companyId)))
                      .ReturnsAsync(new ApiResponse<JobTitle>());

            clientMock.Setup(
                          c => c.GetCompanyChecksAsync(
                              It.IsAny<string>(),
                              It.IsAny<string>(),
                              It.IsAny<Pagination>()
                          )
                      )
                      .ReturnsAsync(new ApiResponse<Check>());

            clientMock.Setup(
                          c => c.SearchWorkersAsync(
                              It.IsAny<Lcp.Paychex.Api.WorkerSearchCriteria>(),
                              It.IsAny<Pagination>()
                          )
                      )
                      .ReturnsAsync(new ApiResponse<Worker>());
        }

        private static void SetupResponses(Mock<IPaychexApiClient> clientMock,
                                           Company company,
                                           PayPeriod payPeriod,
                                           IEnumerable<LaborAssignment> assignments,
                                           IEnumerable<Job> jobs,
                                           IEnumerable<PayrollComponent> components,
                                           IEnumerable<JobTitle> titles,
                                           IEnumerable<Worker> workers,
                                           IEnumerable<Check> checks)
        {
            clientMock.Setup(
                          c => c.GetPayPeriodByIdAsync(
                              It.Is<string>(id => id == company.CompanyId),
                              It.IsAny<string>()
                          )
                      )
                      .ReturnsAsync(
                          CreateResponse(
                              new[]
                              {
                                  payPeriod
                              }
                          )
                      );

            clientMock.Setup(c => c.GetCompanyByIdAsync(It.Is<string>(x => x == company.CompanyId)))
                      .ReturnsAsync(
                          CreateResponse(
                              new[]
                              {
                                  company
                              }
                          )
                      );

            clientMock.Setup(c => c.GetLaborAssignmentsAsync(It.Is<string>(x => x == company.CompanyId)))
                      .ReturnsAsync(CreateResponse(assignments));

            clientMock.Setup(c => c.GetJobsAsync(It.Is<string>(x => x == company.CompanyId)))
                      .ReturnsAsync(CreateResponse(jobs));

            clientMock.Setup(
                          c => c.GetPayrollComponentsAsync(
                              It.Is<string>(x => x == company.CompanyId),
                              It.IsAny<string>()
                          )
                      )
                      .ReturnsAsync(CreateResponse(components));

            clientMock.Setup(c => c.GetJobTitlesAsync(It.Is<string>(x => x == company.CompanyId)))
                      .ReturnsAsync(CreateResponse(titles));

            clientMock.Setup(
                          c => c.GetCompanyChecksAsync(
                              It.Is<string>(x => x == company.CompanyId),
                              It.Is<string>(x => x == payPeriod.PayPeriodId),
                              It.IsAny<Pagination>()
                          )
                      )
                      .ReturnsAsync(CreateResponse(checks));

            clientMock.Setup(
                          c => c.SearchWorkersAsync(
                              It.Is<Lcp.Paychex.Api.WorkerSearchCriteria>(x => x.CompanyId == company.CompanyId),
                              It.IsAny<Pagination>()
                          )
                      )
                      .ReturnsAsync(CreateResponse(workers));
        }

        private static Earning CreateEarningFromComponent(PayrollComponent payrollComponent, decimal? amount = null,
                                                          Job job = null, LaborAssignment assignment = null, DateTime? date = null,
                                                          decimal? hours = null, decimal? rate = null) =>

            new Earning
            {
                CheckComponentId = GeneratePaychexId().Generate().Id,
                ComponentId = payrollComponent.ComponentId,
                ClassificationType = payrollComponent.ClassificationType,
                Name = payrollComponent.Name,
                EffectOnPay = payrollComponent.EffectOnPay,
                Amount = hours.HasValue && rate.HasValue ? hours * rate : amount,
                JobId = job?.JobId,
                LaborAssignmentId = assignment?.LaborAssignmentId,
                LineDate = date,
                Hours = hours,
                Rate = rate,
                Job = job,
                Assignment = assignment,
                Component = payrollComponent
            };

        private static Tax CreateTax(Tax tax, decimal amount, string assignmentId, string jobId) =>
            new Tax
            {
                LaborAssignmentId = assignmentId,
                Amount = amount,
                Name = tax.Name,
                PaidBy = tax.PaidBy,
                JobId = jobId
            };

        #endregion Test Setup

        [TestInitialize]
        public void TestSetup()
        {
            //SetupRandom(87023296, "testing a specific seed");
        }

        /*
retrieve from api async
- no payroll components
- no check lines matching components
- weekly pay period with days outside specified period
- checks with no jobs?
- check with line date but no hourly or ot
- calculations add up(?)
- taxes/deductions same across all records per worker
- biweekly pay period generates two weekly records
- component with state name correctly goes into state bucket
- validate netpay?
- hourly rows unpivot to correct hour fields
         */

        [TestMethod]
        public async Task CompanyNotFound()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            clientMock.Setup(c => c.GetCompanyByIdAsync(It.IsAny<string>()))
                      .ReturnsAsync(new ApiResponse<Company>());

            Func<Task> act = async () =>
                             {
                                 await map.RetrieveFromApiAsync(
                                     "00Z6LYF217B6F42AQR0E",
                                     "1030012767572876",
                                     new[]
                                     {
                                         "123"
                                     }
                                 );
                             };
            await act.Should()
                     .ThrowAsync<ArgumentException>()
                     .WithMessage("Company was not found\nParameter name: contractorId");
        }

        [TestMethod]
        public async Task PayPeriodNotFound()
        {
            const string companyId = "00Z6LYF217B6F42AQR0E";
            const string payPeriodId = "1030012767572876";

            var (map, clientMock, _, _) = CreateTestObjects();

            clientMock.Setup(c => c.GetCompanyByIdAsync(It.Is<string>(x => x == companyId)))
                      .ReturnsAsync(
                          CreateResponse(
                              GenerateCompany(companyId)
                                  .Generate(1)
                          )
                      );
            clientMock.Setup(c => c.GetPayPeriodByIdAsync(It.Is<string>(id => id == companyId), It.IsAny<string>()))
                      .ReturnsAsync(
                          new ApiResponse<PayPeriod>()
                      );
            clientMock.Setup(
                c => c.SearchWorkersAsync(
                    It.Is<Lcp.Paychex.Api.WorkerSearchCriteria>(x => x.CompanyId == companyId),
                    It.IsAny<Pagination>()
                )
            ).ReturnsAsync(new ApiResponse<Worker>());

            SetupEmptyResponses(clientMock, companyId);

            clientMock.Setup(
                          c => c.GetCompanyChecksAsync(
                              It.Is<string>(x => x == companyId),
                              It.IsAny<string>(),
                              It.IsAny<Pagination>()
                          )
                      )
                      .ReturnsAsync(new ApiResponse<Check>());

            Func<Task> action = async () =>
                         {
                             await map.RetrieveFromApiAsync(
                                 companyId,
                                 payPeriodId,
                                 new[]
                                 {
                                     "Test Company"
                                 }
                             );
                         };

            await action.Should()
                  .ThrowAsync<PaychexException>()
                  .WithMessage($"Pay period {payPeriodId} was not found.");
        }

        [TestMethod]
        public async Task NoChecks()
        {
            const string companyId = "00Z6LYF217B6F42AQR0E";

            var (map, clientMock, _, _) = CreateTestObjects();

            var company = GenerateCompany(companyId).Generate();

            clientMock.Setup(c => c.GetCompanyByIdAsync(It.Is<string>(x => x == companyId)))
                      .ReturnsAsync(
                          CreateResponse(
                              new[]
                              {
                                  company
                              }
                          )
                      );

            var payPeriod = CreateResponse(
                GeneratePayPeriod()
                    .Generate(1)
            );

            SetupEmptyResponses(clientMock, companyId);

            clientMock.Setup(c => c.GetPayPeriodByIdAsync(It.Is<string>(id => id == companyId), It.IsAny<string>()))
                      .ReturnsAsync(payPeriod);

            clientMock.Setup(
                          c => c.SearchWorkersAsync(
                              It.Is<Lcp.Paychex.Api.WorkerSearchCriteria>(x => x.CompanyId == companyId),
                              It.IsAny<Pagination>()
                          )
                      )
                      .ReturnsAsync(
                          CreateResponse(
                              GenerateWorker(null, null)
                                  .Generate(5)
                          )
                      );

            var result = await map.RetrieveFromApiAsync(
                companyId,
                "1030012767572876",
                new[]
                {
                    "Test Company"
                }
            );

            result.Should()
                  .BeEmpty();
        }

        [TestMethod]
        public async Task NoWorkersMatchingCheck()
        {
            var (map, clientMock, _, _) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var taxes = LoadTestData<List<Tax>>("taxes.json");
            var pp = GeneratePayPeriod().Generate();
            var jobs = GenerateJob().Generate(3);
            var assignments = GenerateLaborAssignment(3);
            var workers = GenerateWorker(jobs, assignments).Generate(10);

            var checks = GenerateCheck(
                    pp,
                    GeneratePaychexId()
                        .Generate(5)
                        .Select(x => x.Id),
                    Components,
                    assignments,
                    jobs,
                    taxes
                )
                .Generate(5);

            SetupResponses(
                clientMock,
                company,
                pp,
                assignments,
                jobs,
                Components,
                null,
                workers,
                checks
            );

            var result = await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                new[]
                {
                    "Test Company"
                }
            );

            result.Should()
                  .BeEmpty();
        }

        [TestMethod]
        public async Task HourlyOutsideRangeIsNotAssigned()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var jobs = GenerateJob().Generate(1);
            var assignments = GenerateLaborAssignment(1);
            var workers = GenerateWorker(jobs, assignments).Generate(10);

            SetupDefaultDbMapping(context, company);

            Earning CreateHourlyEarning(Worker w, PayrollComponent payrollComponent, decimal hours, decimal rate, DateTime date)
            {
                var earn = CreateEarningFromComponent(
                    payrollComponent,
                    rate: rate,
                    hours: hours,
                    job: w.HomeJob,
                    assignment: w.HomeAssignment,
                    date: date
                );
                return earn;
            }

            var worker = faker.PickRandom(workers);
            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId().Generate().Id,
                    CheckNumber = "1000",
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourlyEarning(worker, Hourly, 8, 12.5m, pp.StartDate.AddDays(1)),
                        CreateHourlyEarning(worker, Hourly, 4, 12.5m, pp.StartDate.AddDays(-2)),
                        CreateHourlyEarning(worker, Hourly, 5, 12.5m, pp.EndDate.AddHours(1)),
                        CreateHourlyEarning(worker, OverTime, 2, 24, pp.StartDate.AddDays(2)),
                        CreateHourlyEarning(worker, OverTime, 1, 24, pp.StartDate.AddHours(-1)),
                        CreateHourlyEarning(worker, OverTime, 0.5m, 24, pp.EndDate.AddDays(1)),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignments,
                jobs,
                Components,
                new List<JobTitle>(),
                workers,
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                jobs.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            var row = result.First();
            row.st_hrs_date2.Should().Be(8);
            row.ov_hrs_date3.Should().Be(2);
            row.StandardHours.Sum().Should().Be(8);
            row.OvertimeHours.Sum().Should().Be(2);
            row.TotalHours.Should().Be(10);
        }

        [TestMethod]
        public async Task AmountsOnChecksMatchHourlyRates()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(1);
            var assignment = GenerateLaborAssignment(1);
            var worker = GenerateWorker(job, assignment).Generate();

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: assignment.Single(), job: job.Single());

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    Payee = worker,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(1)),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(-2)),
                        CreateHourly(Hourly, 5, 12.5m, pp.EndDate.AddHours(1)),
                        CreateHourly(Hourly, 1, 12.5m, pp.StartDate.AddDays(3)),
                        CreateHourly(OverTime, 2, 24, pp.StartDate.AddDays(2)),
                        CreateHourly(OverTime, 1, 24, pp.StartDate.AddHours(-1)),
                        CreateHourly(OverTime, 0.5m, 24, pp.EndDate.AddDays(1)),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            var row = result.First();
            row.TotalHours.Should()
               .Be(11);
            row.StandardHours.Should()
               .BeEquivalentTo(
                   new[]
                   {
                       0, 8, 1, 0, 0, 0, 0
                   }
               );
            row.OvertimeHours.Should()
               .BeEquivalentTo(
                   new[]
                   {
                       0, 0, 2, 0, 0, 0, 0
                   }
               );
            row.TotalsForThisPeriod.Should()
               .Be(160.5M);

            row.ep_hrate.Should()
               .Be(12.5m);
            row.ot_daily.Should()
               .Be(24);
        }

        [TestMethod]
        public async Task EmployeeHireDate()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(1);
            var assignment = GenerateLaborAssignment(1);
            var worker = GenerateWorker(job, assignment).Generate();

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: assignment.Single(), job: job.Single());

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    Payee = worker,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(1)),
                    }
                }
            };

            // Worker class has hire date value set
            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            var row = result.First();
            row.date_hired.Should().Be(worker.HireDate);

            // Worker class does not have hire date value set
            // GetHireDate extension method value is used instead
            worker.HireDate = null;
            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            row = result.First();
            row.date_hired.Should().Be(job.First().StartDate);

            // No hire date value set anywhere
            worker.HireDate = null;
            job.First().StartDate = null;
            assignment.First().StartDate = null;

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            row = result.First();
            row.date_hired.Should().BeNull();
        }

        [TestMethod]
        public async Task WorkerRequiredFields()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(1);
            var assignment = GenerateLaborAssignment(1);
            var workers = GenerateWorker(job, assignment).Generate(1);

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: assignment.Single(), job: job.Single());

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = workers[0].WorkerId,
                    Payee = workers[0],
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(1)),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                workers,
                checks
            );

            // set up failing conditions
            int failingCaseCount = 4;
            for (int i = 0; i < failingCaseCount; i++)
            {
                switch (i)
                {
                    case 0:
                        workers[0].Name.FamilyName = null;
                        break;

                    case 1:
                        workers[0].Name.FamilyName = "Placeholder";
                        workers[0].Name.GivenName = null;
                        break;

                    case 2:
                        workers[0].Name.FamilyName = "Placeholder";
                        workers[0].Name.GivenName = "Placeholder";
                        workers[0].WorkerId = null;
                        break;

                    case 3:
                        workers[0].Name.FamilyName = "Placeholder";
                        workers[0].Name.GivenName = "Placeholder";
                        workers[0].WorkerId = "Placeholder";
                        workers[0].EmployeeId = null;
                        break;

                    default:
                        break;
                }

                try
                {
                    var result = (await map.RetrieveFromApiAsync(
                        company.CompanyId,
                        pp.PayPeriodId,
                        job.Select(x => x.JobNumber.ToString())
                    )).ToList();

                    throw new Exception("This line should be unreachable given all workers are missing a required field");
                }
                catch (InvalidRecordException e)
                {
                    e.Message.Should().StartWith("The import failed due the following employees missing required fields");
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task MultipleAssignmentsJobsAndChecksSplitIntoPayRecordsAppropriately()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var comps = LoadTestData<ApiResponse<PayrollComponent>>("paycomponents.json").content;
            var pp = GeneratePayPeriod().Generate();
            var jobs = GenerateJob().Generate(2);
            var assignments = GenerateLaborAssignment(3);
            var workers = GenerateWorker(jobs, assignments).Generate(10);
            var taxes = LoadTestData<List<Tax>>("taxes.json");

            string GetComponentIdByName(string name) => comps.FirstOrDefault(x => x.Name == name)
                                                             .ComponentId;
            Earning CreateDeduction(Func<PayrollComponent, bool> predicate, decimal amount, Job job = null, LaborAssignment assignment = null) =>
                CreateEarningFromComponent(
                    comps.FirstOrDefault(predicate),
                    amount,
                    job,
                    assignment
                );
            Check CreateCheck(Worker w,
                              List<Earning> earnings = null,
                              List<Earning> deductions = null,
                              List<Tax> tax = null) =>
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = w.WorkerId,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Deductions = deductions ?? new List<Earning>(),
                    Earnings = earnings ?? new List<Earning>(),
                    Taxes = tax ?? new List<Tax>(),
                    Payee = w,
                    PayPeriod = pp
                };

            SetupDbMapping(
                context,
                company,
                new Dictionary<string, FieldType>
                {
                    { Hourly.ComponentId, FieldType.BaseHourlyRate },
                    { OverTime.ComponentId, FieldType.OvertimeHourlyRate },
                    { DoubleOverTime.ComponentId, FieldType.DoubletimeHourlyRate },
                    { GetComponentIdByName("401k EE Pretax"), FieldType.DeductionOther },
                    { GetComponentIdByName("Parking EE Paid"), FieldType.DeductionOther },
                    { GetComponentIdByName("Union dues"), FieldType.DeductionVacationDues },
                    { GetComponentIdByName("Deduction"), FieldType.DeductionOther },
                    { GetComponentIdByName("Med FSA ER"), FieldType.DeductionOther },
                    { GetComponentIdByName("401k Loan Payment"), FieldType.DeductionOther },
                    { GetComponentIdByName("POP EE Dom Partnr"), FieldType.DeductionOther },
                    { GetComponentIdByName("Dep Care EE Post Tax"), FieldType.DeductionOther },
                    { GetComponentIdByName("401k EE MilitaryMkup"), FieldType.DeductionOther },
                }
            );

            var checks = new List<Check>();
            var worker = workers[0];

            checks.Add(
                CreateCheck(
                    worker,
                    deductions: new List<Earning>
                    {
                        CreateDeduction(x => x.Name == "Union dues", 50, jobs[0], assignments[0]),
                        CreateDeduction(x => x.Name == "Deduction", 10, jobs[0], assignments[0]),
                        CreateDeduction(x => x.Name == "Med FSA ER", 25, jobs[0], assignments[0]),
                        CreateDeduction(x => x.Name == "Med FSA ER", 20, jobs[0], assignments[1]),
                        CreateDeduction(x => x.Name == "401k EE Pretax", 50, jobs[0], assignments[0]),
                        CreateDeduction(x => x.Name == "401k Loan Payment", 50, jobs[0], assignments[0]),
                    },
                    earnings: new List<Earning>
                    {
                        CreateEarningFromComponent(Hourly, hours:8, rate:50, job:jobs[0], assignment:assignments[0], date:pp.StartDate.AddDays(1)),
                        CreateEarningFromComponent(Hourly, hours:5, rate:50, job:jobs[0], assignment:assignments[0], date:pp.StartDate.AddDays(2)),
                        CreateEarningFromComponent(Hourly, hours:1, rate:40, job:jobs[0], assignment:assignments[1], date:pp.StartDate.AddDays(4)),
                    },
                    tax: new List<Tax>
                    {
                        CreateTax(taxes.FirstOrDefault(x=>x.Name == "Employee Medicare Tax"), 100, assignments[0].LaborAssignmentId, jobs[0].JobId)
                    }
                )
            );
            checks.Add(
                CreateCheck(
                    worker,
                    deductions: new List<Earning>
                    {
                        CreateDeduction(x=>x.Name == "401k EE Pretax", 10, jobs[1], assignments[0]),
                        CreateDeduction(x=>x.Name == "Parking EE Paid", 15, jobs[1], assignments[0]),
                        CreateDeduction(x=>x.Name == "POP EE Dom Partnr", 42, jobs[1], assignments[1]),
                    },
                    earnings: new List<Earning>
                    {
                        CreateEarningFromComponent(Hourly, hours:7, rate:50, job:jobs[1], assignment:assignments[1], date:pp.StartDate),
                        CreateEarningFromComponent(Hourly, hours:1, rate:25, job:jobs[1], assignment:assignments[2], date:pp.StartDate),
                        CreateEarningFromComponent(OverTime, hours:8, rate:50, job:jobs[1], assignment:assignments[1], date:pp.StartDate.AddDays(2)),
                        CreateEarningFromComponent(OverTime, hours:1, rate:100, job:jobs[1], assignment:assignments[1], date:pp.StartDate.AddDays(2)),
                    },
                    tax: new List<Tax>
                    {
                        CreateTax(taxes.FirstOrDefault(x=>x.Name == "Federal Income Tax"), 64, assignments[0].LaborAssignmentId, jobs[1].JobId)
                    }
                )
            );

            worker = workers[1];
            checks.Add(
                CreateCheck(
                    worker,
                    deductions: new List<Earning>
                    {
                        CreateDeduction(x => x.Name == "Deduction", 5, jobs[0], assignments[1]),
                        CreateDeduction(x => x.Name == "Dep Care EE Post Tax", 24, jobs[1], assignments[0]),
                        CreateDeduction(x => x.Name == "401k EE MilitaryMkup", 2, jobs[1],assignments[1]),
                        CreateDeduction(x => x.Name == "401k Loan Payment", 57, jobs[0], assignments[1]),
                    },
                    earnings: new List<Earning>
                    {
                        CreateEarningFromComponent(Hourly, hours:1, rate:10, job:jobs[0], assignment:assignments[1], date:pp.StartDate.AddDays(3)),
                        CreateEarningFromComponent(Hourly, hours:7, rate:11, job:jobs[0], assignment:assignments[0], date:pp.StartDate.AddDays(5)),
                        CreateEarningFromComponent(Hourly, hours:3, rate:12, job:jobs[0], assignment:assignments[0], date:pp.StartDate.AddDays(4)),
                        CreateEarningFromComponent(DoubleOverTime, hours:2, rate:48, job:jobs[0], assignment:assignments[0], date:pp.StartDate.AddDays(4)),
                    },
                    tax: new List<Tax>
                    {
                        CreateTax(taxes.FirstOrDefault(x=>x.Name == "Re-employment Service Fund"), 100, assignments[0].LaborAssignmentId, jobs[0].JobId)
                    }
                )
            );
            checks.Add(
                CreateCheck(
                    worker,
                    deductions: new List<Earning>
                    {
                        CreateDeduction(x=>x.Name == "401k EE Pretax", 10, jobs[1], assignments[0]),
                        CreateDeduction(x=>x.Name == "Parking EE Paid", 15, jobs[1], assignments[0]),
                        CreateDeduction(x=>x.Name == "POP EE Dom Partnr", 42, jobs[1], assignments[1]),
                    },
                    earnings: new List<Earning>
                    {
                        CreateEarningFromComponent(Hourly, hours:7, rate:50, job:jobs[1], assignment:assignments[1], date:pp.StartDate),
                        CreateEarningFromComponent(Hourly, hours:1, rate:25, job:jobs[1], assignment:assignments[2], date:pp.StartDate),
                    },
                    tax: new List<Tax>
                    {
                        CreateTax(taxes.FirstOrDefault(x=>x.Name == "Federal Income Tax"), 64, assignments[0].LaborAssignmentId, jobs[1].JobId)
                    }
                )
            );

            SetupResponses(
                clientMock,
                company,
                pp,
                assignments,
                jobs,
                comps,
                new List<JobTitle>(),
                workers,
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                jobs.Select(x => x.ToString())
            )).ToList();

            result.Should().HaveCount(12);
            result[0].ProjectCode.Should().Be(jobs[0].ToString());
            result[0].ssn.Should().Be(workers[0].LegalId.LegalIdValue);
            result[0].craft.Should().Be(assignments[0].Name);
            result[0].st_hrs_date2.Should().Be(8);
            result[0].st_hrs_date3.Should().Be(5);
            result[0].dts_dues.Should().Be(50);
            result[0].dts_other.Should().Be(110);
            result[0].ep_haw.Should().Be(25);
            result[0].period_end_date.Should().Be(pp.EndDate);
            result[0].ep_hrate.Should().Be(50);
            result[0].TotalStandardHours.Should().Be(13);
            result[0].TotalsForThisPeriod.Should().Be(50 * 13);
            result[0].dts_medicare.Should().Be(100);

            result[1].ssn.Should().Be(workers[0].LegalId.LegalIdValue);
            result[1].ProjectCode.Should().Be(jobs[0].ToString());
            result[1].craft.Should().Be(assignments[1].Name);
            result[1].st_hrs_date5.Should().Be(1);
            result[1].TotalStandardHours.Should().Be(1);
            result[1].ep_haw.Should().Be(20);
            result[1].TotalsForThisPeriod.Should().Be(40);

            result[2].ProjectCode.Should().Be(jobs[1].ToString());
            result[2].ssn.Should().Be(workers[0].LegalId.LegalIdValue);
            result[2].craft.Should().Be(assignments[0].Name);
            result[2].st_hrs_date1.Should().Be(7);
        }

        [TestMethod]
        public async Task StateTaxLinesPickedUpAndBucketedCorrectly()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var comps = LoadTestData<ApiResponse<PayrollComponent>>("paycomponents.json").content;
            var pp = GeneratePayPeriod().Generate();
            var jobs = GenerateJob().Generate(1);
            var assignments = GenerateLaborAssignment(1);
            var workers = GenerateWorker(jobs, assignments).Generate(1);
            var taxes = LoadTestData<List<Tax>>("taxes.json");

            SetupDefaultDbMapping(context, company);

            var worker = workers.First();

            Tax Tax(Func<Tax, bool> predicate, decimal amount) =>
                CreateTax(
                    taxes.FirstOrDefault(predicate),
                    amount,
                    faker.PickRandom(assignments).LaborAssignmentId,
                    faker.PickRandom(jobs).JobId
                );
            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date) =>
               CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: assignments.Single(), job: jobs.Single());

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId().Generate().Id,
                    CheckNumber = "1000",
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(1)),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(2)),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(3)),
                    },
                    Taxes = new List<Tax>
                    {
                        Tax(x => x.Name == "New York Income Tax", 99),
                        Tax(x => x.Name == "New York Disability Insurance", 55),
                    },
                    Payee = worker,
                    PayPeriod = pp
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignments,
                jobs,
                comps,
                new List<JobTitle>(),
                workers,
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                jobs.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            var row = result.Single();
            row.dts_state_tax.Should().Be(99);
            row.dts_sdi.Should().Be(55);
        }

        [TestMethod]
        public async Task TaxIgnoreEmployerLiability()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var comps = LoadTestData<ApiResponse<PayrollComponent>>("paycomponents.json").content;
            var pp = GeneratePayPeriod().Generate();
            var jobs = GenerateJob().Generate(1);
            var assignments = GenerateLaborAssignment(1);
            var workers = GenerateWorker(jobs, assignments).Generate(1);
            var taxes = LoadTestData<List<Tax>>("taxes.json");

            SetupDefaultDbMapping(context, company);

            var worker = workers.First();

            Tax Tax(Func<Tax, bool> predicate, decimal amount) =>
                CreateTax(
                    taxes.FirstOrDefault(predicate),
                    amount,
                    faker.PickRandom(assignments).LaborAssignmentId,
                    faker.PickRandom(jobs).JobId
                );
            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date) =>
               CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: assignments.Single(), job: jobs.Single());

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId().Generate().Id,
                    CheckNumber = "1000",
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(1)),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(2)),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(3)),
                    },
                    Taxes = new List<Tax>
                    {
                        Tax(x => x.PaidBy == Lcp.Paychex.Models.Payroll.Tax.TaxPaidBy.EMPLOYER_LIABILITY, 999),
                        Tax(x => x.Name == "New York Income Tax", 10),
                        Tax(x => x.Name == "Federal Income Tax", 100),
                        Tax(x => x.Name == "New York Disability Insurance", 1000)
                    },
                    Payee = worker,
                    PayPeriod = pp
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignments,
                jobs,
                comps,
                new List<JobTitle>(),
                workers,
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                jobs.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            var row = result.Single();
            row.dts_state_tax.Should().Be(10);
            row.dts_fed_tax.Should().Be(100);
            row.dts_sdi.Should().Be(1000);
        }

        [Ignore]
        [TestMethod]
        public void NetPayMatchesAmountsFromCheckLines()
        {
            Assert.Inconclusive("Not finished");
        }

        [Ignore]
        [TestMethod]
        public void BiWeeklyPayPeriodSeparatesToMultiplePayRecords()
        {
            Assert.Inconclusive("Not finished");
        }

        [Ignore]
        [TestMethod]
        public void TotalAllProjectsMatchesExpected()
        {
            Assert.Inconclusive("Not finished");
        }

        /// <summary>
        ///*Purpose: If employee works on multiple projects then Gross pay all projects should be
        ///  summation of all projects amount and WagesPaidInLieuFringes amount
        ///*Update to AssignSummations (Story 47329): Sum of TotalsPerProject now handles multiple
        ///  crafts and projects(Different payroll records) in the same pay period.
        /// </summary>
        [TestMethod]
        public async Task GrossPayAllProjectscalcOnMultipleProjects()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(2);
            var assignment = GenerateLaborAssignment(2);
            var worker = GenerateWorker(null, assignment).Generate();

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date, LaborAssignment laborAssignment, Job assignedJob) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: laborAssignment, job: assignedJob);

            Earning CreateFringeComponent(PayrollComponent fringe, decimal? amount, decimal rate, LaborAssignment laborAssingment, Job assignedjob) =>
                CreateEarningFromComponent(fringe, amount: amount, job: assignedjob, assignment: laborAssingment, date: null, hours: null, rate: rate);

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    Payee = worker,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate, assignment.First(), job[0]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(1), assignment.First(), job[0]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(2), assignment.First(), job[1]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(3), assignment.First(), job[1]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(4), assignment.Last(), job[1]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(5), assignment.Last(), job[1]),
                        CreateFringeComponent(Fringe, 3, 0.11M, assignment.First(), null),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(3);
            result.First().all_projects.Should().Be(453.96M);
            result.First().wages_paid_in_lieu_of_fringes.Should().Be(1.32M);
        }

        /// <summary>
        /// If there is no work hours associated to the labor assignment, then do not create a payrecord without hours US 46662
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DonotCreateRecordWithnoWorkHours()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var jobs = GenerateJob().Generate(2);
            var assignments = GenerateLaborAssignment(1);
            var workers = GenerateWorker(jobs, assignments).Generate(2);

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date, Job assignedJob) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: assignments.First(), job: assignedJob);

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = workers[0].WorkerId,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId().Generate().Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                         CreateHourly(Hourly, 8, 12.5m, pp.StartDate, jobs[0]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(1), jobs[0]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(2), jobs[0]),
                    }
                },
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = workers[1].WorkerId,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId().Generate().Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular"
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignments,
                jobs,
                Components,
                new List<JobTitle>(),
                workers,
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                jobs.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);

            var row = result.First();
            row.TotalHours.Should()
               .Be(20);
        }

        /// <summary>
        ///purpose: If no JobId and no work hours then assign mapped fringe amounts to each project.
        /// </summary>
        [TestMethod]
        public async Task AssignNoJobIdfringeamountstoAllassociatedPayrecords()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(2);
            var assignment = GenerateLaborAssignment(3);
            var worker = GenerateWorker(null, assignment).Generate();

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date, LaborAssignment laborAssignment, Job assignedJob) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: laborAssignment, job: assignedJob);

            Earning CreateFringeComponent(PayrollComponent fringe, decimal? amount, decimal rate, LaborAssignment laborAssingment, Job assignedjob) =>
                CreateEarningFromComponent(fringe, amount: amount, job: assignedjob, assignment: laborAssingment, date: null, hours: null, rate: rate);
            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    Payee = worker,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(2), assignment.First(), job[0]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(3), assignment.First(), job[0]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(4), assignment.Last(), job[0]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(5), assignment.Last(), job[0]),
                        CreateFringeComponent(Fringe, 6.5M, 0.22M, assignment[1],  null),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(2);
            foreach (var row in result)
            {
                row.wages_paid_in_lieu_of_fringes.Should().Be(2.64M);
                row.rate_in_lieu_of_fringes.Should().Be(0.22M);
            }
        }

        /// <summary>
        ///purpose: Generate a separate payrecords when an employee has multiple earning rates of pay for same Labor Assignment and same job
        /// </summary>
        [TestMethod]
        public async Task GenerateMultiplePayrecordsWhenChangeInEarningRates()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(2);
            var assignment = GenerateLaborAssignment(3);
            var worker = GenerateWorker(null, assignment).Generate();

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date, LaborAssignment laborAssignment, Job assignedJob) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: laborAssignment, job: assignedJob);

            Earning CreateFringeComponent(PayrollComponent fringe, decimal? amount, decimal rate, LaborAssignment laborAssingment, Job assignedjob) =>
                CreateEarningFromComponent(fringe, amount: amount, job: assignedjob, assignment: laborAssingment, date: null, hours: null, rate: rate);
            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    Payee = worker,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate, assignment.First(), job[0]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(1), assignment.First(), job[0]),
                        CreateHourly(Hourly, 2, 12.5m, pp.StartDate.AddDays(2), assignment.First(), job[0]),
                        CreateHourly(Hourly, 6, 25, pp.StartDate.AddDays(3), assignment.First(), job[0]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(5), assignment.First(), job[1]),
                        CreateHourly(Hourly, 5, 12.5m, pp.StartDate.AddDays(6), assignment.Last(), job[1]),
                        CreateFringeComponent(Fringe, 6.5M, 0.22M, assignment[1],  null),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(4);
            result.Where(r => r.ProjectCode == job[0].JobNumber.ToString()).ToList().Count().Should().Be(2);
        }

        /// <summary>
        /// Calculate cash fringe rate depending on the cash fringe amount
        /// </summary>
        [TestMethod]
        public async Task AssignCashFringesIfNofringeRate()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(2);
            var assignment = GenerateLaborAssignment(3);
            var worker = GenerateWorker(null, assignment).Generate();

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date, LaborAssignment laborAssignment, Job assignedJob) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: laborAssignment, job: assignedJob);

            Earning CreateFringeComponent(PayrollComponent fringe, decimal? amount, decimal rate, LaborAssignment laborAssingment, Job assignedjob) =>
                CreateEarningFromComponent(fringe, amount: amount, job: assignedjob, assignment: laborAssingment, date: null, hours: null, rate: rate);
            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    Payee = worker,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateFringeComponent(Fringe, 1.76M, 0, assignment.First(),  job[0]),
                        CreateFringeComponent(Fringe, 6, 0, assignment.Last(),  job[1]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(2), assignment.First(), job[0]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(3), assignment.First(), job[0]),
                        CreateHourly(Hourly, 8, 12.5m, pp.StartDate.AddDays(4), assignment.Last(), job[1]),
                        CreateHourly(Hourly, 4, 12.5m, pp.StartDate.AddDays(5), assignment.Last(), job[1]),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(2);
            result.First().rate_in_lieu_of_fringes.Should().Be(0.11M);
            result.Last().rate_in_lieu_of_fringes.Should().Be(0.5M);
        }

        /// <summary>
        /// Tests that fringes are assigned correctly.
        /// TODO add rest of fringes: ep_vac_hol, ep_haw, ep_pension, dts_fund_admin, ep_train
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AssignFringes()
        {
            var (map, clientMock, context, faker) = CreateTestObjects();

            var company = GenerateCompany().Generate();
            var pp = GeneratePayPeriod().Generate();
            var job = GenerateJob().Generate(1);
            var assignment = GenerateLaborAssignment(1);
            var worker = GenerateWorker(job, assignment).Generate();

            SetupDefaultDbMapping(context, company);

            Earning CreateHourly(PayrollComponent hourly, decimal hours, decimal rate, DateTime date, LaborAssignment laborAssignment, Job assignedJob) =>
                CreateEarningFromComponent(hourly, hours: hours, rate: rate, date: date, assignment: laborAssignment, job: assignedJob);

            Earning CreateFringeComponent(PayrollComponent fringe, decimal? amount, decimal rate, LaborAssignment laborAssingment, Job assignedjob) =>
                CreateEarningFromComponent(fringe, amount: amount, job: assignedjob, assignment: laborAssingment, date: null, hours: null, rate: rate);

            var checks = new List<Check>()
            {
                new Check
                {
                    BlockAutoDistribution = false,
                    WorkerId = worker.WorkerId,
                    Payee = worker,
                    CheckDate = pp.CheckDate,
                    PayPeriodId = pp.PayPeriodId,
                    PaycheckId = GeneratePaychexId()
                                 .Generate()
                                 .Id,
                    CheckNumber = faker.Random.ReplaceNumbers("####"),
                    CheckType = "Regular",
                    Earnings = new List<Earning>
                    {
                        CreateHourly(Hourly, 5, 41.68M, pp.StartDate.AddDays(0), assignment.First(), job.First()),
                        CreateHourly(Hourly, 5, 41.68M, pp.StartDate.AddDays(1), assignment.First(), job.First()),
                        CreateHourly(Hourly, 5, 41.68M, pp.StartDate.AddDays(2), assignment.First(), job.First()),
                        CreateHourly(Hourly, 5, 41.68M, pp.StartDate.AddDays(3), assignment.First(), job.First()),
                        CreateFringeComponent(Fringe, 486.8M, 24.34M, assignment.First(),  job.First()),
                    }
                }
            };

            SetupResponses(
                clientMock,
                company,
                pp,
                assignment,
                job,
                Components,
                new List<JobTitle>(),
                new[] { worker },
                checks
            );

            var result = (await map.RetrieveFromApiAsync(
                company.CompanyId,
                pp.PayPeriodId,
                job.Select(x => x.JobNumber.ToString())
            )).ToList();

            result.Should().HaveCount(1);
            result.First().rate_in_lieu_of_fringes.Should().Be(24.34M);
            result.First().wages_paid_in_lieu_of_fringes.Should().Be(486.8M);
        }
    }
}