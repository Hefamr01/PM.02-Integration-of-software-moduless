using System;
using System.Collections.Generic;
using System.Text;

    namespace TechModule.Models
    {
        public class AuthResponse
        {
            public bool success { get; set; }
            public string token { get; set; }
            public User user { get; set; }
            public string message { get; set; }
        }

        public class LoginRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class RegisterRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string full_name { get; set; }
            public int role_id { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public int? department_id { get; set; }
        }
    }
