using System.Collections.Generic;
using System.Web.Mvc;
using WebApplication3.Models;
using System.IO;
using WebApplication3.Extentions;
using OfficeOpenXml;
using PagedList;
using System;
using PagedList.Mvc;
using System.Globalization;
using System.Linq;

namespace WebApplication3.Controllers
{

    public class ProductController : Controller
    {




        public ActionResult ProductListFull(string searchString, int? page, string sortBy)
        {
            ViewBag.SortCategoryPara = string.IsNullOrEmpty(sortBy) ? "Category desc" : "";
            ViewBag.SortNamePara = sortBy == "Name" ? "Name desc" : "Name";
            CultureInfo culture = new CultureInfo("en-GB");

            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "pricelistandstock.xlsx");
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];
                bool flag = true;
                var endRow = sheet.Dimension.End;


                List<Product> productList = new List<Product>();
                var searchedList = new List<Product>();
                var searchedListCat = new List<Product>();

                string[] categories = new string[]
                {
                     "Soft Drinks","Oils","Flours","Rice","Tinned Fruit /Veg","Fresh Fruit /Veg","Spices","Sauces","Condiments","Frozen","Chilled","Desserts","Milkshake","Indian","Chinese","Pizza","Packaging","Cleaning Products", "General"
            };



                for (int row = 10; row <= endRow.Row; row++)

                {
                    string id = sheet.Cells[row, 12].Text;
                    string name = sheet.Cells[row, 16].Text;
                    string category = sheet.Cells[row, 21].Text;
                    string price = sheet.Cells[row, 23].Text;

                    Product p = new Product(id, name, price);
                    p.Category = category;

                    if (p.Id.Equals(" ") || p.Id.Equals("ZZZ"))
                    {
                        break;

                    }

                    if(p.Name.Equals("ZZ") || p.Name.ToLower().Contains("do not use") || p.Name.ToLower().Contains("dont use") || p.Name.ToLower().Contains("frankies") || p.Name.ToLower().Contains("chickanos") || p.Name.ToLower().Contains("chicken stop") || p.Name.ToLower().Contains("no stock"))
                    {
                        continue;
                    }
                    else

                    {
                        productList.Add(p);

                    }

                }


                pck.Dispose();
                var products = productList.AsQueryable();

                switch (sortBy)
                {
                    case "Name desc":
                        products = products.OrderByDescending(x => x.Name);
                        break;
                    case "Name":
                        products = products.OrderBy(x => x.Name);
                        break;

                    case "Category desc":
                        products = products.OrderByDescending(x => x.Category);
                        break;
                    default:
                        products = products.OrderBy(x => x.Category);
                        break;
                }





                if (!String.IsNullOrEmpty(searchString))
                {
                    for (int i = 0; i <= categories.Length - 1; i++)
                    {
                        if (culture.CompareInfo.IndexOf(categories[i], searchString, CompareOptions.IgnoreCase) >= 0)
                        {

                            foreach (var product2 in productList)
                            {
                                if (Int32.Parse(product2.Category) == i + 1)
                                {
                                    searchedListCat.Add(product2);
                                    flag = false;
                                }
                            }
                        }
                    }

                    foreach (var product in productList)
                    {

                        if (culture.CompareInfo.IndexOf(product.Name, searchString, CompareOptions.IgnoreCase) >= 0)
                        {
                            searchedList.Add(product);
                        }



                    }
                    // ViewBag.ProductListFull = searchedList;
                    if (flag)
                    {
                        return View(searchedList.ToPagedList(page ?? 1, 25));
                    }
                    else
                    {
                        return View(searchedListCat.ToPagedList(page ?? 1, 25));
                    }

                }
                else
                {

                    //ViewBag.ProductListFull = productList;
                    if (products != null)
                    {
                        return View(products.ToPagedList(page ?? 1, 25));
                    }
                    else
                    {
                        return View(productList.ToPagedList(page ?? 1, 25));
                    }


                }





                // return View("ProductListFull");

            }
        }

        public ActionResult ProductList(string searchString, int? page, string sortBy)
        {

            ViewBag.SortCategoryPara = string.IsNullOrEmpty(sortBy) ? "Category desc" : "";
            ViewBag.SortNamePara = sortBy == "Name" ? "Name desc" : "Name";
            CultureInfo culture = new CultureInfo("en-GB");
            var user = System.Web.HttpContext.Current.User.Identity;
            var customer = new Customer(user.GetCompanyName().ToUpper());
            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "pricelistandstock.xlsx");
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];
                bool flag = true;
                var endRow = sheet.Dimension.End;


                List<Product> productList = customer.PriceList;
                var searchedList = new List<Product>();
                var searchedListCat = new List<Product>();
                string[] categories = new string[]
                {
                     "Soft Drinks","Oils","Flours","Rice","Tinned Fruit /Veg","Fresh Fruit /Veg","Spices","Sauces","Condiments","Frozen","Chilled","Desserts","Milkshake","Indian","Chinese","Pizza","Packaging","Cleaning Products", "General"
            };


                var products = productList.AsQueryable();

                switch (sortBy)
                {
                    case "Name desc":
                        products = products.OrderByDescending(x => x.Name);
                        break;
                    case "Name":
                        products = products.OrderBy(x => x.Name);
                        break;

                    case "Category desc":
                        products = products.OrderByDescending(x => x.Category);
                        break;
                    default:
                        products = products.OrderBy(x => x.Category);
                        break;
                }

                pck.Dispose();

                if (!String.IsNullOrEmpty(searchString))
                {
                    for (int i = 0; i <= categories.Length - 1; i++)
                    {
                        if (culture.CompareInfo.IndexOf(categories[i], searchString, CompareOptions.IgnoreCase) >= 0)
                        {

                            foreach (var product2 in productList)
                            {
                                if (Int32.Parse(product2.Category) == i + 1)
                                {
                                    searchedListCat.Add(product2);
                                    flag = false;
                                }
                            }
                        }
                    }

                    foreach (var product in productList)
                    {

                        if (culture.CompareInfo.IndexOf(product.Name, searchString, CompareOptions.IgnoreCase) >= 0)
                        {
                            searchedList.Add(product);
                        }



                    }
                    // ViewBag.ProductListFull = searchedList;
                    if (flag)
                    {
                        return View(searchedList.ToPagedList(page ?? 1, 25));
                    }
                    else
                    {
                        return View(searchedListCat.ToPagedList(page ?? 1, 25));
                    }

                }
                else
                {
                    if (products != null)
                    {
                        return View(products.ToPagedList(page ?? 1, 25));
                    }
                    else
                    {
                        return View(productList.ToPagedList(page ?? 1, 25));
                    }
                }

            }
        }



        public ActionResult ShoppingCart(int? page)
        {
            if (Session["Cart"] != null)
            { return View("ShoppingCart"); }
            else
            {
                var user = System.Web.HttpContext.Current.User.Identity;
                var customer = new Customer(user.GetCompanyName().ToString());

                var model = customer.PriceList.ToPagedList(page ?? 1, 15);
                return View("ProductList", model);
            }

        }


        [HttpGet]
        public ActionResult SaveList(string ItemList, string ItemList2, int? page)
        {
            var user = System.Web.HttpContext.Current.User.Identity;
            var companyName = user.GetCompanyName().ToString();
            var customer = new Customer(companyName);
            string[] arr = ItemList.Split(',');
            string[] arr2 = ItemList2.Split(',');

            List<Product> prodList = new List<Product>();

            for (int i = 0; i <= arr.Length-1; i++)
            {
                var paramId = arr[i].ToString();
                var paramQuant = arr2[i];

                Product product = new Product("", "", "");
                Product prod = product.GetProductWithoutName(companyName, paramId, paramQuant);
                AddToCart(prod.Id, prod.Name, prod.ListPrice, prod.Quantity, prod.TaxCode, 1);
            }

            var model = customer.PriceList.ToPagedList(page ?? 1, 15);
            return View("ProductList", model);
        }


        public ActionResult AddToCart(string id, string name, string price, string quantity, string taxCode, int? page)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (String.IsNullOrEmpty(quantity))
            {
                quantity = "1";
            }
            Product product = new Product(id, name, price);
            product.TaxCode = taxCode;


            if (Session["Cart"] == null)
            {
                List<Cart> cart = new List<Cart>
            {
                new Cart(product, Int32.Parse(quantity))
            };

                Session["Cart"] = cart;
            }
            else
            {
                List<Cart> cart = (List<Cart>)Session["Cart"];
                int check = isExistingCheck(id);
                if (check == -1)

                    cart.Add(new Cart(product, Int32.Parse(quantity)));
                else

                    cart[check].Quantity += Int32.Parse(quantity);


                Session["Cart"] = cart;
            }
            var user = System.Web.HttpContext.Current.User.Identity;
            var customer = new Customer(user.GetCompanyName().ToString());

            var model = customer.PriceList.ToPagedList(page ?? 1, 15);
            return View("ProductList", model);
        }

        public ActionResult AddQuantity(string id, int? page)
        {
            List<Cart> cart = (List<Cart>)Session["Cart"];
            int check = isExistingCheck(id);
            if (check != -1)
                cart[check].Quantity++;


            Session["Cart"] = cart;
            var user = System.Web.HttpContext.Current.User.Identity;
            var customer = new Customer(user.GetCompanyName().ToString());

            var model = customer.PriceList.ToPagedList(page ?? 1, 15);
            return View("ProductList", model);
        }

        public ActionResult MinusQuantity(string id, int? page)
        {
            List<Cart> cart = (List<Cart>)Session["Cart"];
            int check = isExistingCheck(id);
            if (check != -1 && cart[check].Quantity > 1)
                cart[check].Quantity--;


            Session["Cart"] = cart;
            var user = System.Web.HttpContext.Current.User.Identity;
            var customer = new Customer(user.GetCompanyName().ToString());

            var model = customer.PriceList.ToPagedList(page ?? 1, 15);
            return View("ProductList", model);
        }


        private int isExistingCheck(string id)
        {
            List<Cart> cart = (List<Cart>)Session["Cart"];
            for (int i = 0; i < cart.Count; i++)
            {
                string productId = cart[i].Product.Id;
                if (productId.Equals(id)) return i;
            }

            return -1;
        }

        public ActionResult Remove(string id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            int check = isExistingCheck(id);
            List<Cart> cart = (List<Cart>)Session["Cart"];
            cart.RemoveAt(check);
            var user = System.Web.HttpContext.Current.User.Identity;
            var customer = new Customer(user.GetCompanyName().ToString());

            var model = customer.PriceList.ToPagedList(page ?? 1, 15);
            return View("ProductList", model);
        }

        public ActionResult generateInvoice()
        {

            return View("index");
        }

        public ActionResult ThankYou()
        {
            var user = System.Web.HttpContext.Current.User.Identity;
            bool signedIn = System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string Filename = "Default";
            if (signedIn)
            {
                Filename = user.GetCompanyName().ToUpper() + "-" + user.GetCompanyId() + "-Order" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
            }

            //var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\New Orders" + Filename);
            string path = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + Filename).FullName;



            List<Cart> cart = (List<Cart>)Session["Cart"];
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Cart");
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Product";
                worksheet.Cells[1, 3].Value = "Quantity";
                worksheet.Cells[1, 4].Value = "Price";
                worksheet.Cells[1, 5].Value = "TaxCode";



                int row = 2;

                foreach (var c in cart)
                {
                    string rowString = row.ToString();
                    worksheet.Cells["A" + rowString].Value = c.Product.Id;
                    worksheet.Cells["B" + rowString].Value = c.Product.Name;
                    worksheet.Cells["C" + rowString].Value = c.Quantity;
                    worksheet.Cells["D" + rowString].Value = c.SubTotal;
                    worksheet.Cells["E" + rowString].Value = c.Product.TaxCode;
                    row++;
                }

                var end = worksheet.Dimension.End;

                for (int i = 2; i <= end.Column - 1; i++)
                {
                    var colVal = worksheet.Cells[i, 4].Style.Numberformat.Format = "#,##0.00";

                }
                var xlFile = new FileInfo(path);

                pck.SaveAs(xlFile);
                pck.Dispose();
            }
            cart.Clear();
            return View("ThankYou");
        }



        private void LoadData(ExcelWorksheet ws)
        {
            LoadData(ws, 1000);
        }
        private void LoadData(ExcelWorksheet ws, int rows, int cols = 1, bool isNumeric = false)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (isNumeric)
                        ws.SetValue(r + 1, c + 1, r + c);
                    else
                        ws.SetValue(r + 1, c + 1, r.ToString() + "," + c.ToString());
                }
            }
        }

    }
}