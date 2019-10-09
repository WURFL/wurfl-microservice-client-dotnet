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
using System.Collections.Generic;

namespace Wmclient
{
    /// <summary>
    /// Holds informations about wurfl microservice server and API
    /// </summary>
    public class JSONInfoData
    {

        /// <summary>
        /// Version of WURFL API used by the WM server
        /// </summary>
        public string Wurfl_api_version { get; set; }

        /// <summary>
        /// Version of the WM server
        /// </summary>
        public string Wm_version { get; set; }

        /// <summary>
        /// Information about the WURFL file used by the WM server
        /// </summary>
        public string Wurfl_info { get; set; }

        /// <summary>
        /// List of important headers used when detecting a device using an HTTP Request
        /// </summary>
        public string[] Important_headers { get; set; }

        /// <summary>
        /// List of static capabilities supported by currently running server
        /// </summary>
        public string[] Static_caps { get; set; }

        /// <summary>
        /// List of virtual capabilities supported by currently running server
        /// </summary>
        public string[] Virtual_caps { get; set; }

        /// <summary>
        /// Time of the WURFL.xml last load on server
        /// </summary>
        public string Ltime { get; set; }


    }

    /// <summary>
    /// Holds data relevant for the HTTP request that will be sent to WM server
    /// </summary>
    public class Request
    {
        private IDictionary<string, string> lookup_headers;
        private string[] requested_caps;
        private string[] requested_vcaps;
        private string wurfl_id;
        private string tac_code;

        public Request()
        {
            lookup_headers = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Lookup_headers
        {
            get { return lookup_headers; }
            set { lookup_headers = value; }
        }

        /// <summary>
        /// List of WURFL capabilities requested to the server
        /// </summary>
        public string[] Requested_caps
        {
            get { return requested_caps; }
            set { requested_caps = value; }
        }

        /// <summary>
        /// List of WURFL virtual capabilities requested to the server
        /// </summary>
        public string[] Requested_vcaps
        {
            get { return requested_vcaps; }
            set { requested_vcaps = value; }
        }

        /// <summary>
        /// WURFL Id of the requested device (used when calling LookupDeviceID API)
        /// </summary>
        public string Wurfl_id
        {
            get { return wurfl_id; }
            set { wurfl_id = value; }
        }

        /// <summary>		
        /// Not yet implemented		
        /// </summary>		
        public string Tac_code
        {
            get { return tac_code; }
            set { tac_code = value; }
        }
    }

    /// <summary>
    /// Holds the detected device data received from WM server
    /// </summary>
    public class JSONDeviceData
    {

        private Dictionary<string, string> capabilities;
        private string error;
        private int mtime;

        public int Mtime
        {
            get { return mtime; }
            set { mtime = value; }
        }

        /// <summary>
        /// Returns a message if any error occurred during device detection, or an
        /// empty string if detection task returned a correct result.
        /// </summary>
        public string Error
        {
            get { return error; }
            set { error = value; }
        }

        /// <summary>
        /// List of the capability values for the detected device.
        /// </summary>
        public Dictionary<string, string> Capabilities
        {
            get { return capabilities; }
            set { capabilities = value; }
        }


        public string APIVersion { get; set; }

        /// <summary>
        /// Time of the WURFL.xml last load on server
        /// </summary>
        public string Ltime { get; set; }
    }


    /// <summary>
    /// JSONMakeModel models simple device "identity" data in JSON format
    /// </summary>
    public class JSONMakeModel
    {
        public string Brand_Name { get; set; }
        public string Model_Name { get; set; }
        public string Marketing_Name { get; set; }
    }

    /// <summary>
    /// JSONModelMktName  holds a device's model and marketing names.
    /// </summary>
    public class JSONModelMktName
    {
        public JSONModelMktName(string modelName, string marketingName)
        {
            Model_Name = modelName;
            Marketing_Name = marketingName;
        }

        public string Model_Name { get; set; }
        public string Marketing_Name { get; set; }
    }

    /// <summary>
    /// Hold an couple of OS name and version
    /// </summary>
    public class JSONDeviceOsVersions
    {
        public string Device_os { get; set; }
        public string Device_os_version { get; set; }
    }
}