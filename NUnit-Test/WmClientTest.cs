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
using HiPerfTiming;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using Wmclient;

namespace NUnit_Test
{
    [TestFixture]
    public class WmClientTest
    {
        String serverProtocol = "http";

        // to run locally, set the following environment variables
        String serverIP = Environment.GetEnvironmentVariable("UNITTEST_REMOTE_HOST");
        String serverPort = Environment.GetEnvironmentVariable("UNITTEST_REMOTE_PORT");
    
        [Test]
        public void TestCreateOk()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            Assert.NotNull(client);
            Assert.True(client.ImportantHeaders.Length > 0);
            Assert.True(client.VirtualCaps.Length > 0);
            Assert.True(client.StaticCaps.Length > 0);
        }

        [Test]
        public void TestCreateWithServerDown()
        {
            Assert.Throws<WmException>(() => WmClient.Create(serverProtocol, serverIP, "18080", ""));
        }

        [Test]
        public void TestCreateWithoutHost()
        {
            Assert.Throws<WmException>(() => WmClient.Create(serverProtocol, "", serverPort, ""));
        }

        [Test]
        public void TestCreateWithEmptyServerValues()
        {
            Assert.Throws<WmException>(() => WmClient.Create("", "", "", ""));
        }

        [Test]
        public void TestGetInfo()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                JSONInfoData jsonInfoData = client.GetInfo();
                Assert.NotNull(jsonInfoData);
                Assert.NotNull(jsonInfoData.Wm_version);
                Assert.True(jsonInfoData.Wm_version.Length >= 5);
                Console.WriteLine(string.Format("WM server version #{0} ", jsonInfoData.Wm_version));
                Assert.True(jsonInfoData.Wurfl_info.Length > 0);
                Assert.True(jsonInfoData.Important_headers.Length > 0);
                Assert.True(jsonInfoData.Static_caps.Length > 0);
                Assert.True(jsonInfoData.Virtual_caps.Length > 0);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupUserAgent()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                var ua = "Mozilla/5.0 (Linux; Android 7.0; SAMSUNG SM-G950F Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/5.2 Chrome/51.0.2704.106 Mobile Safari/537.36";
                JSONDeviceData device = client.LookupUserAgent(ua);
                Assert.NotNull(device);
                int dcount = device.Capabilities.Count;
                Assert.True(dcount >= 40); // sum of caps, vcaps and wurfl_id
                Assert.AreEqual(device.Capabilities["model_name"], "SM-G950F");
                Assert.AreEqual("false", device.Capabilities["is_app"]);
                Assert.AreEqual("false", device.Capabilities["is_app_webview"]);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupUserAgentWithSpecificCaps()
        {
            string[] reqCaps = { "brand_name", "model_name", "form_factor", "is_mobile", "is_android", "is_ios", "is_app" };

            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                client.SetRequestedCapabilities(reqCaps);
                var ua = "Mozilla/5.0 (Nintendo Switch; WebApplet) AppleWebKit/601.6 (KHTML, like Gecko) NF/4.0.0.5.9 NintendoBrowser/5.1.0.13341";
                JSONDeviceData device = client.LookupUserAgent(ua);
                Assert.NotNull(device);
                Assert.NotNull(device.Capabilities);
                Assert.AreEqual("Nintendo", device.Capabilities["brand_name"]);
                Assert.AreEqual("Switch", device.Capabilities["model_name"]);
                Assert.AreEqual("true", device.Capabilities["is_mobile"]);
                Assert.AreEqual(8, device.Capabilities.Count);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupUseragentEmptyuUa()
        {
            WmClient conn = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                JSONDeviceData device = conn.LookupUserAgent("");
                Assert.NotNull(device);
                var did = device.Capabilities;
                Assert.NotNull(did);
                Assert.NotNull(device.APIVersion);
                Assert.True(device.Error.Length == 0);
                Assert.AreEqual("generic", device.Capabilities["wurfl_id"]);
            }
            finally
            {
                conn.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupUseragentNullUa()
        {
            WmClient conn = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                JSONDeviceData device = conn.LookupUserAgent(null);
                Assert.NotNull(device);
                var did = device.Capabilities;
                Assert.NotNull(did);
                Assert.NotNull(device.Error);
                // From v 2.1.0 server returns a generic device when queried qith null/empty User-Agent
                Assert.AreEqual("generic", device.Capabilities["wurfl_id"]);

            }
            finally
            {
                conn.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupDeviceId()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupDeviceID("nokia_generic_series40");
                Assert.NotNull(jsonData);
                var did = jsonData.Capabilities;
                Assert.NotNull(did);
                // num caps + num vcaps + wurfl id 
                Assert.True(did.Count >= 40);
                Assert.AreEqual("false", did["is_android"]);
                Assert.AreEqual("128", did["resolution_width"]);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupDeviceIdWithSpecificCaps()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                String[] reqCaps = { "brand_name", "is_smarttv" };
                String[] reqvCaps = { "is_app", "is_app_webview" };
                client.SetRequestedStaticCapabilities(reqCaps);
                client.SetRequestedVirtualCapabilities(reqvCaps);
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupDeviceID("generic_opera_mini_version1");
                Assert.NotNull(jsonData);
                var did = jsonData.Capabilities;
                Assert.NotNull(did);
                Assert.AreEqual("Opera", did["brand_name"]);
                Assert.AreEqual("false", did["is_app"]);
                Assert.AreEqual(5, did.Count);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupDeviceIdWithWrongId()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            var excCatched = false;
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupDeviceID("nokia_generic_series40_wrong");
            }
            catch (WmException e)
            {
                excCatched = true;
                Assert.NotNull(e.Message);
                Assert.True(e.Message.Length > 0);
                Assert.True(e.Message.Contains("device is missing"));
            }
            finally
            {
                client.DestroyConnection();
            }
            Assert.True(excCatched);
        }

        [Test]
        public void TestLookupDeviceIdWithNullId()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            bool excCatched = false;
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupDeviceID(null);
            }
            catch (WmException e)
            {
                excCatched = true;
                Assert.NotNull(e.Message);
                Assert.True(e.Message.Length > 0);
                Assert.True(e.Message.Contains("device is missing"));
            }
            finally
            {
                client.DestroyConnection();
            }
            Assert.True(excCatched);
        }

        [Test]
        public void TestLookupDeviceIdWithEmptyId()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            bool excCatched = false;
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupDeviceID("");
            }
            catch (WmException e)
            {
                excCatched = true;
                Assert.NotNull(e.Message);
                Assert.True(e.Message.Length > 0);
                Assert.True(e.Message.Contains("device is missing"));
            }
            finally
            {
                client.DestroyConnection();
            }
            Assert.True(excCatched);
        }


#if UNIT_TESTS
        [Test]
        public void TestLookupRequestOK()
        {
            var ua = "Mozilla/5.0 (Nintendo Switch; WebApplet) AppleWebKit/601.6 (KHTML, like Gecko) NF/4.0.0.5.9 NintendoBrowser/5.1.0.13341";
            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            NameValueCollection requestHeaders = new NameValueCollection
            {
                { "User-agent", ua},
                { "content-Type", "gzip, deflate"},
                { "Accept-Encoding", "application/json"}
            };
            mockHttpRequest.SetupGet(x => x.Headers).Returns(requestHeaders);

            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupRequest(mockHttpRequest.Object);
                Assert.NotNull(jsonData);
                var did = jsonData.Capabilities;
                Assert.NotNull(did);
                Assert.True(did.Count >= 40);
                Assert.AreEqual("Smart-TV", did["form_factor"]);
                
                Assert.AreEqual("false", did["is_app"]);
                Assert.AreEqual("false", did["is_app_webview"]);
                Assert.AreEqual("Nintendo", did["advertised_device_os"]);
                Assert.AreEqual("Nintendo Switch", did["complete_device_name"]);
                Assert.AreEqual("nintendo_switch_ver1", did["wurfl_id"]);

                // Now set a cap filter
                string[] reqCaps = { "form_factor", "is_mobile", "is_app", "complete_device_name", "advertised_device_os", "brand_name" };
                client.SetRequestedCapabilities(reqCaps);
                jsonData = client.LookupRequest(mockHttpRequest.Object);
                did = jsonData.Capabilities;
                Assert.NotNull(did);
                Assert.AreEqual(7, did.Count);

                // Now, lets try with mixed case headers
                requestHeaders = new NameValueCollection
                {
                { "usEr-AgeNt", ua},
                { "CoNtent-TYpe", "gzip, deflate"},
                { "Accept-ENCoding", "application/json"}
            };
                mockHttpRequest.SetupGet(x => x.Headers).Returns(requestHeaders);
                jsonData = client.LookupRequest(mockHttpRequest.Object);
                did = jsonData.Capabilities;
                Assert.NotNull(did);

            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupHeadersOK()
        {
            var ua = "Mozilla/5.0 (Nintendo Switch; WebApplet) AppleWebKit/601.6 (KHTML, like Gecko) NF/4.0.0.5.9 NintendoBrowser/5.1.0.13341";
            IDictionary<string,string> requestHeaders = new Dictionary<string,string>()
            {
                { "User-agent", ua},
                { "content-Type", "gzip, deflate"},
                { "Accept-Encoding", "application/json"}
            };
            

            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            client.SetCacheSize(1000);
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupHeaders(requestHeaders);
                Assert.NotNull(jsonData);
                var did = jsonData.Capabilities;
                Assert.NotNull(did);
                Assert.True(did.Count >= 40);
                Assert.AreEqual("Smart-TV", did["form_factor"]);

                Assert.AreEqual("false", did["is_app"]);
                Assert.AreEqual("false", did["is_app_webview"]);
                Assert.AreEqual("Nintendo", did["advertised_device_os"]);
                Assert.AreEqual("Nintendo Switch", did["complete_device_name"]);
                Assert.AreEqual("nintendo_switch_ver1", did["wurfl_id"]);

                // Now set a cap filter
                string[] reqCaps = { "form_factor", "is_mobile", "is_app", "complete_device_name", "advertised_device_os", "brand_name" };
                client.SetRequestedCapabilities(reqCaps);
                jsonData = client.LookupHeaders(requestHeaders);
                did = jsonData.Capabilities;
                Assert.NotNull(did);
                Assert.AreEqual(7, did.Count);

                // Now, lets try with mixed case headers
                requestHeaders = new Dictionary<string,string>()
                {
                { "usEr-AgeNt", ua},
                { "CoNtent-TYpe", "gzip, deflate"},
                { "Accept-ENCoding", "application/json"}
            };

                jsonData = client.LookupHeaders(requestHeaders);
                did = jsonData.Capabilities;
                Assert.NotNull(did);
            
                int[] cacheSizes = client.GetActualCacheSizes();
                // Cache has been hit even if header names have a different case
                Assert.AreEqual(1, cacheSizes[1]);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupHeadersWithoutHeders()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupHeaders(null);
                Assert.NotNull(jsonData);
                Assert.AreEqual("generic", jsonData.Capabilities["wurfl_id"]);
            } 
            catch(WmException e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupHeadersWithEmptyHeaders()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                Assert.NotNull(client);
                JSONDeviceData jsonData = client.LookupHeaders(new Dictionary<string,string>());
                Assert.NotNull(jsonData);
                Assert.AreEqual("generic", jsonData.Capabilities["wurfl_id"]);
            }
            catch (WmException e)
            {
                Assert.NotNull(e.Message);
                Assert.True(e.Message.Length > 0);
                Assert.True(e.Message.Contains("Headers dictionary cannot be null"));
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupRequestWithSpecificCapsAndNoHeaders()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            
            try
            {
                string[] reqCaps = { "brand_name", "is_wireless_device", "pointing_method", "model_name" };
                Assert.NotNull(client);
                client.SetRequestedCapabilities(reqCaps);

                // Create request to pass
                Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
                NameValueCollection requestHeaders = new NameValueCollection { };
                mockHttpRequest.SetupGet(x => x.Headers).Returns(requestHeaders);
                JSONDeviceData jsonData = client.LookupRequest(mockHttpRequest.Object);
                // From version 2.1.0 on, WURFL Microservice server returns "generic" device and not
                // an error in case of empty user-agent or headers.
                Assert.AreEqual("generic", jsonData.Capabilities["wurfl_id"]);
            }
            catch (WmException e)
            {
                Assert.NotNull(e.Message);
                Assert.True(e.Message.Length > 0);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestLookupRequestWithEmptyBody()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            bool excCatched = false;
            try
            {
                Assert.NotNull(client);

                // Create request to pass
                Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();

                JSONDeviceData jsonData = client.LookupRequest(mockHttpRequest.Object);
            }
            catch (WmException e)
            {
                excCatched = true;
                Assert.NotNull(e.Message);
                Assert.True(e.Message.Length > 0);
                Assert.True(e.Message.Contains("Headers dictionary cannot be null"));
            }
            finally
            {
                client.DestroyConnection();
            }
            Assert.True(excCatched);
        }
#endif

        [Test]
        public void TestLookupUseragentWithVcapOnly()
        {
            var ua = "Mozilla/5.0 (Nintendo Switch; WebApplet) AppleWebKit/601.6 (KHTML, like Gecko) NF/4.0.0.5.9 NintendoBrowser/5.1.0.13341";

            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            try
            {
                string[] reqCaps = { "is_robot", "is_smartphone", "is_app", "complete_device_name", "advertised_device_os",
            "advertised_device_os_version", "form_factor", "is_app_webview" };
                Assert.NotNull(client);
                client.SetRequestedCapabilities(reqCaps);

                JSONDeviceData jsonData = client.LookupUserAgent(ua);
                Assert.NotNull(jsonData);
                var did = jsonData.Capabilities;
                Assert.NotNull(did);
                Assert.AreEqual(9, did.Count);
                Assert.AreEqual("false", did["is_robot"]);
                Assert.AreEqual("false", did["is_smartphone"]);
                Assert.AreEqual("false", did["is_app"]);
                Assert.AreEqual("false", did["is_app_webview"]);
                Assert.AreEqual("Nintendo", did["advertised_device_os"]);
                Assert.AreEqual("Nintendo Switch", did["complete_device_name"]);
                Assert.AreEqual("nintendo_switch_ver1", did["wurfl_id"]);
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        [Test]
        public void TestDestroyConnection()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            Assert.NotNull(client);


            Assert.NotNull(client.GetInfo());

            client.DestroyConnection();
            Assert.Throws<WmException>(() => client.GetInfo());
        }

        [Test]
        public void TestCreateWithEmptySchemeValue()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            Assert.NotNull(client);
            Assert.True(client.ImportantHeaders.Length > 0);
            Assert.True(client.VirtualCaps.Length > 0);
            Assert.True(client.StaticCaps.Length > 0);
        }

        [Test]
        public void TestCreateWithWrongSchemeValue()
        {
            Assert.Throws<WmException>(() => WmClient.Create("smtp", serverIP, "18080", ""));
        }

        [Test]
        public void TestHasStaticCapability()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            Assert.NotNull(client);
            Assert.True(client.HasStaticCapability("brand_name"));
            Assert.True(client.HasStaticCapability("model_name"));
            Assert.True(client.HasStaticCapability("is_smarttv"));
            // this is a virtual capability, so it shouldn't be returned
            Assert.False(client.HasStaticCapability("is_app"));
        }

        [Test]
        public void TestSetRequestedCapabilities()
        {
            string ua = "Mozilla/5.0 (Nintendo Switch; WebApplet) AppleWebKit/601.6 (KHTML, like Gecko) NF/4.0.0.5.9 NintendoBrowser/5.1.0.13341";
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            Assert.NotNull(client);
            client.SetRequestedStaticCapabilities(new string[] { "brand_name", "is_ios", "non_ex_cap" });
            client.SetRequestedVirtualCapabilities(new string[] { "brand_name", "is_ios", "non_ex_vcap" });

            JSONDeviceData d = client.LookupUserAgent(ua);
            Assert.NotNull(d);
            Assert.AreEqual(3, d.Capabilities.Count);
            client.SetRequestedStaticCapabilities(null);
            d = client.LookupUserAgent(ua);
            Assert.AreEqual(2, d.Capabilities.Count);
            client.SetRequestedVirtualCapabilities(null);
            // Resetting all requested caps arrays makes server return ALL available caps
            d = client.LookupUserAgent(ua);
            Assert.True(d.Capabilities.Count >= 40);

            bool exc = false;
            try
            {
                var cap = d.Capabilities["non_ex_cap"];
            }
            catch (Exception)
            {
                exc = true;
            }
            Assert.True(exc);
        }

        [Test]
        public void TestHasVirtualCapability()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            Assert.NotNull(client);
            Assert.True(client.HasVirtualCapability("is_app"));
            Assert.True(client.HasVirtualCapability("is_smartphone"));
            Assert.True(client.HasVirtualCapability("form_factor"));
            Assert.True(client.HasVirtualCapability("is_app_webview"));
            // this is a static capability, so it shouldn't be returned
            Assert.False(client.HasVirtualCapability("brand_name"));
            Assert.False(client.HasVirtualCapability("is_wireless_device"));
        }

        public WmClient CreateTestCachedClient(int csize)
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            client.SetCacheSize(csize);
            Assert.NotNull(client);
            return client;
        }

        [Test]
        public void TestMultipleLookupUserAgentWithCache()
        {
            var client = CreateTestCachedClient(1000);
            Assert.NotNull(client);

            for (int i = 0; i < 50; i++)
            {
                internalTestLookupUserAgent(client);
            }
            client.DestroyConnection();
        }

        public void internalTestLookupUserAgent(WmClient client)
        {
            var ua = "Mozilla/5.0 (Linux; Android 7.0; SAMSUNG SM-G950F Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/5.2 Chrome/51.0.2704.106 Mobile Safari/537.36";
            var jsonData = client.LookupUserAgent(ua);
            Assert.NotNull(jsonData);
            var did = jsonData.Capabilities;
            Assert.NotNull(did);
            Assert.True(did.Count >= 40); // sum of caps, vcaps and 1 wurfl_id. 40 is the size of minimum capability set
            Assert.AreEqual(did["model_name"], "SM-G950F");
            Assert.AreEqual("false", did["is_app"]);
            Assert.AreEqual("false", did["is_app_webview"]);
        }

#if UNIT_TESTS
        [Test]
        public void TestLookupRequestWithCache()
        {
            var client = CreateTestCachedClient(1000);
            var url = "http://mysite.com/api/v2/brad/info.json";

            var ua = "Mozilla/5.0 (Nintendo Switch; WebApplet) AppleWebKit/601.6 (KHTML, like Gecko) NF/4.0.0.5.9 NintendoBrowser/5.1.0.13341";
            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            NameValueCollection requestHeaders = new NameValueCollection
            {
                { "User-Agent", ua},
                { "Content-Type", "gzip, deflate"},
                { "Accept-Encoding", "application/json"},
                { "X-UCBrowser-Device-UA", "Mozilla/5.0 (SAMSUNG; SAMSUNG-GT-S5253/S5253DDJI7; U; Bada/1.0; en-us) AppleWebKit/533.1 (KHTML, like Gecko) Dolfin/2.0 Mobile WQVGA SMM-MMS/1.2.0 OPN-B"},
                { "X-Requested-With", "json_client" }
            };
            mockHttpRequest.SetupGet(x => x.Headers).Returns(requestHeaders);

            for (int i = 0; i < 50; i++)
            {
                var jsonData = client.LookupRequest(mockHttpRequest.Object);
                Assert.NotNull(jsonData);
                var did = jsonData.Capabilities;
                Assert.NotNull(did);

                Assert.AreEqual("Samsung", did["brand_name"]); // UA has been overridden by X-UCBrowser-Device-UA
                Assert.AreEqual("true", did["is_mobile"]);
                Assert.True(did.Count >= 40);

                var cSize = client.GetActualCacheSizes();

                Assert.AreEqual(0, cSize[0]);
                Assert.AreEqual(1, cSize[1]);
            }
            client.DestroyConnection();
        }
#endif

#if UNIT_TESTS
        [Test]
        public void TestSingleLookupDeviceIdWithCacheExpiration()
        {
            var client = CreateTestCachedClient(1000);
            var d1 = client.LookupDeviceID("nokia_generic_series40");
            Assert.NotNull(d1);
            var d2 = client.LookupUserAgent("Mozilla/5.0 (iPhone; CPU iPhone OS 10_2_1 like Mac OS X) AppleWebKit/602.4.6 (KHTML, like Gecko) Version/10.0 Mobile/14D27 Safari/602.1");
            Assert.NotNull(d2);
            var csizes = client.GetActualCacheSizes();
            Assert.AreEqual(1, csizes[0]);
            Assert.AreEqual(1, csizes[1]);
            // Date doesn't change, so cache stay full
            client.ClearCachesIfNeeded(d1.Ltime);
            Assert.AreEqual(1, csizes[0]);
            Assert.AreEqual(1, csizes[1]);

            // Now, date changes, so caches must be cleared
            client.ClearCachesIfNeeded("2199-12-31");

            csizes = client.GetActualCacheSizes();
            Assert.AreEqual(0, csizes[0]);
            Assert.AreEqual(0, csizes[1]);
            // Load a device again
            d1 = client.LookupDeviceID("nokia_generic_series40");
            d2 = client.LookupUserAgent("Mozilla/5.0 (iPhone; CPU iPhone OS 10_2_1 like Mac OS X) AppleWebKit/602.4.6 (KHTML, like Gecko) Version/10.0 Mobile/14D27 Safari/602.1");

            // caches are filled again
            csizes = client.GetActualCacheSizes();
            Assert.AreEqual(1, csizes[0]);
            Assert.AreEqual(1, csizes[1]);
            client.DestroyConnection();
        }
#endif

        [Test]
        public void TestMultiThreadClientUsage()
        {
            var userAgents = TestData.CreateTestUserAgentList();
            var client = CreateTestCachedClient(50000);
            int numThreads = 16;
            Thread[] threads = new Thread[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var lookupUA = new LookupUserAgent(client, i, userAgents);
                threads[i] = new Thread(new ThreadStart(lookupUA.lookup));
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
        }

        [Test]
        public void TestCacheUsage()
        {
            WmClient client = WmClient.Create(serverProtocol, serverIP, serverPort, "");
            HiPerformanceTimer timer = new HiPerformanceTimer();
            var userAgentList = TestData.CreateTestUserAgentList();
            try
            {
                timer.Start();
                foreach (var ua in userAgentList)
                {
                    client.LookupUserAgent(ua);
                }
                timer.Stop();
                // get the total detection time in nanoseconds
                var totalDetectionTime = timer.Duration(true);

                // now enable the cache and fill it
                client.SetCacheSize(10000);
                foreach (var ua in userAgentList)
                {
                    client.LookupUserAgent(ua);
                }

                // Now measure cache usage time
                timer.Start();
                foreach (var ua in userAgentList)
                {
                    client.LookupUserAgent(ua);
                }
                timer.Stop();
                var totalCacheUsageTime = timer.Duration(true);
                var avgDetectionTime = (double)totalDetectionTime / (double)userAgentList.Length;
                var avgCacheTime = (double)totalCacheUsageTime / (double)userAgentList.Length;
                // Test passes only if cache performance is at least one order of magnitude faster than detection
                Assert.True(avgDetectionTime > avgCacheTime * 10);
            }
            finally
            {
                client.DestroyConnection();
            }

        }
    }

    public class LookupUserAgent
    {
        private int threadIndex = -1;
        private WmClient client;
        private String[] userAgents;
        public LookupUserAgent(WmClient client, int idx, String[] userAgents)
        {
            this.client = client;
            this.threadIndex = idx;
            this.userAgents = userAgents;
        }

        public void lookup()
        {
            int i = 0;
            try
            {
                Console.WriteLine(string.Format("Starting thread #{0} ", this.threadIndex));
                foreach (String line in userAgents)
                {
                    try
                    {
                        client.LookupUserAgent(line);
                        i++;
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("Test failed with exceptions " + e.StackTrace);
                    }
                }
            }
            finally
            {
                Console.WriteLine(string.Format("{0} user agents read", i));
                Console.WriteLine(string.Format("Thread #{0} finished execution", this.threadIndex));
            }
        }
    }
}