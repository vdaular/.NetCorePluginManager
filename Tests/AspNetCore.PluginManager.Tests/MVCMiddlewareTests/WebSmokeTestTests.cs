﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *  .Net Core Plugin Manager is distributed under the GNU General Public License version 3 and  
 *  is also available under alternative licenses negotiated directly with Simon Carter.  
 *  If you obtained Service Manager under the GPL, then the GPL applies to all loadable 
 *  Service Manager modules used on your system as well. The GPL (version 3) is 
 *  available at https://opensource.org/licenses/GPL-3.0
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 *  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *  See the GNU General Public License for more details.
 *
 *  The Original Code was created by Simon Carter (s1cart3r@gmail.com)
 *
 *  Copyright (c) 2018 - 2020 Simon Carter.  All Rights Reserved.
 *
 *  Product:  AspNetCore.PluginManager.Tests
 *  
 *  File: WebSmokeTestTests.cs
 *
 *  Purpose:  Test units for MVC WebSmokeTest Middleware class
 *
 *  Date        Name                Reason
 *  09/06/2020  Simon Carter        Initially Created
 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using AspNetCore.PluginManager.DemoWebsite.Classes;

using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PluginManager.Abstractions;

using SharedPluginFeatures;

using WebSmokeTest.Plugin;

using pm = PluginManager.Internal;


namespace AspNetCore.PluginManager.Tests.MiddlewareTests
{
    [TestClass]
    public class WebSmokeTestTests : TestBasePlugin
    {
        [TestInitialize]
        public void InitialiseSmokeTestPluginManager()
        {
            InitializeSmokeTestPluginManager();
        }

        [TestMethod]
        public void LoadSmokeTests()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            ILogger logger = new Logger();

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(null, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);
            }
        }

        [TestMethod]
        public void LoadSmokeTests_ClearCache_LoadFromFile()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();
            ILogger logger = new Logger();


            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(null, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                sut.ClearCache();
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_InvalidRequest_SiteId_Returns404()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/SiteId";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();


            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(null, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);

                await sut.Invoke(httpContext);

                Assert.AreEqual(404, httpResponse.StatusCode);
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_ValidRequest_SiteId_Returns200()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/SiteId/";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();
            bool nextDelegateCalled = false;
            RequestDelegate requestDelegate = async (context) => { nextDelegateCalled = true; await Task.Delay(0); };

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(requestDelegate, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);
                Assert.IsFalse(nextDelegateCalled);

                await sut.Invoke(httpContext);

                Assert.AreEqual(200, httpResponse.StatusCode);
                byte[] data = new byte[httpResponse.Body.Length];
                httpResponse.Body.Position = 0;
                httpResponse.Body.Read(data, 0, data.Length);
                string id = Encoding.UTF8.GetString(data);

                Assert.AreEqual("8D801A6912AF939", id);
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_ValidRequest_SiteId_Disabled_Returns200()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/SiteId/";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();
            bool nextDelegateCalled = false;
            RequestDelegate requestDelegate = async (context) => { nextDelegateCalled = true; await Task.Delay(0); };

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(requestDelegate, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                sut.Enabled = false;
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);

                await sut.Invoke(httpContext);
                Assert.IsTrue(nextDelegateCalled);
                Assert.AreEqual(200, httpResponse.StatusCode);

                byte[] data = new byte[httpResponse.Body.Length];
                httpResponse.Body.Position = 0;
                httpResponse.Body.Read(data, 0, data.Length);
                string id = Encoding.UTF8.GetString(data);

                Assert.AreEqual("", id);
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_ValidRequest_Count_Disabled_Returns200()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/Count/";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();
            bool nextDelegateCalled = false;
            RequestDelegate requestDelegate = async (context) => { nextDelegateCalled = true; await Task.Delay(0); };

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(requestDelegate, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                sut.Enabled = false;
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);

                await sut.Invoke(httpContext);
                Assert.IsTrue(nextDelegateCalled);
                Assert.AreEqual(200, httpResponse.StatusCode);

                byte[] data = new byte[httpResponse.Body.Length];
                httpResponse.Body.Position = 0;
                httpResponse.Body.Read(data, 0, data.Length);
                string id = Encoding.UTF8.GetString(data);

                Assert.AreEqual("", id);
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_ValidRequest_Count_Enabled_Returns200()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/Count/";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();
            bool nextDelegateCalled = false;
            RequestDelegate requestDelegate = async (context) => { nextDelegateCalled = true; await Task.Delay(0); };

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(requestDelegate, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);
                Assert.IsFalse(nextDelegateCalled);

                await sut.Invoke(httpContext);

                Assert.AreEqual(200, httpResponse.StatusCode);
                byte[] data = new byte[httpResponse.Body.Length];
                httpResponse.Body.Position = 0;
                httpResponse.Body.Read(data, 0, data.Length);
                string count = Encoding.UTF8.GetString(data);

                if (Int32.TryParse(count, out int actualCount))
                    Assert.IsTrue(actualCount > 1);
                else
                    throw new InvalidCastException("Failed to convert returned count");                
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_ValidRequest_RetrieveTest_Disabled_Returns200()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/Test/0";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();
            bool nextDelegateCalled = false;
            RequestDelegate requestDelegate = async (context) => { nextDelegateCalled = true; await Task.Delay(0); };

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(requestDelegate, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                sut.Enabled = false;
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);

                await sut.Invoke(httpContext);
                Assert.IsTrue(nextDelegateCalled);
                Assert.AreEqual(200, httpResponse.StatusCode);

                byte[] data = new byte[httpResponse.Body.Length];
                httpResponse.Body.Position = 0;
                httpResponse.Body.Read(data, 0, data.Length);
                string count = Encoding.UTF8.GetString(data);

                Assert.AreEqual("", count);
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_ValidRequest_RetrieveTest_Enabled_Returns200()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/Test/1";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();
            bool nextDelegateCalled = false;
            RequestDelegate requestDelegate = async (context) => { nextDelegateCalled = true; await Task.Delay(0); };

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(requestDelegate, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);

                await sut.Invoke(httpContext);
                Assert.IsFalse(nextDelegateCalled);
                Assert.AreEqual(200, httpResponse.StatusCode);
                Assert.AreEqual("application/json", httpResponse.ContentType);

                byte[] data = new byte[httpResponse.Body.Length];
                httpResponse.Body.Position = 0;
                httpResponse.Body.Read(data, 0, data.Length);
                string test = Encoding.UTF8.GetString(data);

                Assert.IsTrue(test.Contains("Please try again"));
                Assert.IsTrue(test.Contains("Method\":\"POST"));
            }
        }

        [TestMethod]
        public async Task RetrieveUniqueId_ValidRequest_RetrieveTest_Enabled_InvalidTestId_Returns400()
        {
            ISettingsProvider settingsProvider = new pm.DefaultSettingProvider(Directory.GetCurrentDirectory());

            TestHttpRequest httpRequest = new TestHttpRequest();
            httpRequest.Path = "/smokeTest/Test/100000";
            TestHttpResponse httpResponse = new TestHttpResponse();

            IPluginHelperService pluginHelperServices = _testPluginSmokeTest.GetRequiredService<IPluginHelperService>();
            IPluginTypesService pluginTypesService = _testPluginSmokeTest.GetRequiredService<IPluginTypesService>();

            TestHttpContext httpContext = new TestHttpContext(httpRequest, httpResponse);
            ILogger logger = new Logger();
            bool nextDelegateCalled = false;
            RequestDelegate requestDelegate = async (context) => { nextDelegateCalled = true; await Task.Delay(0); };

            using (WebSmokeTestMiddleware sut = new WebSmokeTestMiddleware(requestDelegate, pluginHelperServices,
                pluginTypesService, settingsProvider, logger))
            {
                List<WebSmokeTestItem> smokeTests = sut.SmokeTests;

                Assert.IsTrue(smokeTests.Count > 1);

                await sut.Invoke(httpContext);
                Assert.IsFalse(nextDelegateCalled);
                Assert.AreEqual(400, httpResponse.StatusCode);
                Assert.IsNull(httpResponse.ContentType);

                byte[] data = new byte[httpResponse.Body.Length];
                httpResponse.Body.Position = 0;
                httpResponse.Body.Read(data, 0, data.Length);
                string test = Encoding.UTF8.GetString(data);

                Assert.IsTrue(String.IsNullOrEmpty(test));
            }
        }

        [SmokeTest]
        public void LoadTestData_InvalidReturnType_Void()
        {

        }

        [SmokeTest]
        public string LoadTestData_InvalidReturnType_String()
        {
            return String.Empty;
        }

        [SmokeTest]
        public WebSmokeTestItem LoadTestData_ValidReturnType_WebSmokeTestItem()
        {
            return new WebSmokeTestItem(
                "api/Restricted/",
                "GET",
                200,
                10,
                "Api Restricted Returns Test",
                "",
                new List<string> { "Test" });
        }
    }
}