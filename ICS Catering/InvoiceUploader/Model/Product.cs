

namespace InvoiceUploader.Models
{

    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ListPrice { get; set; }
        public string Quantity { get; set; }
        public string Category { get; set; }

        public string TaxCode { get; set; }

       
        public Product(string name, string listPrice, string quantity)
        {
           
            Name = name;
            ListPrice = listPrice;
            Quantity = quantity;
           
        }

    
    }
}