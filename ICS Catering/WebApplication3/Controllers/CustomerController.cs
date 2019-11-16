using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication3.Models;
using System.IO;
using OfficeOpenXml;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Diagnostics;
using PagedList;
using System.Globalization;
using System.IO.Compression;

namespace WebApplication3.Controllers
{
    public class CustomerController : Controller
    {
        private ApplicationUserManager _userManager;

        public CustomerController()
        {
        }

        public CustomerController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

    

        public ActionResult CustomerList(string searchString, int? page)
        {
            CultureInfo culture = new CultureInfo("en-GB");
            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "pricelistandstock.xlsx");
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];

                var endRow = sheet.Dimension.End;


                List<UserModel> customerList = new List<UserModel>();



                for (int row = 5; row <= endRow.Row; row++)

                {
                   
                    var cell1Val = sheet.Cells[row, 1].Text;
                    var cell4Val = sheet.Cells[row, 4].Text;
                    System.Diagnostics.Debug.WriteLine(cell1Val + " - " + cell4Val);

                    if(cell1Val.Equals("Price List:"))
                    {
                        if (cell4Val.Equals("ZZZ"))
                        {
                            break;
                        }

                        UserModel customer = new UserModel(cell4Val);
                        customer.Id = sheet.Cells[row - 1, 4].Text;
                        System.Diagnostics.Debug.WriteLine(customer.Name);
                        customerList.Add(customer);
                    } else if (cell4Val.Equals("ZZZ"))
                    {
                        break;
                    }


                }



                pck.Dispose();
                if (!String.IsNullOrEmpty(searchString))
                {
                    var searchedList = new List<UserModel>();
                    foreach (var customer in customerList)
                    {

                        if (culture.CompareInfo.IndexOf(customer.Name, searchString, System.Globalization.CompareOptions.IgnoreCase) >= 0)
                        {
                            searchedList.Add(customer);
                        }
                    }
                    // ViewBag.ProductListFull = searchedList;
                    return View(searchedList.ToPagedList(page ?? 1, 30));
                }
                else
                {
                    //ViewBag.ProductListFull = productList;
                    return View(customerList.ToPagedList(page ?? 1, 30));
                }
            }


        }



        public ActionResult OrderList(string searchString, int? page)
        {
            CultureInfo culture = new CultureInfo("en-GB");
            DirectoryInfo dir = new DirectoryInfo(new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\").DirectoryName);
            FileInfo[] files = dir.GetFiles("*.xlsx");
            List<Order> orderList = new List<Order>();
            int count = 0;
            foreach (var file in files)
            {
                if (file.Name.Contains("Order"))
                {
                    Order order = new Order(file.Name, count.ToString());
                    orderList.Add(order);
                }
                    
                count++;
            }
           

            if (!String.IsNullOrEmpty(searchString))
            {
                List<Order> searchedList = new List<Order>();
                foreach (var order in orderList)
                {

                    if (culture.CompareInfo.IndexOf(order.Name, searchString, System.Globalization.CompareOptions.IgnoreCase) >= 0)
                    {
                        searchedList.Add(order);
                    }
                }
                // ViewBag.ProductListFull = searchedList;
                return View(searchedList.ToPagedList(page ?? 1, 30));
            }
            else
            {
                //ViewBag.ProductListFull = productList;
                return View(orderList.ToPagedList(page ?? 1, 30));
            }

        }

        public ActionResult UploadOrder(string fileName)
        {
            
            var productList = new List<Product>();
           

            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + fileName);
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];

                var endRow = sheet.Dimension.End;

                for (int row = 2; row < endRow.Row+1; row++)
                {
                    string cellValId = sheet.Cells[row, 1].Text;
                    string cellValName = sheet.Cells[row, 2].Text;
                    string cellValQuant = sheet.Cells[row, 3].Text;
                    string cellValPrice = sheet.Cells[row, 4].Text;

                    if (cellValName.Equals(" "))
                    {
                        break;
                    }
                    else
                    {
                        Product product = new Product(cellValId, cellValName, cellValPrice) { Quantity = cellValQuant };
                        productList.Add(product);
                        System.Diagnostics.Debug.WriteLine(product.Name);
                    }
                }
                string textFile = fileName.Substring(0,fileName.IndexOf(".")+2);

                var file2 = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + textFile + ".txt");
                try
                {
                    //Pass the filepath and filename to the StreamWriter Constructor
                    StreamWriter sw = new StreamWriter(file2.FullName);

                    //Write a line of text
                    foreach(Product product in productList)
                    {
                        sw.WriteLine("-" + product.Name + "-" + product.Quantity + "-" + product.ListPrice);

                    }
 
                    //Close the file
                    sw.Close();
                  
                    string doubleSlash = @"\\";
                    string first = file2.FullName.Replace(doubleSlash, "lol");
                    string argument1 = first.Replace(" ", "gap");

                    /* Process p = new Process();
                     p.StartInfo.FileName = "C:\\InvoiceUploader.exe";
                     p.StartInfo.Arguments = argument1;
                     p.Start(); */

                    var processStartInfo = new ProcessStartInfo()
                    {
                        Arguments = "/c echo \"test\"",
                        FileName = @"c:\windows\system32\cmd.exe",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };

                    var process = Process.Start(processStartInfo);

                    using (var streamReader = new StreamReader(process.StandardOutput.BaseStream))
                    {
                        ViewBag.Result = streamReader.ReadToEnd();
                    }




                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
                pck.Dispose();



             


            }

        //     ViewBag.Result = "Successfully uploaded to Sage";
            return View("UploadOrder");

        }

        public void DownloadAllImportableOrders()
        {
            var file = CompressAllImportableOrders().FileDownloadName;
            Debug.WriteLine(file);
            string path = new System.IO.FileInfo(Server.MapPath("~/archive.zip")).FullName;
            Download(path);
        }

     

        [HttpGet]
        public void BatchDownload(string ItemList)
        {
           
            string[] arr = ItemList.Split(',');
            var file = CompressBatchImportableOrders(arr);
        }

        public void DownloadSelected()
        {
            string path = new System.IO.FileInfo(Server.MapPath("~/batcharchive.zip")).FullName;
            Download(path);
        }

        [HttpPost]
        public FileResult CompressBatchImportableOrders(string[] fileArr)
        {

            DirectoryInfo dir = new DirectoryInfo(new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\").DirectoryName);
            FileInfo[] files = dir.GetFiles("*.xlsx");
            var txtFiles = new List<string>();
            var archive = Server.MapPath("~/batcharchive.zip");
            var temp = Server.MapPath("~/temp/");

            int count = 0;
            foreach (var file in files)
            {
                foreach(var fname in fileArr)
                {
                    if (file.Name.Equals(fname))
                    {

                        CreateImportableOrder(file.Name);

                        string textFile2 = file.Name.Substring(0, file.Name.IndexOf(".") + 1);

                        var file2 = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + textFile2 + "txt");
                        txtFiles.Add(file2.FullName);


                        count++;
                    }
                }
               
            }

            if (System.IO.File.Exists(archive))
            {
                System.IO.File.Delete(archive);
            }

            Directory.EnumerateFiles(temp).ToList().ForEach(f => System.IO.File.Delete(f));
            txtFiles.ForEach(f => System.IO.File.Copy(f, Path.Combine(temp, Path.GetFileName(f))));
            ZipFile.CreateFromDirectory(temp, archive);

            Debug.WriteLine(count + " Downloaded Orders");

            return File(archive, "application/zip", "batcharchive.zip");
        } 

        [HttpPost]
        public FileResult CompressAllImportableOrders()
        {
            DirectoryInfo dir = new DirectoryInfo(new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\").DirectoryName);
            FileInfo[] files = dir.GetFiles("*.xlsx");
            var txtFiles = new List<string>();
            var archive = Server.MapPath("~/archive.zip");
            var temp = Server.MapPath("~/temp/");
   
            int count = 0;
            foreach (var file in files)
            {
                if (file.Name.Contains("Order"))
                {
                   
                    CreateImportableOrder(file.Name);
                    string textFile = file.Name.Substring(0, file.Name.IndexOf(".") + 1);

                    string textFile2 = file.Name.Substring(0, file.Name.IndexOf(".") + 1);

                    var file2 = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + textFile2 + "txt");
                    txtFiles.Add(file2.FullName);
                    
                  
                    count++;
                }                 
            }

            if (System.IO.File.Exists(archive))
            {
                System.IO.File.Delete(archive);
            }

            Directory.EnumerateFiles(temp).ToList().ForEach(f => System.IO.File.Delete(f));
            txtFiles.ForEach(f => System.IO.File.Copy(f, Path.Combine(temp, Path.GetFileName(f))));
            ZipFile.CreateFromDirectory(temp, archive);

            Debug.WriteLine(count + " Downloaded Orders");

            return File(archive, "application/zip", "archive.zip");
        }

        public void CreateImportableOrder(string fileName)
        {
            
            var productList = new List<Product>();


            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + fileName);
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];

                var endRow = sheet.Dimension.End;

                for (int row = 2; row < endRow.Row + 1; row++)
                {
                    string cellValId = sheet.Cells[row, 1].Text;
                    string cellValName = sheet.Cells[row, 2].Text;
                    string cellValQuant = sheet.Cells[row, 3].Text;
                    string cellValPrice = sheet.Cells[row, 4].Text;
                    string cellValTax = sheet.Cells[row, 5].Text;

                    if (cellValName.Equals(""))
                    {
                        break;
                    }
                    else
                    {
                        Product product = new Product(cellValId, cellValName, cellValPrice) { Quantity = cellValQuant, TaxCode = cellValTax };
                        productList.Add(product);
                        
                    }
                }
                string textFile = fileName.Substring(0, fileName.IndexOf(".") + 1);

                var file2 = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + textFile + "txt");
                try
                {
                    //Pass the filepath and filename to the StreamWriter Constructor
                    StreamWriter sw = new StreamWriter(file2.FullName);

                    //Write a line of text
                    foreach (Product product in productList)
                    {
                        sw.WriteLine("-" + product.Name + "-" + product.Quantity + "-" + product.ListPrice + "-" + product.TaxCode);

                    }

                    //Close the file
                    sw.Close();

                  
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
                pck.Dispose();

            }

        }

        public void DownloadImportableOrder(string fileName)
        {
            Debug.WriteLine(fileName);
            var productList = new List<Product>();


            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + fileName);
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];

                var endRow = sheet.Dimension.End;

                for (int row = 2; row < endRow.Row+1 ; row++)
                {
                    string cellValId = sheet.Cells[row, 1].Text;
                    string cellValName = sheet.Cells[row, 2].Text;
                    string cellValQuant = sheet.Cells[row, 3].Text;
                    string cellValPrice = sheet.Cells[row, 4].Text;
                    string cellValTax = sheet.Cells[row, 5].Text;

                    if (cellValName.Equals(""))
                    {
                        break;
                    }
                    else
                    {
                        Product product = new Product(cellValId, cellValName, cellValPrice) { Quantity = cellValQuant, TaxCode = cellValTax};
                        productList.Add(product);
                        System.Diagnostics.Debug.WriteLine(product.Name);
                    }
                }
                string textFile = fileName.Substring(0, fileName.IndexOf(".") + 1);

                var file2 = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + textFile + "txt");
                try
                {
                    //Pass the filepath and filename to the StreamWriter Constructor
                    StreamWriter sw = new StreamWriter(file2.FullName);

                    //Write a line of text
                    foreach (Product product in productList)
                    {
                        sw.WriteLine("-" + product.Name + "-" + product.Quantity + "-" + product.ListPrice + "-" + product.TaxCode);
                       
                    }

                    //Close the file
                    sw.Close();

                    Download(file2.FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
                pck.Dispose();

            }
              
         }    
        

        public void DownloadSageUploaderTest()
        {
            string path = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "InvoiceUploaderTest.exe").FullName;
            Download(path);
        }
        public void DownloadSageUploaderFiyaz()
        {
            string path = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "InvoiceUploaderFiyaz.exe").FullName;
            Download(path);
        }
        public void DownloadSageUploaderMain()
        {
            string path = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "InvoiceUploaderMain.exe").FullName;
            Download(path);
        }

        public void DownloadOrder(string fileName)
        {
            // string path = AppDomain.CurrentDomain.BaseDirectory + "~/Content/New Orders/" + fileName;
            string path = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + fileName).FullName;
            Download(path);

        }

        private void Download(string fName)
        {
            string path = fName;
            string name = Path.GetFileName(path);
            string ext = Path.GetExtension(path);
            //string type = "application/vnd.ms-excel";
            string type = MimeMapping.GetMimeMapping(fName);

            if (type != "")
            {
                Response.AppendHeader("content-disposition",
                    "attachment; filename=" + name);
                Response.ContentType = type;
                Response.WriteFile(path);
                Response.End();
            }

        }

        private void DownloadTxt(string fName)
        {

            string path = fName;
            string name = Path.GetFileName(path);
            string ext = Path.GetExtension(path);
            string type = MimeMapping.GetMimeMapping(fName);

            if (type != "")
                {
                    Response.AppendHeader("content-disposition",
                        "attachment; filename=" + name);
                    Response.ContentType = type;
                    Response.WriteFile(path);
                    Response.End();
                }

            
        }

        public ActionResult ClearOrders()
        {

            DirectoryInfo dir = new DirectoryInfo(new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\").DirectoryName);

            foreach (FileInfo file in dir.GetFiles("*.xlsx"))
            {
                if (file.Name.Contains("Order"))
                {
                    file.Delete();
                }

            }
            foreach (FileInfo file in dir.GetFiles("*.txt"))
            {
                if (file.Name.Contains("Order"))
                {
                    file.Delete();
                }

            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ImportPriceList(HttpPostedFileBase excelFile)
        {

            if (excelFile.ContentLength == 0 || excelFile == null)
            {
                ViewBag.Error = "Please select an excel file";
                return View("CustomerList");
            }
            else
            {
                if (excelFile.FileName.EndsWith("xls") || excelFile.FileName.EndsWith("xlsx") || excelFile.FileName.EndsWith("csv"))
                {
                    string path = System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "pricelistandstock.xlsx";
                    if (System.IO.File.Exists(path))

                        System.IO.File.Delete(path);
                    excelFile.SaveAs(path);
                    return View("ImportPriceList");
                }
                else
                {
                    ViewBag.Error = "Wrong file, rename file to 'pricelistandstock.xlsx' and try again";
                    return View("CustomerList");
                }

            }


        }

       

        public ActionResult ExportPriceList(string companyName)
        {

            string Filename = companyName + "PriceList" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";



            string path = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + Filename).FullName;


            Customer customer = new Customer(companyName);





            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet sheet = pck.Workbook.Worksheets.Add("Price List");
         
                sheet.Cells["A1:A200"].Style.Numberformat.Format = "0";
                sheet.Cells["B1:B200"].Style.Numberformat.Format = "0.00";
                var endRow = sheet.Dimension.End;



                int row = 1;
                foreach(var prod in customer.PriceList)
                {

                    var cell1Val = sheet.Cells[row, 1];
                    var cell2Val = sheet.Cells[row, 2];

                    cell1Val.Style.Numberformat.Format = "0";
                    cell2Val.Style.Numberformat.Format = "0.00";

                    cell1Val.Value = prod.Id;
                    cell2Val.Value = prod.ListPrice;

                    if (cell1Val.Equals(""))
                    {
                        break;
                    }

                    row++;
                }

                var xlFile = new FileInfo(path);

                pck.SaveAs(xlFile);
                pck.Dispose();
            }

            DownloadOrder(Filename);
            ViewBag.CustomerName = companyName;
            return View("ExportPriceList");
        }

        public ActionResult PriceList(string companyName)
        {
            System.Diagnostics.Debug.WriteLine(companyName);
            Customer customer = new Customer(companyName);

            ViewBag.PriceList = customer.PriceList;
            return View("PriceList");
        }

       





    }
}