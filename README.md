# ICSwebapp
To view the site please go onto: icsnorthernltd.com
ASP .NET Framework web application

This web app was created to digitalise a catering company's order and invoice system.

Customers have access to an account which is stored on an SQL database.
Customers are shown a paged list of unique product list by iterating through an excel source document containing company names and pricelists.
Lists are sortable by name/category and items can be searched for by name/category

Orders are made by inputing values in the 'Quantity' textbox which passes list-item variable through to 
an ASP .Net controller class via Ajax POST.
Orders are saved on the server as an excel document written with the EPPlus library.

Admin users can log on and view a paged list of customers fetched from an excel document iteration loop.
All Orders can be compressed and downloaded and then imported to Sage.
