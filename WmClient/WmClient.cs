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
using System;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Net.Http.Headers;

namespace Wmclient
{
    public class WmClient
    {
        private static readonly string UserAgentHeaderName = "User-Agent";
        private static readonly int DeviceDefaultCacheSize = 20000;
        private static readonly Object _lock = new Object();

        private string scheme;
        private string host;
        private string port;
        private string baseURI;
        private string[] importantHeaders;

        // These are the lists of all static or virtual that can be returned by the running WM server
        public string[] StaticCaps { get; set; }
        public string[] VirtualCaps { get; set; }

        // Requested are used in the lookup requests, accessible via the SetRequested[...] methods
        private string[] requestedStaticCaps;
        private string[] requestedVirtualCaps;

        // Reusable instance of http client: its methods SendAsync, GetAsync and extension 
        //static method PostAsJsonAync are thread-safe from .net 4.5.2 as of MSDN documentation.
        private HttpClient c;

        // Internal caches: LRU caches are internally thread safe
        private LRUCache<String, JSONDeviceData> devIdCache; // Maps device ID -> JSONDeviceData
        private LRUCache<String, JSONDeviceData> uaCache; // Maps concat headers (mainly UA) -> JSONDeviceData
        // Time of last WURFL.xml file load on server
        private string ltime;

        private JSONMakeModel[] makeModels;

        // List of device makes ("Nokia", "Apple", etc.)
        private string[] deviceMakes;
        // Used to lock deviceMakes object in critical code sections
        private Object _deviceMakesLock = new Object();
        // Dictionary that associates maker name to JSONModelMktName objects
        private IDictionary<String, IList<JSONModelMktName>> deviceMakesMap = new Dictionary<String, IList<JSONModelMktName>>();
        // Dictionary that associates os name to JSONDeviceOsVersions objects
        private IDictionary<String, IList<String>> deviceOsVersionsMap = new Dictionary<String, IList<String>>();
        // List of all device OSes
        private String[] deviceOSes = new String[0];
        // Lock object user for deviceOSes safety
        private Object _deviceOSesLock = new Object();

        // Cache of make model structure
        public JSONMakeModel[] MakeModels
        {
            get
            {
                lock (_lock) { return this.makeModels; };
            }
        }

        public string[] DeviceMakes
        {
            get
            {
                lock (_deviceMakesLock) { return this.deviceMakes; }
            }
        }

        public string[] DeviceOSes
        {
            get
            {
                lock (_deviceOSesLock) { return this.deviceOSes; }
            }
        }


        private WmClient()
        {
            c = new HttpClient();
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // provides internal access to HttpClient.
        private HttpClient GetClient()
        {
            return c;
        }

        /// <summary>
        /// Creates a WURFL microservice client and tests if server denoted by the given host and port is available for connection.
        /// Throws WmClientException in case server is down or not available for connection
        /// </summary>
        /// <param name="host">WM server host</param>
        /// <param name="port">WM server port</param>
        /// <returns>A instance of the wurfl microservice client</returns>
        public static WmClient Create(string scheme, string host, string port, string baseURI)
        {

            WmClient client = new WmClient();

            client.Host = host;
            client.Port = port;
            if (scheme != null && scheme.Length > 0)
            {
                client.scheme = scheme;
            }
            else
            {
                client.scheme = "http";
            }
            client.baseURI = baseURI;

            // retieves internal http client and performs an HEAD request

            JSONInfoData data = null;

            try
            {
                data = client.GetInfo();

                client.ImportantHeaders = data.Important_headers;
                client.StaticCaps = data.Static_caps;
                client.VirtualCaps = data.Virtual_caps;
                Array.Sort(client.StaticCaps);
                Array.Sort(client.VirtualCaps);
            }
            catch (Exception e)
            {
                if (client != null)
                {
                    client.DestroyConnection();
                }
                throw new WmException("Error creating WM CLIENT: " + e.Message, e);
            }
            return client;
        }

        public void SetCacheSize(int uaMaxEntries)
        {
            this.uaCache = new LRUCache<string, JSONDeviceData>(uaMaxEntries);
            this.devIdCache = new LRUCache<string, JSONDeviceData>(DeviceDefaultCacheSize);
        }

        // Checks consistency of returned data
        private static bool checkData(JSONInfoData _data)
        {
            return !String.IsNullOrEmpty(_data.Wm_version)
                && !String.IsNullOrEmpty(_data.Wurfl_api_version)
                && !String.IsNullOrEmpty(_data.Wurfl_info)
                && (_data.Static_caps != null || _data.Virtual_caps != null);
        }

        // Returns the complete URL for connection, used by API methods internally.
        private string createURL(string p)
        {
            if (baseURI != null && baseURI.Length > 0)
            {
                return scheme + "://" + host + ":" + port + "/" + baseURI + p;
            }
            else
            {
                return scheme + "://" + host + ":" + port + p;
            }
        }

        /// <summary>
        /// WM server host to connect to
        /// </summary>
        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        /// <summary>
        /// Port of the host to connect to
        /// </summary>
        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        public string[] ImportantHeaders
        {
            get { return importantHeaders; }
            set { importantHeaders = value; }
        }

        // Returns true if the given capability Name exist in this client' static capability set, false otherwise
        public bool HasStaticCapability(string capName)
        {
            return arrayHasValue(StaticCaps, capName);
        }

        // Returns true if the given capName exist in this client' virtual capability set, false otherwise
        public bool HasVirtualCapability(string capName)
        {
            return arrayHasValue(VirtualCaps, capName);
        }

        private bool arrayHasValue(string[] caps, string capName)
        {
            return (Array.BinarySearch(caps, capName) >= 0);
        }

        public void SetRequestedCapabilities(string[] capsList)
        {
            if (capsList == null)
            {
                this.requestedStaticCaps = null;
                this.requestedVirtualCaps = null;
                this.ClearCaches();
                return;
            }

            var capNames = new List<string>();
            var vcapNames = new List<string>();

            foreach (string name in capsList)
            {
                if (HasStaticCapability(name))
                {
                    capNames.Add(name);
                }
                else if (HasVirtualCapability(name))
                {
                    vcapNames.Add(name);
                }
            }
            this.requestedStaticCaps = capNames.ToArray();
            this.requestedVirtualCaps = vcapNames.ToArray();
            ClearCaches();
        }

        /// <summary>
        /// Sets the list of static capabilities to be requested to WM server
        /// </summary>
        /// <param name="capsList"></param>
        public void SetRequestedStaticCapabilities(string[] capsList)
        {
            if (capsList == null)
            {
                this.requestedStaticCaps = null;
                this.ClearCaches();
                return;
            }

            var capNames = new List<string>();
            foreach (string name in capsList)
            {
                if (HasStaticCapability(name))
                {
                    capNames.Add(name);
                }
                this.requestedStaticCaps = capNames.ToArray();
            }
            ClearCaches();
        }

        /// <summary>
        /// Sets the list of virtual capabilities to be requested to WM server
        /// </summary>
        /// <param name="capsList"></param>
        public void SetRequestedVirtualCapabilities(string[] vcapsList)
        {
            if (vcapsList == null)
            {
                this.requestedVirtualCaps = null;
                this.ClearCaches();
                return;
            }

            var vcapNames = new List<string>();
            foreach (string name in vcapsList)
            {
                if (HasVirtualCapability(name))
                {
                    vcapNames.Add(name);
                }
                this.requestedVirtualCaps = vcapNames.ToArray();
            }
            ClearCaches();
        }


        /// <summary>
        /// Returns information about the running WM server and API.
        /// Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <returns></returns>
        public JSONInfoData GetInfo()
        {
            JSONInfoData info = null;

            HttpResponseMessage response = null;
            try
            {
                response = c.GetAsync(createURL("/v2/getinfo/json")).Result;
                if (response != null && response.IsSuccessStatusCode && response.Content != null)
                {
                    info = response.Content.ReadAsAsync<JSONInfoData>().Result;
                    // check if caches must be cleaned
                    if(!checkData(info)){
                        throw new WmException("Server returned empty data or a wrong json format");
                    }

                    ClearCachesIfNeeded(info.Ltime);
                }
            }
            catch (Exception e)
            {
                throw new WmException("Error getting informations for WM server: " + e.Message, e);
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }

            }

            return info;
        }

        /// <summary>
        /// Searches WURFL device data using the given user-agent for detection.
        /// Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <param name="userAgent">
        /// A user-agent string
        /// </param>
        /// <returns>device data detected using the given user agent</returns>
        public JSONDeviceData LookupUserAgent(string userAgent)
        {
            JSONDeviceData device = null;

            // First, do a cache lookup
            if (uaCache != null)
            {
                device = uaCache.GetEntry(userAgent);
                if (device != null)
                {
                   // Console.WriteLine(device.Capabilities["device_id"]);
                    return device;
                }
            }

            // Cache does not contain device, try a server lookup
            Request req = new Request();
            addCapabilitiesToRequest(req);

            req.Lookup_headers.Add(UserAgentHeaderName, userAgent);

            HttpResponseMessage response = null;
            try
            {
                response = c.PostAsJsonAsync<Request>(createURL("/v2/lookupuseragent/json"), req).Result;
                if (response != null && response.IsSuccessStatusCode && response.Content != null)
                {
                    device = response.Content.ReadAsAsync<JSONDeviceData>().Result;
                    // Check if cache must be cleared before adding to it
                    ClearCachesIfNeeded(device.Ltime);
                    SafePutDevice(uaCache, userAgent, device);
                }
                return device;

            }
            catch (Exception e)
            {
                throw new WmException("Error retrieving device data: " + e.Message, e);
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        /// <summary>
        /// Searches WURFL device data using the given WURFL device id for detection.
        /// Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <param name="wurflID"></param>
        /// <returns>device data detected using the given WURFL ID</returns>
        public JSONDeviceData LookupDeviceID(string wurflID)
        {
            JSONDeviceData device = null;

            // First, do a cache lookup
            if (devIdCache != null)
            {
                device = devIdCache.GetEntry(wurflID);
                if (device != null)
                {
                    return device;
                }
            }



            // No device found in cache, let's try a server lookup
            Request req = new Request();
            req.Wurfl_id = wurflID;

            device = internalLookup(req, "/v2/lookupdeviceid/json");
            // check if cache must be cleared before adding device to it
            ClearCachesIfNeeded(device.Ltime);
            SafePutDevice(devIdCache, wurflID, device);

            return device;
        }

        /// <summary>
        /// Searches WURFL device data using the given HttpRequest headers for detection.
        /// Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
#if UNIT_TESTS
        public JSONDeviceData LookupRequest(HttpRequestBase request)
#else
        public JSONDeviceData LookupRequest(HttpRequest request)
#endif

        {
            JSONDeviceData device = null;

            // First, do a cache lookup
            var cacheKey = GetUserAgentCacheKey(request.Headers);
            if (uaCache != null)
            {
                device = uaCache.GetEntry(cacheKey);
                if (device != null)
                {
                    return device;
                }
            }

            // Device, not found in cache, let's try a server lookup
            Request req = new Request();
            addCapabilitiesToRequest(req);

            if (request.Headers != null)
            {
                for (int i = 0; i < ImportantHeaders.Length; i++)
                {
                    String name = ImportantHeaders[i];
                    String header = request.Headers[name];

                    if (header!= null && !"".Equals(header))
                    {
                        req.Lookup_headers.Add(name, header);
                    }
                }
            }

            HttpResponseMessage response = null;
            try
            {
                response = c.PostAsJsonAsync<Request>(createURL("/v2/lookuprequest/json"), req).Result;
                if (response != null && response.IsSuccessStatusCode && response.Content != null)
                {
                    device = response.Content.ReadAsAsync<JSONDeviceData>().Result;
                    if (device.Error != null && device.Error.Length > 0)
                    {
                        throw new WmException("Received error from WM server: " + device.Error);
                    }
                }

                // Check if cache must be cleaned before adding device to cache
                ClearCachesIfNeeded(device.Ltime);
                SafePutDevice(uaCache, cacheKey, device);

                return device;

            }
            catch (Exception e)
            {
                if (e is WmException)
                {
                    throw e;
                }
                throw new WmException("Error retrieving device data: " + e.Message, e);
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        /// <summary>
        /// Retrieves a list of all device manufacturers.
        /// Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <returns>A list of all device manufacturers</returns>
        public string[] GetAllDeviceMakes()
        {
            LoadDeviceMakesData();
            return deviceMakes;
        }

        /// <summary>
        /// Retrieves all device names for the given manufacturer.
        /// Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <param name="make"></param>
        /// <returns>An array of JSONModelMktName, an object that holds device model and marketing names</returns>
        public JSONModelMktName[] GetAllDevicesForMake(String make)
        {
            LoadDeviceMakesData();

            if (deviceMakesMap.ContainsKey(make))
            {
                IList<JSONModelMktName> modelMktNames = deviceMakesMap[make];
                JSONModelMktName[] strModelMktNames = new JSONModelMktName[modelMktNames.Count];
                modelMktNames.CopyTo(strModelMktNames, 0);
                return strModelMktNames;
            }
            else
            {
                throw new WmException(String.Format("Error getting data from WM server: %s does not exist", make));
            }

        }

        // Performs server lookup POSTs. It does not use cache, it must be done by caller methods.
        private JSONDeviceData internalLookup(Request req, string routePath)
        {
            JSONDeviceData device = null;
            addCapabilitiesToRequest(req);

            HttpResponseMessage response = null;
            try
            {
                response = c.PostAsJsonAsync<Request>(createURL(routePath), req).Result;
                if (response != null && response.IsSuccessStatusCode && response.Content != null)
                {
                    device = response.Content.ReadAsAsync<JSONDeviceData>().Result;
                    if (device.Error != null && device.Error.Length > 0)
                    {
                        throw new WmException("Received error from WM server: " + device.Error);
                    }
                }

            }
            catch (Exception e)
            {
                if (e is WmException)
                {
                    throw e;
                }
                throw new WmException("Error retrieving device data: " + e.Message, e);
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
            }

            return device;
        }

        // sets both static and virtual capabilities into request object
        private void addCapabilitiesToRequest(Request req)
        {
            if (requestedStaticCaps != null && requestedStaticCaps.Length > 0)
            {
                req.Requested_caps = requestedStaticCaps;
            }

            if (requestedVirtualCaps != null && requestedVirtualCaps.Length > 0)
            {
                req.Requested_vcaps = requestedVirtualCaps;
            }
        }


        /// <summary>
        /// Diposes all internal resources used (ie: the instance of HttpClient, internal caches, etc.).
        /// Calling API methods after this method will cause Exception
        /// </summary>
        public void DestroyConnection()
        {
            ClearCaches();

            if (c != null)
            {
                c.Dispose();
            }
            this.deviceMakes = null;
            this.deviceMakesMap = null;
            this.deviceOSes = null;
            this.deviceOsVersionsMap = null;
            c = null;
        }

        /// <summary>
        /// Returns the current client version
        /// </summary>
        /// <returns></returns>
        public string GetApiVersion()
        {
            return "2.0.1";
        }

        /// <summary>
        /// Lists all device OSes in WM API. Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <returns>an array of all devices device_os capabilities in WM server</returns>
        public String[] GetAllOSes()
        {
            LoadDeviceOsesData();
            return deviceOSes;
        }

        /// <summary>
        /// returns an array containing device_os_version for the given os_name.
        /// Throws WmClientException in case any client related problem occurs.
        /// </summary>
        /// <param name="osName">A device OS name</param>
        /// <returns>an array of all devices OS versions for the given OS name</returns>
        public String[] GetAllVersionsForOS(String osName)
        {
            LoadDeviceOsesData();
            if (deviceOsVersionsMap.ContainsKey(osName))
            {
                IList<String> osVersions = deviceOsVersionsMap[osName];
                // Old school way to avoid modification errors.
                for (int i = osVersions.Count - 1; i >= 0; i--)
                {
                    if (osVersions[i].Length == 0)
                    {
                        osVersions.RemoveAt(i);
                    }
                }

                String[] arrDevOsVersions = new String[osVersions.Count];
                if (osVersions.Count > 0)
                {
                    osVersions.CopyTo(arrDevOsVersions, 0);
                }
                return arrDevOsVersions;
            }
            else
            {
                throw new WmException(String.Format("Error getting data from WM server: %s does not exist", osName));
            }
        }


#if UNIT_TESTS
        public void ClearCachesIfNeeded(string ltime)
#else
        internal void ClearCachesIfNeeded(string ltime)
#endif
        {
            if (ltime != null && !ltime.Equals(this.ltime))
            {
                this.ltime = ltime;
                ClearCaches();
            }
        }

        private void ClearCaches()
        {
            if (uaCache != null)
            {
                uaCache.Clear();
            }

            if (devIdCache != null)
            {
                devIdCache.Clear();
            }

            lock (_lock)
            {
                this.makeModels = new JSONMakeModel[0];
            }

            lock (_deviceMakesLock)
            {
                this.deviceMakes = new string[0];
                this.deviceMakesMap = new Dictionary<String, IList<JSONModelMktName>>();
            }

            lock (_deviceOSesLock)
            {
                this.deviceOSes = new string[0];
                this.deviceOsVersionsMap = new Dictionary<String, IList<string>>();
            }
        }

        private string GetUserAgentCacheKey(NameValueCollection headers)
        {
            string key = "";

            if (headers == null)
            {
                throw new WmException("No User-Agent provided");
            }

            // Using important headers array preserves header name order
            foreach (string h in importantHeaders)
            {
                var hkey = headers[h];
                if (key != null)
                {
                    key += headers[h];
                }
            }
            return key;
        }

        private void SafePutDevice(LRUCache<String, JSONDeviceData> cache, String key, JSONDeviceData device)
        {
            if (cache != null)
            {
                cache.PutEntry(key, device);
            }
        }

        public int[] GetActualCacheSizes()
        {
            int uaS = 0;
            int deS = 0;
            if (uaCache != null)
            {
                uaS = uaCache.Size();
            }

            if (devIdCache != null)
            {
                deS = devIdCache.Size();
            }
            return new int[] { deS, uaS };
        }

        private void LoadDeviceMakesData()
        {
            // If deviceMakes cache has values everything has already been loaded, thus we exit
            lock (_deviceMakesLock)
            {
                if (this.deviceMakes != null && this.deviceMakes.Length > 0)
                {
                    return;
                }
            }

            // No values already loaded, gotta load 'em all!
            JSONMakeModel[] modelMktNames;
            Request req = new Request();
            HttpResponseMessage response = null;
            try
            {
                response = c.GetAsync(createURL("/v2/alldevices/json")).Result;
                if (response != null && response.IsSuccessStatusCode && response.Content != null)
                {
                    modelMktNames = response.Content.ReadAsAsync<JSONMakeModel[]>().Result;
                    IDictionary<String, IList<JSONModelMktName>> dmMap = new Dictionary<String, IList<JSONModelMktName>>();
                    ISet<String> devMakes = new HashSet<String>();
                    foreach (JSONMakeModel mkModel in modelMktNames)
                    {
                        if (!dmMap.ContainsKey(mkModel.Brand_Name))
                        {
                            dmMap[mkModel.Brand_Name] = new List<JSONModelMktName>();
                            devMakes.Add(mkModel.Brand_Name);
                        }

                        IList<JSONModelMktName> mdMkNames = dmMap[mkModel.Brand_Name];
                        mdMkNames.Add(new JSONModelMktName(mkModel.Model_Name, mkModel.Marketing_Name));
                    }

                    lock (_deviceMakesLock)
                    {
                        this.deviceMakesMap = dmMap;
                        this.deviceMakes = new String[devMakes.Count];
                        devMakes.CopyTo(this.deviceMakes, 0);
                    }
                }
            }
            catch (Exception e)
            {
                throw new WmException("An error occurred getting makes and model data " + e.Message, e);
            }
        }

        private void LoadDeviceOsesData()
        {
            lock (_deviceOSesLock)
            {
                if (deviceOSes != null && deviceOSes.Length > 0)
                {
                    return;
                }
            }

            try
            {
                // No values already loaded, gotta load 'em all!
                JSONDeviceOsVersions[] devOsesversions;
                Request req = new Request();
                HttpResponseMessage response = null;
                response = c.GetAsync(createURL("/v2/alldeviceosversions/json")).Result;
                if (response != null && response.IsSuccessStatusCode && response.Content != null)
                {
                    devOsesversions = response.Content.ReadAsAsync<JSONDeviceOsVersions[]>().Result;
                    IDictionary<String, IList<String>> dmMap = new Dictionary<String, IList<String>>();
                    ISet<String> devOSes = new HashSet<String>();
                    foreach (JSONDeviceOsVersions osVer in devOsesversions)
                    {
                        if (!devOSes.Contains(osVer.Device_os))
                        {
                            devOSes.Add(osVer.Device_os);
                        }

                        if (!dmMap.ContainsKey(osVer.Device_os))
                        {
                            dmMap[osVer.Device_os] = new List<String>();
                        }
                        dmMap[osVer.Device_os].Add(osVer.Device_os_version);
                    }
                    lock (_deviceOSesLock)
                    {
                        this.deviceOSes = new string[devOSes.Count];
                        devOSes.CopyTo(this.deviceOSes, 0);
                        this.deviceOsVersionsMap = dmMap;
                    }
                }
            }
            catch (Exception e)
            {
                throw new WmException("An error occurred getting device os name and version data " + e.Message, e);
            }
        }
    }
}