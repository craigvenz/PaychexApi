// LcpMvc.Lcp.Services.Tests.TestBase.cs
// Craig Venz - 09/26/2019 - 11:20 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Idioms;
using Bogus;
using FluentAssertions;
using Lcp.Paychex.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lcp.Services.Tests.Paychex 
{
    public class TestBase
    {
        protected int Seed;

        // after having issues with tests intermittently failing due to the random data being generated
        // from faker being sometimes fine, sometimes not, I am now going to do this everywhere I use faker.
        // Declare the random seed ahead of time, initialize faker's Random instance with it, and output the
        // seed to the console. When an intermittent test failure occurs, we can see what the seed was,
        // set it manually, and reproduce the issue.
        // In [TestInitialize] in derived classes, call SetupRandom() with your reproduction seed.
        public void SetupRandom(int seed, string reason = "")
        {
            Seed = seed;
            Randomizer.Seed = new Random(Seed);
            var reasonString = (!string.IsNullOrEmpty(reason) ? $" because {reason}" : "");
            Trace.TraceInformation(
$@"SetupRandom called at: {DateTime.Now:yyyy'-'MM'-'dd' 'HH':'mm':'ss.fffffff}{reasonString}
Random seed is: {Seed}
");
        }

        [TestInitialize]
        public void CommonTestInitialization()
        {
            // This is apparently what the default constructor for Random does.
            // There's no way to look at the seed of a Random object so this is the work around.
            SetupRandom(Environment.TickCount);
        }
        
        protected static Mock<T> CreateTestMock<T>([CallerMemberName] string caller = "") where T : class =>
            new Mock<T> { Name = $"{caller} - {typeof(T).Name} mock" };

        protected static ApiResponse<T> CreateResponse<T>(IEnumerable<T> item)
        {
            return item==null ? new ApiResponse<T>() : new ApiResponse<T>(item);
        }
        /// <summary>
        /// https://stackoverflow.com/a/53004985/223942
        /// </summary>
        /// <param name="pathRelativeUnitTestingFile"></param>
        /// <returns></returns>
        public static string GetFullPathToFile(string pathRelativeUnitTestingFile)
        {
            var folderProjectLevel = GetPathToCurrentUnitTestProject();
            var final = System.IO.Path.Combine(folderProjectLevel, pathRelativeUnitTestingFile);
            return final;
        }
        private static string GetPathToCurrentUnitTestProject()
        {
            var pathAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var folderAssembly = System.IO.Path.GetDirectoryName(pathAssembly);
            if (folderAssembly?.EndsWith("\\") == false) folderAssembly += "\\";
            var folderProjectLevel = System.IO.Path.GetFullPath(folderAssembly + "..\\..\\");
            return folderProjectLevel;
        }

        protected static void ExerciseProperties<T>(IPostprocessComposer<T> composer)
        {
            var writable = new WritablePropertyAssertion(composer);
            foreach (var p in typeof(T).GetProperties())
            {
                Action action = () => writable.Verify(p);
                action.Should()
                      .NotThrow<WritablePropertyException>();
            }
        }

        protected static void TestCoverageOf<T>()
        {
            var f = new Fixture();
            ExerciseProperties(f.Build<T>());
        }

        protected static void TestHashCodes<T>()
        {
            var f = new Fixture();
            Action act = () => new GetHashCodeSuccessiveAssertion(
                             f.Build<T>()
                         ).Verify(
                             typeof(T).GetMethod("GetHashCode")
                         );

            act.Should()
               .NotThrow<GetHashCodeOverrideException>();
        }
    }
}