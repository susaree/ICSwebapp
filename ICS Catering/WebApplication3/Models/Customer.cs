using System.Collections.Generic;
using System.Linq;
using WebApplication3.Extentions;
using OfficeOpenXml;
using System.Text;
using System.IO;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using System;
using System.Drawing;
using OfficeOpenXml.Style;

namespace WebApplication3.Models
{
    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Category { get; set; }
        public List<Product> PriceList { get; set; }

        public Customer(string name)
        {
        
            Name = name;
            

            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "pricelistandstock.xlsx");
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];
                bool flag = true;
                var endRow = sheet.Dimension.End;

                int start = 0;
                int end = 0;
                string companyNameFlag = "Price List:";          
                List<Product> productList = new List<Product>();
                var category = "";
                var taxCode = "";
                for (int row = 5; row <= endRow.Row; row++)
                {
                   
                    string cellVal = sheet.Cells[row, 1].Text;
                    string cellVal2 = sheet.Cells[row, 4].Text;
                    if (cellVal2.Equals("ZZZ"))
                    {
                        break;
                    }

                    if (cellVal.Equals(companyNameFlag) && cellVal2.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        start = row + 2;

                        for (int row2 = start; row2 <= endRow.Row; row2++)
                        {
                            cellVal = sheet.Cells[row2, 1].Text;
                            cellVal2 = sheet.Cells[row2, 4].Text;

                            if (cellVal.Equals(""))
                            {
                                break;
                            }

                            if (cellVal.Equals("Ref"))
                            {

                                end = row2 - 1;
                                


                                for (int row3 = start; row3 <= end; row3++)
                                {
                                    var id = sheet.Cells[row3, 1].Text;
                                    var prodName = sheet.Cells[row3, 4].Text;
                                    var price = sheet.Cells[row3, 8].Text;
                                   
                                    for (int row4 = 10; row4 <= endRow.Row; row4++)
                                    {
                                        var cellValue = sheet.Cells[row4, 16].Text;
                                        var catVal = sheet.Cells[row4, 21].Text;
                                        var taxVal = sheet.Cells[row4, 25].Text;
                                        if (cellValue.Equals(""))
                                        {
                                            break;
                                        }
                                        if (cellValue.Equals(prodName, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            category = catVal;
                                            taxCode = taxVal;
                                        }
                                    }


                                    Product p = new Product(id, prodName, price);
                                    p.Category = category;
                                    p.TaxCode = taxCode;

                                    productList.Add(p);

                                }
                                break;
                            }
                        }

                    }
                    
                } 


                pck.Dispose();

                Category = category;

                PriceList = productList;





            }

        }
        
    }
}