﻿/*
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
using System;
using System.Collections.Generic;
using Wmclient;

namespace Example
{
    public class WmClientExample
    {
        static void Main(string[] args)
        {
            try
            {
                // First we need to create a WM client instance, to connect to our WM server API at the specified host and port.
                // Last parameter is a prefix for path, most of the time we won't need it
                WmClient client = WmClient.Create("http" ,"localhost", "8080", "");
            
                if (client != null)
                {
                    // enable cache: by setting the cache size we are also activating the caching option in WM client. 
                    // In order to not use cache, you just to need to omit setCacheSize call
                    client.SetCacheSize(100000);

                    // We ask Wm server API for some Wm server info such as server API version and info about WURFL API and file used by WM server.
                    JSONInfoData info = client.GetInfo();
                    Console.WriteLine("Server info: \n");
                    Console.WriteLine("WURFL API version: " + info.Wurfl_api_version);
                    Console.WriteLine("WM server version: " + info.Wm_version);
                    Console.WriteLine("WURFL file info:" + info.Wurfl_info + '\n');
                    
                    var requestedStaticCaps = new string[]{
                        "brand_name",
                        "model_name"
                    };
                    var requestedVirtualCapabilities = new string[]{
                        "is_smartphone",
                        "form_factor"
                    };
                    // set the capabilities we want to receive from WM server
                    client.SetRequestedStaticCapabilities(requestedStaticCaps);
                    client.SetRequestedVirtualCapabilities(requestedVirtualCapabilities);
                    var ua = "UCWEB/2.0 (Java; U; MIDP-2.0; Nokia203/20.37) U2/1.0.0 UCBrowser/8.7.0.218 U2/1.0.0 Mobile";
                    Console.WriteLine("Device lookup for user-agent: " + ua);
                    // Perform a device detection calling WM server API
                    JSONDeviceData device = client.LookupUserAgent(ua);
                    // Let's get the device capabilities and print some of them
                    Console.WriteLine("Detected device WURFL ID: " + device.Capabilities["wurfl_id"]);
                    Console.WriteLine("Detected device brand & model: " + device.Capabilities["brand_name"]
                        + " " + device.Capabilities["model_name"]);
                    Console.WriteLine("Detected device form factor: " + device.Capabilities["form_factor"]);
                    if (device.Capabilities["is_smartphone"].Equals("true"))
                    {
                        Console.WriteLine("This is a smartphone");
                    }

                    // Now let's print all the device capabilities
                    DumpDevice(device);

                    // Get all the device manufacturers, and print the first twenty
                    int limit = 20;
                    String[] deviceMakes = client.GetAllDeviceMakes();
                    Console.WriteLine(String.Format("Print the first {0} Brand of {1}\n", limit, deviceMakes.Length));

                    // Sort the device manufacturer names
                    Array.Sort(deviceMakes);
                    for (int i = 0; i < limit; i++)
                    {
                        Console.WriteLine(String.Format(" - {0}\n", deviceMakes[i]));
                    }

                    // Now call the WM server to get all device model and marketing names produced by Apple
                    Console.WriteLine("Print all Model for the Apple Brand");
                    JSONModelMktName[] devNames = client.GetAllDevicesForMake("Apple");

                    // Sort ModelMktName objects by their model name (a specific comparer is used)
                    Array.Sort(devNames, new ByModelNameComparer());
                    foreach (JSONModelMktName modelMktName in devNames)
                    {
                        Console.WriteLine(" - {0} {1}\n", modelMktName.Model_Name, modelMktName.Marketing_Name);
                    }

                    // Now call the WM server to get all operative system names
                    Console.WriteLine("Print the list of OSes");
                    String[] oses = client.GetAllOSes();
                    // Sort and print all OS names
                    Array.Sort(oses);
                    foreach (String os in oses)
                    {
                        Console.WriteLine(String.Format(" - {0}\n", os));
                    }
                    // Let's call the WM server to get all version of the Android OS
                    Console.WriteLine("Print all versions for the Android OS");
                    String[] osVersions = client.GetAllVersionsForOS("Android");
                    // Sort all Android version numbers and print them.
                    Array.Sort(osVersions);
                    foreach (String ver in osVersions)
                    {
                        Console.WriteLine(" - {0}\n", ver);
                    }
                }
                else
                {
                    Console.WriteLine("WmClient instance is null");
                }
                // Deallocate all client resources. Any call on client method after this one will throw a WmException
                client.DestroyConnection();
            }
            catch (WmException e)
            {
                // problems such as network errors  or internal server problems
                Console.WriteLine(e.Message);
            }

            finally
            {
                
                Console.Write("Press a key...");
                Console.ReadKey();
            }
        }

        // Prints all the device capabilities
        private static void DumpDevice(JSONDeviceData device)
        {
            if(device.Capabilities.Count > 0)
            {
                Console.WriteLine("Requested device capabilities");
                Console.WriteLine();
                var capKeys = device.Capabilities.Keys;
                
                foreach(string key in capKeys)
                {
                    Console.WriteLine("Capability " + key + ": " + device.Capabilities[key]);
                }
            }
        }
    }

    // Comparer implementation used to sort JSONModelMktName objects according to their model name property, 
    // for which is used the String natural ordering.
    internal class ByModelNameComparer : IComparer<JSONModelMktName>
    {
        public int Compare(JSONModelMktName o1, JSONModelMktName o2)
        {
            if (o1 == null && o2 == null) { return 0; }
            if (o1 == null && o2 != null)
            {
                return 1;
            }

            if (o1 != null && o2 == null)
            {
                return -1;
            }

            return o1.Model_Name.CompareTo(o2.Model_Name);
        }
    }
}