using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Order
    {

        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        public Order(string name, string id)
        {
            Name = name;
            Id = id;
            IsSelected = false;
        }
    }
}