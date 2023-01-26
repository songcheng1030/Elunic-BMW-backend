using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AIQXCommon.Middlewares;
using AIQXCommon.Models;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AIQXCommon.Services
{
    public class FileService
    {
        private readonly string _url;
        private readonly string _serverId;
        private readonly IJwtEncoder _encoder;
        private readonly HttpClient _client = new HttpClient();
        public FileService(string url, string serverId)
        {
            _url = url;
            _serverId = serverId;

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            JsonSerializer jsonSerializer = new JsonSerializer
            {
                ContractResolver = contractResolver,
            };
            jsonSerializer.Converters.Add(new StringEnumConverter());
            IJsonSerializer serializer = new JsonNetSerializer(jsonSerializer);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            _encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
        }

        public async Task LockFile(string id, AuthInformation authInfo)
        {
            var url = $"{_url}/v1/files/{id}";

            var payload = new Dictionary<string, object>
                {
                    { "id", _serverId },
                    { "original_claims", authInfo }
                };
            var token = _encoder.Encode(payload, Environment.GetEnvironmentVariable("APP_INTERNAL_TOKEN_SECRET"));

            var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/metadata");
            request.Headers.Add("X-Internal-Request-Token", token);
            HttpResponseMessage response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<DataResponseSchema<FileMetadata, DataResponseMeta>>(resp).Data;

            dto.Tags.Add(FileTag.LOCKED);
            var json = JsonConvert.SerializeObject(new
            {
                Tags = dto.Tags
            });
            HttpContent body = new StringContent(json, Encoding.UTF8, "application/json");
            body.Headers.Add("X-Internal-Request-Token", token);

            response = await _client.PutAsync(url, body);
            response.EnsureSuccessStatusCode();
        }
    }
}
