using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public UserModel(string name)
        {
            Name = name;
        }

    }
}