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
    public class UserService
    {
        private readonly string _url;
        private readonly HttpClient _client = new HttpClient();
        public UserService(string url)
        {
            _url = url;
        }

        public UserDto getUserByIdAsync(string id)
        {
            // TODO: Get from KeyCloak
            return new UserDto
            {
                Id = id,
                DisplayName = "Dev User",
                Username = "dev_user",
                GivenName = "Dev",
                FamilyName = "User",
                Mail = "dev_user@elunic.com",
                Language = "en"
            };
        }
    }
}
