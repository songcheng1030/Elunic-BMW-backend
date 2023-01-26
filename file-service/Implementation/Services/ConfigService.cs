using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;


namespace AIQXFileService.Implementation.Services
{
    public class ConfigService
    {
        private IConfiguration _config;

#nullable enable
        private static ConfigService? Instance;
#nullable disable

        public ConfigService(IConfiguration config)
        {
            _config = config;
            ConfigService.Instance = this;
        }

        public static ConfigService GetInstance()
        {
            if (Instance == null)
            {
                throw new NullReferenceException("The singelton of ConfigService has not yet been created!");
            }

            return Instance;
        }

        private string Get(string key) => Environment.GetEnvironmentVariable(key) ?? _config[key];

        public bool isDev() => Get("ASPNETCORE_ENVIRONMENT") == "Development";

        public string TablePrefix() => Environment.GetEnvironmentVariable("APP_TABLE_PREFIX") ?? _config["APP_TABLE_PREFIX"] ?? "";

        public int HttpPort() => int.Parse(Environment.GetEnvironmentVariable("APP_PORT_FILE") ?? Environment.GetEnvironmentVariable("APP_PORT") ?? _config["APP_PORT"]);

        public string SqlConnStr() => Environment.GetEnvironmentVariable("APP_MSSQL_CONNSTR") ?? _config["APP_MSSQL_CONNSTR"];

        public string StoragePath() => Environment.GetEnvironmentVariable("APP_STORAGE_PATH") ?? _config["APP_STORAGE_PATH"] ?? "data";

        public string IconsPath() => Path.Join("./", "icons");

        public string ServiceUrl() => Environment.GetEnvironmentVariable("APP_SERVICE_URL") ?? _config["APP_SERVICE_URL"];

        public string mediaTokenPrivateKey() => Environment.GetEnvironmentVariable("APP_MEDIA_TOKEN_PRIVATE_KEY") ?? _config["APP_MEDIA_TOKEN_PRIVATE_KEY"];

        public string mediaTokenPublicKey() => Environment.GetEnvironmentVariable("APP_MEDIA_TOKEN_PUBLIC_KEY") ?? _config["APP_MEDIA_TOKEN_PUBLIC_KEY"];

        public string Azure_Storage_Connection() => Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") ?? _config["AZURE_STORAGE_CONNECTION_STRING"];

        public string Azure_Storage_Name() => Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME") ?? _config["AZURE_STORAGE_CONTAINER_NAME"];

        // TODO
        public string[] ThumbnailFileIcons() =>
            _config.GetSection(nameof(ThumbnailFileIcons)).AsEnumerable()
            .Where(_ => _.Value != null)
            .Select(_ => _.Value)
            .ToArray();

        public int MaxImageSize() => int.Parse(Environment.GetEnvironmentVariable("APP_MAX_IMG_SIZE") ?? _config["APP_MAX_IMG_SIZE"] ?? "1024");
    }
}
