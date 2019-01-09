using Dendi.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dendi.Controllers
{
    public class ProductsController : ApiController
    {        
        public SqlConnection connection = new SqlConnection(@"Server=den1.mssql8.gear.host;Database=dendi;Uid=dendi;Pwd=Be1W!dsz0_f6;");
        
        [HttpGet]
        public IHttpActionResult AllProducts()
        {
            // Unnecessary
            IList<Product> ProducstsToReturn = new List<Product>();
            connection.Open();
            SqlCommand GetAllProductsCommand = new SqlCommand("SELECT *FROM ProductsTable", connection);
            SqlDataReader reader = GetAllProductsCommand.ExecuteReader();

            while (reader.Read())
            {
                ProducstsToReturn.Add(new DiscountedProduct
                {
                    ID = Convert.ToInt32(reader["ID"]),
                    Name = reader["ProductName"].ToString().TrimEnd(),
                    Price = Convert.ToDecimal(reader["ProductPrice"]),
                    Discount = Convert.ToDecimal(reader["DiscountPercentage"]),
                    LastPrice = Convert.ToDecimal(reader["ProductPrice"]) * Convert.ToDecimal(reader["DiscountPercentage"]) / 100,
                    Shop = reader["DiscountPlace"].ToString().TrimEnd(),
                    // AdditionTime = Convert.ToDateTime(reader["AdditionTime"])
                });
            }
            connection.Close();
            if (ProducstsToReturn.Count == 0)
            {
                return NotFound();
            }
            return Ok(ProducstsToReturn);
        }

        [HttpGet]
        public IHttpActionResult ProdByParts([FromUri]int rn)
        {
            // READY
            var QueryString = "SELECT ID, UserID, Name, Shop, Price, Discount, AdditionTime " +
                $"FROM ProductsTable ORDER BY ID OFFSET {rn * 5} ROWS " +
                $"FETCH NEXT 5 ROWS ONLY";
            
            IList<DiscountedProduct> discountedProducts = new List<DiscountedProduct>();
            connection.Open();
            using(var command = new SqlCommand(QueryString, connection))
            {
                using(var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        discountedProducts.Add(new DiscountedProduct()
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            Name = reader["Name"].ToString(),
                            Shop = reader["Shop"].ToString(),
                            Price = Convert.ToInt32(reader["Price"]),
                            Discount = Convert.ToDecimal(reader["Discount"])
                            //AdditionTime = Convert.ToDateTime(reader["AdditionTime"].ToString())
                        });
                    }
                }
            }
            connection.Close();
            if (discountedProducts.Count != 0) return Ok(discountedProducts);
            return NotFound();
        }

        [HttpGet]
        public IHttpActionResult CartProducts([FromUri]int uc)
        {
            // READY ----------
            var QueryString = $"select * from User_Product up inner join ProductsTable pt on pt.ID = up.ProductID and up.UserID = {uc}";

            IList<DiscountedProduct> products = new List<DiscountedProduct>();
            connection.Open();
            using(SqlCommand command = new SqlCommand(QueryString, connection))
            {
                using(SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new DiscountedProduct()
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            Name = reader["Name"].ToString(),
                            Shop = reader["Shop"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Discount = Convert.ToDecimal(reader["Discount"]),
                            LastPrice = Convert.ToDecimal(reader["LastPrice"]),
                            // AdditionTime = Convert.ToDateTime(reader["AdditionTime"])
                        });
                    }
                }
            }
            if (products.Count == 0)
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpGet]
        public IHttpActionResult ShearedProducts([FromUri]int us)
        {
            // READY ----------
            var QueryString = $"select * from User_SharedProduct up inner join ProductsTable pt on pt.ID = up.ProductID and up.UserID = {us}";
            IList<DiscountedProduct> products = new List<DiscountedProduct>();
            connection.Open();
            using (SqlCommand command = new SqlCommand(QueryString, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new DiscountedProduct()
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            Name = reader["Name"].ToString(),
                            Shop = reader["Shop"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Discount = Convert.ToDecimal(reader["Discount"]),
                            LastPrice = Convert.ToDecimal(reader["LastPrice"]),
                            // AdditionTime = Convert.ToDateTime(reader["AdditionTime"])
                        });
                    }
                }
            }
            if (products.Count == 0)
            {
                return NotFound();
            }
            return Ok(products);            
        }

        [HttpPost]
        public IHttpActionResult AddProduct([FromBody]DiscountedProduct p, [FromUri]int u)
        {
            // READY
            var lastP = p.Price - (p.Price * p.Discount / 100);
            if (!ModelState.IsValid)
            {
                return BadRequest("\"Invalid product entry\" -Try again.");
            }
            var QueryString = "INSERT INTO ProductsTable (UserID, Name, Shop, Price, Discount, AdditionTime, LastPrice) " +
                "VALUES('" + u + "', '" + p.Name + "', '" + p.Shop + "', '" + p.Price + "', '" + p.Discount + "', '" + p.AdditionTime.ToShortDateString() + "', '" + lastP + "')";
            
            connection.Open();

            SqlCommand PostProductCmd = new SqlCommand(QueryString, connection);
            if (PostProductCmd.ExecuteNonQuery() == 0)
            {
                connection.Close();
                return BadRequest("Couldn't insert product. Try again later.");
            }

            connection.Close();
            return Ok();            
        }

        [HttpPost]
        public IHttpActionResult AddToCart([FromUri]int u,[FromUri]int p)
        {
            // READY ---------->
            var QueryString = "Insert into User_Product (UserID, ProductID) values('" + u + "', '" + p + "')";

            connection.Open();
            using(SqlCommand command = new SqlCommand(QueryString, connection))
            {
                switch (command.ExecuteNonQuery())
                {
                    case 1:
                        connection.Close();
                        return Ok();
                    default:
                        connection.Close();
                        return InternalServerError();
                }
            }
        }

        [HttpPost]
        public IHttpActionResult ShareProduct([FromUri]int u, [FromUri]int p)
        {
            // READY ---------->
            var QueryString = "Insert into User_SharedProduct (UserID, ProductID) values ('" + u + "', '" + p + "')";

            connection.Open();
            using(SqlCommand command = new SqlCommand(QueryString, connection))
            {
                switch (command.ExecuteNonQuery())
                {
                    case 1:
                        connection.Close();
                        return Ok();
                    default:
                        connection.Close();
                        return InternalServerError();
                }
            }
        }
    }
}
