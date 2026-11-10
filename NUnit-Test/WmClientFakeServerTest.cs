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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Wmclient;

namespace NUnit_Test
{
    /// <summary>
    /// Tests that run against a tiny in-process fake WM server (HttpListener), so they do NOT need
    /// a real WURFL Microservice and can verify how the client reacts to unusual server responses.
    /// </summary>
    [TestFixture]
    public class WmClientFakeServerTest
    {
        private HttpListener listener = null!;
        private Thread worker = null!;
        private string port = "";
        private volatile string getInfoJson = "";

        [SetUp]
        public void StartFakeServer()
        {
            // Ask the OS for a free port, release it and let HttpListener use it.
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            port = ((IPEndPoint)probe.LocalEndpoint).Port.ToString();
            probe.Stop();

            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
            listener.Start();

            worker = new Thread(ServeLoop) { IsBackground = true };
            worker.Start();
        }

        [TearDown]
        public void StopFakeServer()
        {
            listener.Close();
            worker.Join(2000);
        }

        private void ServeLoop()
        {
            while (true)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = listener.GetContext();
                }
                catch (Exception)
                {
                    return; // listener closed
                }

                byte[] body = Encoding.UTF8.GetBytes(ctx.Request.Url!.AbsolutePath == "/v2/getinfo/json" ? getInfoJson : "{}");
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "application/json";
                ctx.Response.ContentLength64 = body.Length;
                ctx.Response.OutputStream.Write(body, 0, body.Length);
                ctx.Response.Close();
            }
        }

        // The fields common to every getinfo answer; the caps arrays are added by each test.
        private const string InfoPrefix =
            "{\"wurfl_api_version\":\"1.0\",\"wm_version\":\"2.0\",\"wurfl_info\":\"fake wurfl\"," +
            "\"important_headers\":[\"User-Agent\"],\"ltime\":\"t1\"";

        // The server answers with static capabilities only (no "virtual_caps" field).
        // The client accepts this answer in checkData ("static != null || virtual != null") but then
        // sorts both arrays, so Create used to crash with a "Value cannot be null" error.
        [Test]
        public void TestCreateWithStaticCapsOnly()
        {
            getInfoJson = InfoPrefix + ",\"static_caps\":[\"model_name\",\"brand_name\"]}";

            WmClient client = WmClient.Create("http", "127.0.0.1", port, "");
            try
            {
                Assert.True(client.HasStaticCapability("brand_name"));
                Assert.False(client.HasStaticCapability("unknown_cap"));
                Assert.False(client.HasVirtualCapability("is_smartphone"));
            }
            finally
            {
                client.DestroyConnection();
            }
        }

        // Same as above, the other way round: the server answers with virtual capabilities only (no "static_caps" field).
        [Test]
        public void TestCreateWithVirtualCapsOnly()
        {
            getInfoJson = InfoPrefix + ",\"virtual_caps\":[\"is_smartphone\",\"form_factor\"]}";

            WmClient client = WmClient.Create("http", "127.0.0.1", port, "");
            try
            {
                Assert.True(client.HasVirtualCapability("is_smartphone"));
                Assert.False(client.HasVirtualCapability("unknown_cap"));
                Assert.False(client.HasStaticCapability("brand_name"));
            }
            finally
            {
                client.DestroyConnection();
            }
        }
    }
}
