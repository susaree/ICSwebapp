
using OfficeOpenXml;
using System;
using System.Globalization;

namespace WebApplication3.Models
{

    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ListPrice { get; set; }
        public string Quantity { get; set; }
        public string Category { get; set; }

        public string TaxCode { get; set; }

        public bool IsSelected { get; set; }

        public Product GetProductWithoutName(string company, string id, string quant)
        {
            CultureInfo culture = new CultureInfo("en-GB");
            var file = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"\Content\" + "pricelistandstock.xlsx");
            using (var pck = new ExcelPackage(file))
            {
                var sheet = pck.Workbook.Worksheets[1];
                var endRow = sheet.Dimension.End;
                for (int row = 6; row <= endRow.Row; row++)
                {
                    var cellValName = sheet.Cells[row, 4].Text;
                    if (cellValName.Equals(company, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var start = row + 2;
                        for (int row2 = start; row2 <= endRow.Row; row2++)
                        {
                            var cellValId = sheet.Cells[row2, 1].Text;
                            if (cellValId.Equals("ZZZ") || cellValId.Equals("Ref"))
                            {
                                break;
                            }

                            if (cellValId.Equals(id))
                            {

                                var name = sheet.Cells[row2, 4].Text;
                                var price = sheet.Cells[row2, 8].Text;
                                var category = "";
                                var taxCode = "";
                                for (int row4 = 10; row4 <= endRow.Row; row4++)
                                {
                                    var cellValue = sheet.Cells[row4, 16].Text;
                                    var catVal = sheet.Cells[row4, 21].Text;
                                    var taxVal = sheet.Cells[row4, 25].Text;
                                    if (cellValue.Equals(""))
                                    {
                                        break;
                                    }
                                    if (cellValue.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        category = catVal;
                                        taxCode = taxVal;
                                    }
                                }

                                Product p = new Product(id, name, price);
                                p.Category = category;
                                p.TaxCode = taxCode;
                                p.Quantity = quant;

                                return p;


                            }
                        }
                    }
                 
                }



            }
            return null;
        }

        public Product(string id, string name, string listPrice)
        {
            Id = id;
            Name = name;
            ListPrice = listPrice;
            IsSelected = false;

        }





    }
}