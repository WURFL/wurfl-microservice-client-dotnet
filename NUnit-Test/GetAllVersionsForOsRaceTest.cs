using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wmclient;

namespace NUnit_Test
{
    /// <summary>
    /// Checks that GetAllVersionsForOS is thread-safe. If the test fails, this method may
    /// have a race condition.
    /// Requires a live server (UNITTEST_REMOTE_HOST / UNITTEST_REMOTE_PORT).
    /// </summary>
    [TestFixture]
    public class GetAllVersionsForOsRaceTest
    {
        private readonly string? serverIP = Environment.GetEnvironmentVariable("UNITTEST_REMOTE_HOST");
        private readonly string? serverPort = Environment.GetEnvironmentVariable("UNITTEST_REMOTE_PORT");

        private const string OsName = "Android"; // many versions + one empty version on the server
        private const int Iterations = 300;
        private const int Threads = 8;

        [Test]
        public void TestGetAllVersionsForOSConcurrentCallsAreConsistent()
        {
            // The expected result is computed single-threaded on a separate client (with its own cache).
            WmClient reference = WmClient.Create("http", serverIP, serverPort, "");
            string[] expected = reference.GetAllVersionsForOS(OsName);
            Assert.Greater(expected.Length, 10, "the test needs an OS with several versions");
            Assert.False(expected.Any(v => string.IsNullOrEmpty(v)));
            var expectedSet = new HashSet<string>(expected);

            var problems = new List<string>();
            for (int it = 0; it < Iterations; it++)
            {
                WmClient client = WmClient.Create("http", serverIP, serverPort, "");
                var results = new string[Threads][];
                var errors = new Exception?[Threads];
                var barrier = new Barrier(Threads);
                var threads = new Thread[Threads];
                for (int t = 0; t < Threads; t++)
                {
                    int idx = t;
                    threads[t] = new Thread(() =>
                    {
                        try
                        {
                            barrier.SignalAndWait();
                            results[idx] = client.GetAllVersionsForOS(OsName);
                        }
                        catch (Exception e) { errors[idx] = e; }
                    });
                    threads[t].Start();
                }
                foreach (var th in threads) th.Join();

                for (int t = 0; t < Threads; t++)
                {
                    if (errors[t] != null)
                    {
                        problems.Add($"iter {it}: exception {errors[t]!.GetType().Name}: {errors[t]!.Message}");
                    }
                    else if (results[t].Length != expected.Length || !expectedSet.SetEquals(results[t]))
                    {
                        var lost = expectedSet.Except(results[t]).ToArray();
                        problems.Add($"iter {it}: wrong result, {results[t].Length} versions instead of {expected.Length}, lost: [{string.Join(",", lost)}]");
                    }
                }

                // Corruption is persistent: the shared cache loses versions for every later caller.
                string[] after = client.GetAllVersionsForOS(OsName);
                if (!expectedSet.SetEquals(after))
                {
                    problems.Add($"iter {it}: cache corrupted, later call returns {after.Length} versions instead of {expected.Length}");
                }
            }

            if (problems.Count > 0)
            {
                Console.WriteLine($"{problems.Count} problems in {Iterations} iterations; first 10:");
                foreach (var p in problems.Take(10)) Console.WriteLine("  " + p);
            }
            Assert.AreEqual(0, problems.Count, "GetAllVersionsForOS is not thread safe");
        }
    }
}
