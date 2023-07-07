// LcpMvc.Lcp.Services.Tests.PaychexDataGenerator.cs
// Craig Venz - 09/26/2019 - 4:41 PM

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bogus;
using Lcp.Paychex.Models.Common;
using Lcp.Paychex.Models.Companies;
using Lcp.Paychex.Models.Payroll;
using Lcp.Paychex.Models.Workers;
using Newtonsoft.Json;
using BaseCommunication = Lcp.Paychex.Models.Common.Communication;
using WorkerCommunication = Lcp.Paychex.Models.Workers.Communication;
using CompanyCommunication = Lcp.Paychex.Models.Companies.Communication;
using PayPeriodInterval = Lcp.Paychex.Models.Payroll.PayPeriod.IntervalCodes;

namespace Lcp.Services.Tests.Paychex
{
    public class PaychexId
    {
        public string Id { get; set; }
    }

    public class PaychexSubId
    {
        public string Id { get; set; }
    }

    public static class PaychexDataGenerator
    {
        private static readonly string[] CraftList = JsonConvert.DeserializeObject<string[]>(
            File.ReadAllText(TestBase.GetFullPathToFile("paychex\\data\\crafts.json"))
        );

        public static Faker<PaychexId> GeneratePaychexId() =>
            new Faker<PaychexId>().Rules(
                (f, o) =>
                {
                    o.Id = GenerateTopLevelPaychexId(f);
                }
            );

        public static Faker<PaychexSubId> GeneratePaychexSubId() =>
            new Faker<PaychexSubId>().Rules(
                (f, o) =>
                {
                    o.Id = GenerateSubObjectPaychexId(f);
                }
            );

        private static string GenerateTopLevelPaychexId(Faker f) =>
            $"00{f.Random.AlphaNumeric(18).ToUpperInvariant()}";

        private static string GenerateSubObjectPaychexId(Faker f) =>
            f.Random.ReplaceNumbers("10##############");

        public static Faker<PayPeriod> GeneratePayPeriod(
            PayPeriodInterval interval = PayPeriodInterval.WEEKLY,
            PayPeriod.PayPeriodStatus status = PayPeriod.PayPeriodStatus.COMPLETED,
            Action<Faker, PayPeriod> actions = null
        ) =>
            new Faker<PayPeriod>().Rules(
                (f, o) =>
                {
                    o.IntervalCode = interval;
                    o.Status = status;
                    o.PayPeriodId = GenerateSubObjectPaychexId(f);
                    o.Description = f.Random.Words(2);
                    o.CheckCount = f.Random.Int(1, 10);
                    var dt = f.Date.Between(
                        new DateTime(
                            DateTime.Today.Year,
                            DateTime.Today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        ),
                        f.Date.Recent(14, DateTime.UtcNow.Date)
                    );
                    dt = dt.AddDays(-((int)dt.DayOfWeek - 1));
                    o.StartDate = dt.Date;
                    o.EndDate = o.StartDate.AddDays(
                        interval == PayPeriodInterval.BI_WEEKLY ? 13 : 6
                    );
                    o.CheckDate = o.EndDate.AddDays(7 + (DayOfWeek.Friday - o.EndDate.DayOfWeek));
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Company> GenerateCompany(string companyId = "", Action<Faker, Company> actions = null) =>
            new Faker<Company>().Rules(
                (f, o) =>
                {
                    o.CompanyId = string.IsNullOrEmpty(companyId)
                        ? GenerateTopLevelPaychexId(f)
                        : companyId;
                    o.DisplayId = f.Random.ReplaceNumbers("#######");
                    o.LegalName = f.Company.CompanyName();
                    o.LegalId = new LegalId
                    {
                        LegalIdType = LegalIdType.FEIN,
                        LegalIdValue = f.Random.ReplaceNumbers("##-#######")
                    };
                    o.Communications = new List<CompanyCommunication>
                    {
                        new CompanyCommunication(
                            GenerateAddress()
                                .Generate()
                        )
                    };
                    o.Links = new List<Link>
                    {
                        new Link
                        {
                            Rel = "self", Href = $"file://companies/{companyId}"
                        }
                    };
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<BaseCommunication> GenerateAddress(Action<Faker, BaseCommunication> actions = null) =>
            new Faker<BaseCommunication>().Rules(
                (f, o) =>
                {
                    o.CommunicationId = GenerateTopLevelPaychexId(f);
                    o.Type = ContactType.STREET_ADDRESS;
                    o.UsageType = f.Random.Enum<ContactUsageType>();
                    o.City = f.Address.City();
                    o.StreetLineOne = f.Address.StreetAddress();
                    o.PostalCode = f.Address.ZipCode();
                    o.CountryCode = "US";
                    o.CountrySubdivisionCode = f.Address.StateAbbr();
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<WorkerCommunication> GenerateEmail(string fn,
                                                               string ln,
                                                               Action<Faker, WorkerCommunication> actions = null) =>
            new Faker<WorkerCommunication>().Rules(
                (f, o) =>
                {
                    o.CommunicationId = GenerateTopLevelPaychexId(f);
                    o.Type = ContactType.EMAIL;
                    o.UsageType = f.Random.Enum<ContactUsageType>();
                    o.Uri = f.Internet.Email(fn, ln);
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<WorkerCommunication>
            GeneratePhoneNumber(Action<Faker, WorkerCommunication> actions = null) =>
            new Faker<WorkerCommunication>().Rules(
                (f, o) =>
                {
                    o.CommunicationId = GenerateTopLevelPaychexId(f);
                    o.Type = ContactType.PHONE;
                    o.UsageType = f.Random.Enum<ContactUsageType>();
                    o.DialArea = f.Random.ReplaceNumbers("###");
                    o.DialNumber = f.Random.ReplaceNumbers("###-####");
                    o.DialCountry = "1";
                    o.DialExtension =
                        f.Random.Number(10) % 5 == 0
                            ? f.Random.ReplaceNumbers("x####")
                            : string.Empty;
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Name> GenerateName(Action<Faker, Name> actions = null) =>
            new Faker<Name>().Rules(
                (f, o) =>
                {
                    o.GivenName = f.Name.FirstName();
                    o.FamilyName = f.Name.LastName();
                    o.MiddleName =
                        f.Random.Number(10) % 5 == 0
                            ? f.Name.FirstName()
                            : null;
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Earning> GenerateEarning(
            DateTime? lineDate,
            PayrollComponent comp,
            LaborAssignment assignment,
            Job job,
            Action<Faker, Earning> actions = null
        ) =>
            new Faker<Earning>().Rules(
                (f, o) =>
                {
                    o.CheckComponentId = GenerateSubObjectPaychexId(f);
                    o.ComponentId = comp.ComponentId;
                    o.EffectOnPay = comp.EffectOnPay;
                    o.ClassificationType = comp.ClassificationType;
                    o.Name = comp.Name;
                    if (PayrollNameConstants.IsHourly(comp.Name))
                    {
                        o.Rate = Math.Floor(f.Random.Decimal(1, 100));
                        o.Hours = decimal.Round(f.Random.Decimal(0.5m, 12), 1);
                        o.Amount = o.Rate * o.Hours;
                        o.LineDate = lineDate;
                    }
                    else
                    {
                        o.Amount = decimal.Round(f.Random.Decimal(0, 1000), 2);
                    }

                    o.JobId = job.JobId;
                    o.LaborAssignmentId = assignment.LaborAssignmentId;
                    o.Organization = assignment.Organization;
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Earning> GenerateDeduction(
            PayrollComponent component,
            LaborAssignment assignment,
            Job job,
            Action<Faker, Earning> actions = null
        ) =>
            new Faker<Earning>().Rules(
                (f, o) =>
                {
                    o.CheckComponentId = GenerateSubObjectPaychexId(f);
                    o.ComponentId = component.ComponentId;
                    o.EffectOnPay = component.EffectOnPay;
                    o.ClassificationType = component.ClassificationType;
                    o.Name = component.Name;
                    o.JobId = job.JobId;
                    o.LaborAssignmentId = assignment.LaborAssignmentId;
                    o.Organization = assignment.Organization;
                    o.Amount = decimal.Round(f.Random.Decimal(0, 1000), 2);
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Tax> GenerateTax(
            Tax tax,
            LaborAssignment assignment,
            Action<Faker, Tax> actions = null
        ) =>
            new Faker<Tax>().Rules(
                (f, o) =>
                {
                    o.Name = tax.Name;
                    o.PaidBy = tax.PaidBy;
                    o.LaborAssignmentId = assignment.LaborAssignmentId;
                    o.Amount = decimal.Round(f.Random.Decimal(0, 1000), 2);
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Check> GenerateCheck(PayPeriod payPeriod,
                                                 IEnumerable<string> workerIds,
                                                 IEnumerable<PayrollComponent> components,
                                                 IEnumerable<LaborAssignment> assignments,
                                                 IEnumerable<Job> jobs,
                                                 IEnumerable<Tax> taxes,
                                                 Action<Faker, Check> actions = null
        ) =>
            new Faker<Check>().Rules(
                (f, o) =>
                {
                    o.BlockAutoDistribution = false;
                    o.WorkerId = f.PickRandom(workerIds);
                    o.CheckDate = payPeriod.CheckDate;
                    o.PayPeriodId = payPeriod.PayPeriodId;
                    o.PaycheckId = GenerateTopLevelPaychexId(f);
                    o.CheckNumber = f.Random.ReplaceNumbers("####");
                    o.CheckType = "Regular";

                    var payrollComponents = components as PayrollComponent[] ?? components.ToArray();

                    o.Earnings = new List<Earning>();
                    var hourly = payrollComponents.FirstOrDefault(x => x.Name == PayrollNameConstants.Hourly);
                    var ot = payrollComponents.FirstOrDefault(x => x.Name == PayrollNameConstants.OverTime);
                    var ot2 = payrollComponents.FirstOrDefault(x => x.Name == PayrollNameConstants.DoubleOverTime);
                    var laborAssignments = assignments as LaborAssignment[] ?? assignments.ToArray();
                    var jobList = jobs as Job[] ?? jobs.ToArray();
                    for (var dt = payPeriod.StartDate; dt <= payPeriod.EndDate; dt = dt.AddDays(1))
                    {
                        void Generate(PayrollComponent payrollComponent) => o.Earnings.Add(
                            GenerateEarning(
                                    dt,
                                    payrollComponent,
                                    f.PickRandom(
                                        laborAssignments
                                    ),
                                    f.PickRandom(jobList)
                                )
                                .Generate()
                        );

                        if (f.Random.Bool())
                            Generate(hourly);
                        if (f.Random.Int(10) % 5 == 0)
                            Generate(ot);
                        if (f.Random.Int(10) % 7 == 0)
                            Generate(ot2);
                    }

                    foreach (var c in f.Random.Shuffle(
                        from c in payrollComponents
                        where !PayrollNameConstants.IsHourly(c.Name)
                              && f.Random.Bool()
                        select c
                    ))
                    {
                        o.Earnings.Add(
                            GenerateEarning(null, c, f.PickRandom(laborAssignments), f.PickRandom(jobList))
                                .Generate()
                        );
                    }

                    o.Deductions = payrollComponents.Where(
                                                        x => x.EffectOnPay == EffectOnPayType.REDUCTION
                                                             && f.Random.Bool()
                                                    )
                                                    .Select(
                                                        x => GenerateDeduction(
                                                                x,
                                                                f.PickRandom(laborAssignments),
                                                                f.PickRandom(jobList)
                                                            )
                                                            .Generate()
                                                    )
                                                    .ToList();
                    o.Taxes = taxes.Where(_ => f.Random.Int(10) % 5 == 0)
                                   .Select(
                                       x => GenerateTax(x, f.PickRandom(laborAssignments))
                                           .Generate()
                                   )
                                   .ToList();
                    o.NetPay = o.Earnings.Sum(x => x.Amount)
                               - o.Deductions.Sum(x => x.Amount)
                               - o.Taxes.Sum(x => x.Amount);
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Job> GenerateJob(Action<Faker, Job> actions = null) =>
            new Faker<Job>().Rules(
                (f, o) =>
                {
                    o.JobId = GenerateSubObjectPaychexId(f);
                    o.JobName = f.Company.CompanyName();
                    o.StartDate = f.Date.Past(28);
                    o.JobNumber = new JobNumber
                    {
                        Segment1 = f.Random.AlphaNumeric(3)
                    };
                    actions?.Invoke(f, o);
                }
            );

        public static Faker<Organization> GenerateOrganization(Action<Faker, Organization> actions = null) =>
            new Faker<Organization>().Rules(
                (f, o) =>
                {
                    o.OrganizationId = GenerateSubObjectPaychexId(f);
                    o.Number = f.Finance.Iban();
                    o.Name = f.Company.CompanyName();
                    o.Level = "1";
                    actions?.Invoke(f, o);
                }
            );

        public static List<LaborAssignment> GenerateLaborAssignment(
            int count,
            Action<Faker, LaborAssignment> actions = null)
        {
            var craftList = new List<string>();
            var faker = new Faker<LaborAssignment>().Rules(
                (f, o) =>
                {
                    o.LaborAssignmentId = GenerateSubObjectPaychexId(f);
                    o.Name = f.PickRandom(CraftList.Where(c => !craftList.Contains(c)));
                    craftList.Add(o.Name);
                    o.Organization = GenerateOrganization()
                        .Generate();
                    o.OrganizationId = o.Organization.OrganizationId;
                    o.LocationId = GenerateSubObjectPaychexId(f);
                    o.PositionId = GenerateSubObjectPaychexId(f);
                    o.StartDate = f.Date.Recent(180);
                    actions?.Invoke(f, o);
                });
            return faker.Generate(count);
        }

        public static Faker<Worker> GenerateWorker(IEnumerable<Job> jobs, IEnumerable<LaborAssignment> assignments,
                                                   Action<Faker, Worker> actions = null) =>
            new Faker<Worker>()
                .Rules(
                    (f, o) =>
                    {
                        o.WorkerId = GenerateTopLevelPaychexId(f);
                        o.Name = GenerateName()
                            .Generate();
                        o.BirthDate = f.Date.Between(
                                           new DateTime(1970, 1, 1),
                                           new DateTime(1995, 12, 31)
                                       )
                                       .Date;
                        o.HireDate = f.Date.Past(2);
                        o.CurrentStatus = new CurrentStatus
                        {
                            WorkerStatusId = GenerateTopLevelPaychexId(f),
                            StatusType = f.Random.Enum<WorkerStatusType>(),
                            StatusReason = f.Random.Enum<WorkerStatusReason>(),
                            EffectiveDate = f.Date.Between(
                                                 new DateTime(2000, 1, 1),
                                                 DateTime.Today
                                             )
                                             .Date
                        };
                        o.EmployeeId = f.Random.ReplaceNumbers("#########");
                        o.EmploymentType = f.Random.Enum<WorkerEmploymentType>();
                        o.EthnicityCode = f.Random.Enum<EthnicityType>();
                        o.Sex = f.Random.Enum<Gender>();
                        o.ExemptionType = EmployeeExemptionType.NON_EXEMPT;
                        o.LegalId = new LegalId
                        {
                            LegalIdType = LegalIdType.SSN,
                            LegalIdValue = f.Random.ReplaceNumbers("###-##-####")
                        };
                        o.Communications = new List<WorkerCommunication>
                        {
                            new WorkerCommunication(
                                GenerateAddress()
                                    .Generate()
                            ),
                            GenerateEmail(o.Name.GivenName, o.Name.FamilyName)
                                .Generate(),
                            GeneratePhoneNumber()
                                .Generate()
                        };
                        foreach (var c in o.Communications)
                        {
                            c.Links = new List<Link>
                            {
                                new Link
                                {
                                    Rel = "self",
                                    Href = $"file://workers/{o.WorkerId}/communications/{c.CommunicationId}"
                                }
                            };
                        }

                        o.HomeAssignment = assignments != null ? f.PickRandom(assignments) : null;
                        o.HomeJob = jobs != null ? f.PickRandom(jobs) : null;
                        o.JobId = o.HomeJob?.JobId;
                        o.LaborAssignmentId = o.HomeAssignment?.LaborAssignmentId;
                        actions?.Invoke(f, o);
                    }
                );
    }
}