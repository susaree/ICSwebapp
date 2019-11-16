


namespace InvoiceUploader.Models
{
    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
     

        public Customer(string name)
        {
            Name = name;
        }
        
    }
}