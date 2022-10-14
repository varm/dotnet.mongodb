using Microsoft.Extensions.Configuration;

namespace dotnet.mongodb.utils
{
    /// <summary>
    /// 
    /*
     * Packages:
     * Microsoft.Extensions.Configuration
     * Microsoft.Extensions.Configuration.FileExtensions
     * Microsoft.Extensions.Configuration.Json
     */
    /// </summary>
    public class ConfigHelper
    {
        public static ConfigHelper _Instance;
        private static IConfigurationRoot _ConfigurationRoot;

        private static readonly object _ThisLock = new();
        public ConfigHelper()
        {
            _ConfigurationRoot=new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                .AddJsonFile("appsettings.json",optional:false)
                .Build();
        }
        public static ConfigHelper Instance
        {
            get
            {
                lock (_ThisLock)
                {
                    _Instance ??= new ConfigHelper();
                    return _Instance;
                }
            }
        }

        /// <summary>
        /// Get ConnectionStrings
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string? GetConnectionString(string key,string? defaultValue =null)
        {
            string result = _ConfigurationRoot.GetConnectionString(key);
            if(result == null)
                return defaultValue;
            return result;
        }

        /// <summary>
        /// Get appsettings section
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetAppSettings<T>(string key, T defaultValue)
        {
            IConfigurationSection configSection = _ConfigurationRoot.GetSection(key);
            if (configSection.Value != null)
            {
                return (T)Convert.ChangeType(configSection.Value, typeof(T));
            }
            return defaultValue;
        }
        
    }
}
