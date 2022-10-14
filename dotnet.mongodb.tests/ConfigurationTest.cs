using dotnet.mongodb.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet.mongodb.tests
{
    public class ConfigurationTest
    {
        public void GetSettings()
        {
            var dbConnectString = ConfigHelper.Instance.GetConnectionString("mongodb");
            Assert.NotNull(dbConnectString);

            var appSettings = ConfigHelper.Instance.GetAppSettings("Logging:LogLevel:Default", "");
            Assert.NotNull(appSettings);
        }
    }
}
