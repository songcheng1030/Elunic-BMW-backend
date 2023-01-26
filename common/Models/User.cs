using System;
using System.Collections.Generic;
using System.IO;

namespace AIQXCommon.Models
{
    public class UserDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Mail { get; set; }
        public string Language { get; set; }
    }
}