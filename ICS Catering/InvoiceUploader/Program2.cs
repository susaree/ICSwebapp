using System;
using System.Collections.Generic;
using System.Linq;
using Sage.crmErp.x2008.Feeds;
using InvoiceUploader.Models;
using Sage.Common.Syndication;
using Sage.Integration.Client;
using System.IO;
using OfficeOpenXml;

namespace InvoiceUploader
{
    class Program2
    {
        public static string dataSourceTest2 = "http://localhost:5495/sdata/accounts50/GCRM/-/";
        public static string dataSourceTest1 = "http://dewsburypc1:5495/sdata/accounts50/GCRM/-/";
        public static string dataSourceTest = "http://icsn02:5495/sdata/accounts50/GCRM/-/";
        public static string username = "manager";
       // public static string password = "aceshot";
        public static string password = "18031700";
        static void Main(string[] args)
        {
            //  string fileName = "C:\\Users\\Susar\\source\\repos\\ICS Catering\\WebApplication3\\Content\\FRANKIES-1-Order2019-23-10--14-00-06.txt";
            int totalOrderCount = 0;
            int successCount = 0;
            int failCount = 0;
            string folder = args[0];

            //DirectoryInfo dir = new DirectoryInfo(new System.IO.FileInfo(folder).DirectoryName);
            var dir = new DirectoryInfo(folder);
            
        
            FileInfo[] files = dir.GetFiles("*.txt");
            foreach(var file in files)
            {
                totalOrderCount++;
                Console.WriteLine(file.FullName);
                string doubleSlash = @"\";
                string fileName = file.FullName;
                string split = fileName.Substring(fileName.LastIndexOf(doubleSlash));
                string compId = split.Split('-')[1];
                if (UpdateSage(fileName, compId))
                {
                    System.IO.File.Delete(file.FullName);
                    successCount++;
                } else
                {
                    
                    failCount++;
                    continue;
                }
            }

            Console.WriteLine("Total orders: " + totalOrderCount);
            Console.WriteLine("Total successful orders: " + successCount);
            Console.WriteLine("Total failed orders: " + failCount);
            Console.ReadLine();
        }

        public static bool UpdateSage(string fileName, string companyId)
        {


            var orderList = new List<Product>();
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(fileName);

                //Read the first line of text
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the lie to console window
                    Console.WriteLine(line);
                    var split = line.Split('-');
                   
                    if (split.Length > 0)
                    {
                        string name = split[1];
                        string quantity = split[2];
                        string price = split[3];
                        string tax = split[4];
                        Product product = new Product(name, price, quantity);
                        product.TaxCode = tax;
                        orderList.Add(product);

                        //Read the next line
                        line = sr.ReadLine();
                    }
                    
                }

                //close the file
                sr.Close();
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            Console.WriteLine(fileName);
            Console.WriteLine(companyId);

            int companyCount = 2000;
            int productCount = 5000;

            if (CreateSalesInvoice(companyId, companyCount, productCount, orderList))
            {
                return true;
            } else
            {
                return false;
            }
            


        }

        public static bool CreateSalesInvoice(string compId, int companyCount, int productCount, List<Product> orderList)
        {
            salesInvoiceFeedEntry salesInvoice = new salesInvoiceFeedEntry();
            salesInvoice.salesInvoiceLines = new salesInvoiceLineFeed();
            // Find a customer to associate with the new sales invoice
            salesInvoice.tradingAccount = GetCustomer(compId, companyCount);

            if (salesInvoice.tradingAccount == null)
            {
                // No customer record means we can go no further
                Console.WriteLine("Unable to find a customer record");
               // Console.ReadKey(true);
                
                return false;
            }


            foreach (Product product in orderList)
            {


                // Lookup a commodity to use on the new sales invoice
                commodityFeedEntry commodity = GetCommodity(product.Name, productCount);
                if (commodity == null)
                {
                    // No commodity record means we go no further
                    Console.WriteLine("Unable to find a commodity record");
                   // Console.ReadKey(true);
                    
                    return false;
                }
                
                commodityFeedEntry commodityReference = new commodityFeedEntry();
                commodityReference.UUID = commodity.UUID;

                taxCodeFeedEntry taxCode = GetTaxCode(product.TaxCode, 2);
           
                if (taxCode == null)
                {
                    // No record means we go no further
                    Console.WriteLine("Unable to find a tax code record");
                  //  Console.ReadKey(true);
                    
                    return false;
                }

                taxCodeFeedEntry taxReference = new taxCodeFeedEntry();
                taxReference.UUID = taxCode.UUID;
                salesInvoice.taxCodes = new taxCodeFeed();
                salesInvoice.taxCodes.Entries.Add(taxReference);

                // Create a new sale invoice line using the commodity we just looked up
                salesInvoiceLineFeedEntry orderLine = new salesInvoiceLineFeedEntry();
                orderLine.type = "Standard";
                orderLine.text = commodity.description;
                orderLine.commodity = commodityReference;
                orderLine.quantity = Convert.ToDecimal(product.Quantity);
                 orderLine.actualPrice = Convert.ToDecimal(product.ListPrice)/orderLine.quantity;
                // orderLine.netTotal = orderLine.quantity * orderLine.actualPrice;
                orderLine.netTotal = Convert.ToDecimal(product.ListPrice);
                orderLine.taxCodes = new taxCodeFeed();
                orderLine.taxCodes.Entries.Add(taxReference);

                // Associate the lines with our invoice

                salesInvoice.salesInvoiceLines.Entries.Add(orderLine);


            }

            // Now we have constructed our new invoice we can submit it using the HTTP POST verb  
            //string url = "http://localhost:5495/sdata/accounts50/GCRM/-/salesInvoices";
            //string url = "http://dewsburypc1:5495/sdata/accounts50/GCRM/-/salesInvoices";
            string url = dataSourceTest + "salesInvoices";
            SDataUri salesInvoiceUri = new SDataUri(url);

            SDataRequest invoiceRequest = new SDataRequest(salesInvoiceUri.Uri, salesInvoice, Sage.Integration.Messaging.Model.RequestVerb.POST);
            invoiceRequest.Username = username;
            invoiceRequest.Password = password;

            // IF successful the POST operation will provide us with a the newly created sales invoice
            salesInvoiceFeedEntry savedSalesInvoice = new salesInvoiceFeedEntry();
            invoiceRequest.RequestFeedEntry<salesInvoiceFeedEntry>(savedSalesInvoice);


            if (invoiceRequest.IsStatusValidForVerb)
            {

                Console.WriteLine(string.Format("Successfully created sales invoice {0}", savedSalesInvoice.reference));
                
            }
            else
            {
                // There was a problem
                Console.WriteLine("Create failed. Response was {0}", invoiceRequest.HttpStatusCode.ToString());
                if (invoiceRequest.Diagnoses != null)
                {
                    foreach (Diagnosis diagnosis in invoiceRequest.Diagnoses)
                        Console.WriteLine(diagnosis.Message);
                   
                    return false;
                }
            }
            return true;

        }



        static tradingAccountFeedEntry GetCustomer(string companyId, int companyCount)
        {
            // Look up the first customer record 
            // Sage.Common.Syndication.SDataUri accountUri = new Sage.Common.Syndication.SDataUri();
            string customerUrl = dataSourceTest + "tradingaccounts";
        
            SDataUri accountUri = new SDataUri(customerUrl);

            accountUri.Where = "customerSupplierFlag eq 'Customer'";
            accountUri.Count = companyCount;

            SDataRequest accountRequest = new SDataRequest(accountUri.Uri);
            accountRequest.AllowPromptForCredentials = false;
            accountRequest.Username = username;
            accountRequest.Password = password;

            tradingAccountFeed accounts = new tradingAccountFeed();
            accountRequest.RequestFeed<tradingAccountFeedEntry>(accounts);

            // If we found a customer record return it
            if (accountRequest.IsStatusValidForVerb && accounts.Entries != null && accounts.Entries.Count > 0)
            {

                foreach (tradingAccountFeedEntry account in accounts.Entries)
                {
                  //  Console.WriteLine(account.Id);
                    Console.WriteLine(account.reference);
                   // Console.WriteLine(string.Format(account.reference2));
                    
                    if (account.reference.Equals(companyId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine(string.Format("name: {0}", account.Id));
                        Console.WriteLine(string.Format("name: {0}", account.UUID));
                        return account;
                    }
                    else
                    {

                        continue;
                    }

                }



                return null;

            }

            else
            {
                // There was a problem
                Console.WriteLine("Account lookup failed. Response was {0}", accountRequest.HttpStatusCode.ToString());
                if (accountRequest.Diagnoses != null)
                {
                    foreach (Diagnosis diagnosis in accountRequest.Diagnoses)
                        Console.WriteLine(diagnosis.Message);
                    
                }
                
                return null;
            }
        }

        static commodityFeedEntry GetCommodity(string productName, int productCount)
        {
            // Look up the first commodity (product) record 
            string url = dataSourceTest + "commodities";
         
            SDataUri commodityUri = new SDataUri(url);
            commodityUri.Count = productCount;

            SDataRequest commodityRequest = new SDataRequest(commodityUri.Uri);
            commodityRequest.Username = username;
            commodityRequest.Password = password;

            commodityFeed commodities = new commodityFeed();
            commodityRequest.RequestFeed<commodityFeedEntry>(commodities);

            // If we found a record return it
            if (commodityRequest.IsStatusValidForVerb && commodities.Entries != null && commodities.Entries.Count > 0)

            {
                foreach (commodityFeedEntry commodity in commodities.Entries)
                {
                    if (commodity.name.Equals(productName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine(string.Format("name: {0}", commodity.name));
                        Console.WriteLine(string.Format("name: {0}", commodity.UUID));
                        return commodity;
                        
                    }
                    else
                    {

                        continue;
                    }

                }

                
                return null;
            }

            else
            {
                // There was a problem
                Console.WriteLine("Commodity lookup failed. Response was {0}", commodityRequest.HttpStatusCode.ToString());
                if (commodityRequest.Diagnoses != null)
                {
                    foreach (Diagnosis diagnosis in commodityRequest.Diagnoses)
                        Console.WriteLine(diagnosis.Message);
                    
                    
                }
                

                return null;
            }
        }


        static taxCodeFeedEntry GetTaxCode(string productCode, int productCount)
        {
            // Look up the tax code record 
            string taxUrl = dataSourceTest + "taxcodes";

            
            SDataUri taxCodeUri = new SDataUri(taxUrl);
            //  taxCodeUri.Where =  "reference eq 'T1'";
            taxCodeUri.Count = 2;
          

            SDataRequest taxcodeRequest = new SDataRequest(taxCodeUri.Uri);
            taxcodeRequest.Username = username;
            taxcodeRequest.Password = password;
            // taxcodeRequest.Password = "18031700";


            taxCodeFeed taxcodes = new taxCodeFeed();

            taxcodeRequest.RequestFeed<taxCodeFeedEntry>(taxcodes);

            // If we found a customer record return it
            if (taxcodeRequest.IsStatusValidForVerb && taxcodes.Entries != null && taxcodes.Entries.Count > 0)
            {
                if (productCode.Equals(taxcodes.Entries[0].reference))
                {
                    return taxcodes.Entries[0];
                } else
                {
                    return taxcodes.Entries[1];
                }
                
            }
           
            else
            {
                // There was a problem
                Console.WriteLine("Tax code lookup failed. Response was {0}", taxcodeRequest.HttpStatusCode.ToString());
                if (taxcodeRequest.Diagnoses != null)
                {
                    foreach (Diagnosis diagnosis in taxcodeRequest.Diagnoses)
                        Console.WriteLine(diagnosis.Message);
                   
                }
                

                return null;
            }
        }


    }
}

