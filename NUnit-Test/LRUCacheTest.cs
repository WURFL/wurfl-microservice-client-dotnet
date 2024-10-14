/*
Copyright 2019 ScientiaMobile Inc. http://www.scientiamobile.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using NUnit.Framework;
using System;
using System.Threading;
using Wmclient;

// suppressing warnings related to use of constraint/classic model. We use classic.
#pragma warning disable NUnit2005 // Consider using Assert.That(actual, Is.EqualTo(expected)) instead of ClassicAssert.AreEqual(expected, actual)

namespace NUnit_Test
{
    [TestFixture]
    public class LRUCacheTest
    {
        [Test]
        public void LRUCache_RemoveOnMaxSizeTest()
        {
            LRUCache<string, TestNode> cache = new LRUCache<string, TestNode>(5);
            for (int i = 0; i < 6; i++)
            {
                cache.PutEntry(i.ToString(), new TestNode(i.ToString(), i));
            }
            Assert.AreEqual(5, cache.Size());
            // "0" entry has been removed when inserting "6"
            Assert.Null(cache.GetEntry("0"));
        }

        [Test]
        public void LRUCache_ReplaceExistingItemTest()
        {
            LRUCache<string, TestNode> cache = new LRUCache<string, TestNode>(5);
            for (int i = 0; i < 5; i++)
            {
                cache.PutEntry(i.ToString(), new TestNode(i.ToString(), i));
            }
            Assert.AreEqual(5, cache.Size());
            // Now put an element with the same key and a different value
            cache.PutEntry("2", new TestNode("2", 159));
            // previous value has been overwritten
            Assert.AreEqual(159, cache.GetEntry("2").Value);
        }

        [Test]
        public void LRUCache_ReplaceOnMultiAddTest()
        {
            LRUCache<string, Int32> cache = new LRUCache<string, Int32>(5);
            for (int i = 0; i < 6; i++)
            {
                cache.PutEntry(i.ToString(), i);
            }

            // re-add element with different value
            cache.PutEntry("3", 7);
            Assert.AreEqual(5, cache.Size());
            // "0" entry has been removed when inserting "6"
            Assert.AreEqual(7,cache.GetEntry("3"));
        }

        [Test]
        public void LRUCache_AddAndGetTest()
        {
            LRUCache_MultiThreadTestTask("add-get", 32);
        }

        [Test]
        public void LRUCache_GetAndClearTest()
        {
            LRUCache_MultiThreadTestTask("get-clear", 32);
        }

        private void LRUCache_MultiThreadTestTask(string taskType, int numThreads)
        {
            var userAgents = TestData.CreateTestUserAgentList();
            var cache = new LRUCache<string, Object>(100);
            Thread[] threads = new Thread[numThreads];
            TestTask[] tasks = new TestTask[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var task = new TestTask(cache, userAgents, i);
                tasks[i] = task;
                if (taskType.Equals("add-get"))
                {
                    threads[i] = new Thread(new ThreadStart(task.PerformAddAndGet));
                }
                else if (taskType.Equals("get-clear"))
                {
                    threads[i] = new Thread(new ThreadStart(task.PerformPutOrGetAndClear));
                }
                else
                {
                    Assert.Fail("Invalid test task type chosen");
                }
            }

            // Start all the threads...
            for (int i = 0; i < numThreads; i++)
            {
                threads[i].Start();
            }

            // ...and wait
            for (int i = 0; i < numThreads; i++)
            {
                threads[i].Join();
            }

            // check some data
            int linesRead = -1;
            for (int i = 0; i < numThreads; i++)
            {
                // Process did complete?
                Console.WriteLine("Task " + i + " completed: " + tasks[i].GetSuccess());
                Assert.True(tasks[i].GetSuccess());
                // Is the number of read lines consistent? (means: no thread has exited abruptly)
                if (i != 0)
                {
                    Console.WriteLine("Checking lines read for task " + i);
                    Assert.AreEqual(tasks[i].GetReadLines(), linesRead);
                }
                linesRead = tasks[i].GetReadLines();
            }
            if (taskType.Equals("get-clear"))
            {
                Console.WriteLine("Get-clear: checking cache size ");
                // in this case, we cannot determine if the last thread executed will perform a clear, a get, or a put
                Assert.True(cache.Size() == 0 || cache.Size() == 100);
            }
            else
            {
                Console.WriteLine("checking cache size for non clear cases");
                Assert.AreEqual(100, cache.Size());
            }

        }

    }

    internal class TestTask
    {
        private LRUCache<string, Object> cache;
        private int taskIndex;
        private string[] userAgents;
        private bool success = false;
        private int readLines;

        public bool GetSuccess()
        {
            return this.success;
        }

        public int GetReadLines()
        {
            return this.readLines;
        }

        public TestTask(LRUCache<string, Object> cache, String[] userAgents, int tindex)
        {
            this.userAgents = userAgents;
            this.cache = cache;
            this.taskIndex = tindex;
            this.readLines = 0;
        }

        public void PerformAddAndGet()
        {
            try
            {
                Console.WriteLine(string.Format("Starting thread #{0} ", this.taskIndex));
                foreach(String line in userAgents)
                {
                    try
                    {
                        cache.PutEntry(line, new Object());
                        cache.GetEntry(line);
                        readLines++;
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("Test failed with exceptions " + e.StackTrace);
                    }
                }
                this.success = true;
            }
            finally
            {
                Console.WriteLine(string.Format("{0} user agents read", readLines));
                Console.WriteLine(string.Format("Thread #{0} finished execution", this.taskIndex));
            }
        }

        public void PerformPutOrGetAndClear()
        {
            try
            {
                Console.WriteLine(string.Format("Starting thread #{0} ", this.taskIndex));
                foreach (String line in userAgents)
                {
                    try
                    {
                        cache.GetEntry(line);
                        if (taskIndex % 2 == 0 && readLines % 300 == 0)
                        {
                            cache.Clear();
                        }
                        else if (taskIndex % 2 != 0)
                        {
                            cache.PutEntry(line, new object());
                        }

                        readLines++;
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("Test failed with exceptions " + e.StackTrace);
                    }
                }
                this.success = true;
            }
            finally
            {
                Console.WriteLine(string.Format("{0} user agents read", readLines));
                Console.WriteLine(string.Format("Thread #{0} finished execution", this.taskIndex));
            }
        }
    }

    internal class TestNode
    {
        public TestNode(string k, int v)
        {
            this.Key = k;
            this.Value = v;
        }
        public string Key { get; set; }
        public int Value { get; set; }
    }
}
