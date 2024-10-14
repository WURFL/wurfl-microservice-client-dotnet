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

namespace Wmclient
{
    /// <summary>
    /// WmException is a general purpouse exception throws whenever an unrecoverable error occurs during device detection (ie: no connection available to WM server,
    /// wrong url or port configurations, etc.
    /// </summary>
    public class WmException:Exception
    {
        /// <summary>
        /// Creates a WmClientException with the given error message
        /// </summary>
        /// <param name="message">Custom error message</param>
        public WmException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// /// Creates a WmException with the given error message and
        /// the exception that caused the current one. (ie: if uri is invalid, innerException could be an UriSyntaxException)
        /// </summary>
        /// <param name="message">Custom error message</param>
        /// <param name="innerException">Exception that caused the current one</param>
        public WmException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public static class ExceptionHelper
    {
        // This method preserves the original stack trace when an exception is thrown again ina a catch block
        public static void ReThrow(Exception ex)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
        }
    }
}
