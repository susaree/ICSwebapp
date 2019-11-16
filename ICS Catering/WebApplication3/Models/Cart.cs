using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Cart
    {
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public double SubTotal { get; set; }

        public Cart(Product product, int quantity)
        {
            double subTotal = 0;
            subTotal = double.Parse(product.ListPrice) * quantity;

            Product = product;
            Quantity = quantity;
            SubTotal = subTotal;
        }
    }
}