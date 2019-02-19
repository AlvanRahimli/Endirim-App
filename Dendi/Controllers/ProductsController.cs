using Dendi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dendi.Controllers
{    
    /// <summary>
    /// Controller for Products actions. Includes Loading Products in parts, Adding product to Cart, Adding products to Shared List, Adding product itself, 
    /// Deleting product and Updating product.
    /// </summary>
    public class ProductsController : ApiController
    {     
        private SqlConnection connection = new SqlConnection(@"Server=den1.mssql8.gear.host;Database=dendi;Uid=dendi;Pwd=Be1W!dsz0_f6;");
        
        /// <summary>
        /// Loads products in parts of 10 to increase performance. 
        /// </summary>
        /// <param name="rn">The number of request. It changes range of IDs of Products.</param>
        /// <returns>List of Products</returns>
        [HttpGet]
        public IHttpActionResult ProdByParts([FromUri]int rn)
        {
            // READY
            var QueryString = "SELECT ProductsTable.ID, UserID, Name, Shop, Price, Discount, AdditionTime, Username " +
                "FROM ProductsTable, UsersTable Where ProductsTable.UserID = UsersTable.ID " +
                $"ORDER BY ProductsTable.ID OFFSET {rn} ROWS FETCH NEXT 10 ROWS ONLY";
            
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
                            Discount = Convert.ToDecimal(reader["Discount"]),
                            AdditionTime = reader["AdditionTime"].ToString()
                        });
                    }
                }
            }
            connection.Close();
            if (discountedProducts.Count != 0) return Ok(discountedProducts);
            return NotFound();
        }

        /// <summary>
        /// Loads given User's products of Cart
        /// </summary>
        /// <param name="uc">Spesified user's ID</param>
        /// <returns>List of products</returns>
        [HttpGet]
        public IHttpActionResult CartProducts([FromUri]int uc)
        {
            var QueryString = $"select * from User_Product up inner join ProductsTable pt on pt.ID = up.ProductID and up.UserID = @UId";

            IList<DiscountedProduct> products = new List<DiscountedProduct>();
            connection.Open();
            using(SqlCommand command = new SqlCommand(QueryString, connection))
            {
                command.Parameters.Add("@UId", SqlDbType.Int).Value = uc;
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
                            AdditionTime = reader["AdditionTime"].ToString()
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

        /// <summary>
        /// Loads given User's products of Sheard
        /// </summary>
        /// <param name="us">Specified user's ID</param>
        /// <returns>List of products</returns>
        [HttpGet]
        public IHttpActionResult SharedProducts([FromUri]int us)
        {
            var QueryString = "select * from User_SharedProduct up inner join ProductsTable pt on pt.ID = up.ProductID and up.UserID = @Us";
            IList<DiscountedProduct> products = new List<DiscountedProduct>();
            connection.Open();
            using (SqlCommand command = new SqlCommand(QueryString, connection))
            {
                command.Parameters.Add("@Us", SqlDbType.Int).Value = us;
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
                            AdditionTime = reader["AdditionTime"].ToString()
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

        /// <summary>
        /// Method to add products
        /// </summary>
        /// <param name="p">[FromBody] DiscountedProduct</param>
        /// <param name="u">[FromUri] User's ID</param>
        /// <returns>ActionResult of Ok()</returns>
        [HttpPost]
        public IHttpActionResult AddProduct([FromBody]DiscountedProduct p, [FromUri]int u)
        {
            var lastP = p.Price - (p.Price * p.Discount / 100);
            if (!ModelState.IsValid)
            {
                return BadRequest("\"Invalid product entry\" -Try again.");
            }
            var QueryString = "INSERT INTO ProductsTable (UserID, Name, Shop, Price, Discount, AdditionTime, LastPrice) " +
                "VALUES(@UId, @Name, @Shop, @Price, @Discount, @AdditionTime, @LastPrice)";
            
            connection.Open();

            using (SqlCommand command = new SqlCommand(QueryString, connection))
            {
                command.Parameters.Add("@UId", SqlDbType.Int).Value = u;
                command.Parameters.Add("@Name", SqlDbType.Text).Value = p.Name;
                command.Parameters.Add("@Shop", SqlDbType.Text).Value = p.Shop;
                command.Parameters.Add("@Price", SqlDbType.Decimal).Value = p.Price;
                command.Parameters.Add("@Discount", SqlDbType.Decimal).Value = p.Discount;
                command.Parameters.Add("@AdditionTime", SqlDbType.VarChar).Value = p.AdditionTime;
                command.Parameters.Add("@LastPrice", SqlDbType.Decimal).Value = lastP;
                if (command.ExecuteNonQuery() == 0)
                {
                    connection.Close();
                    return BadRequest("Couldn't insert product. Try again later.");
                }
            }

            connection.Close();
            return Ok();            
        }

        /// <summary>
        /// Method to add product to Cart
        /// </summary>
        /// <param name="u">User's ID</param>
        /// <param name="p">ID of product</param>
        /// <returns>'Ok or 'Internal Server Error'</returns>
        [HttpPost]
        public IHttpActionResult AddToCart([FromUri]int u,[FromUri]int p)
        {
            var QueryString = "Insert into User_Product (UserID, ProductID) values(@U, @P)";

            connection.Open();
            using(SqlCommand command = new SqlCommand(QueryString, connection))
            {
                command.Parameters.Add("@U", SqlDbType.Int).Value = u;
                command.Parameters.Add("@P", SqlDbType.Int).Value = p;
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

        /// <summary>
        /// Method to Share Product. Currently not avaliable.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ShareProduct([FromUri]int u, [FromUri]int p)
        {
            var QueryString = "Insert into User_SharedProduct (UserID, ProductID) values (@U, @P)";

            connection.Open();
            using(SqlCommand command = new SqlCommand(QueryString, connection))
            {
                command.Parameters.Add("@U", SqlDbType.Int).Value = u;
                command.Parameters.Add("@P", SqlDbType.Int).Value = p;
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

        /// <summary>
        /// Method to delete product
        /// </summary>
        /// <param name="Id">ID of Product</param>
        /// <returns>Ok or Interlan Server Error</returns>
        [HttpDelete]
        public IHttpActionResult DeleteProduct([FromUri]int Id)
        {
            var QueryString = "Delete From User_Product Where ProductID=@PID; " +
                "Delete from User_SharedProduct where ProductID=@PID;" +
                " Delete from ProductsTable where ID=@PID;";

            connection.Open();
            using(SqlCommand command = new SqlCommand(QueryString, connection))
            {
                command.Parameters.Add("@PID", SqlDbType.Int).Value = Id;
                switch (command.ExecuteNonQuery())
                {
                    case 1:
                        connection.Close();
                        return Ok($"Product with ID of {Id} deleted succesfully.");
                    default:
                        connection.Close();
                        return InternalServerError();
                }
            }
        }

        /// <summary>
        /// Method to Update Product data
        /// </summary>
        /// <param name="action">Type of Action. Can be: 'name', 'shop', 'price', 'discount'</param>
        /// <param name="newParameter">New parameter for given properity</param>
        /// <param name="UpId">Product's ID</param>
        /// <returns>Ok or Interlan Server Error</returns>
        [HttpPost]
        public IHttpActionResult UpdateProduct([FromUri]string action, [FromUri]string newParameter,[FromUri]int UpId)
        {
            if (action.Equals("name"))
            {
                var QueryString = "Update ProductsTable set Name = @NewName where ID = @Id";

                connection.Open();
                using(SqlCommand command = new SqlCommand(QueryString, connection))
                {
                    command.Parameters.Add("@NewName", SqlDbType.Text).Value = newParameter;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = UpId;

                    switch (command.ExecuteNonQuery())
                    {
                        case 1:
                            connection.Close();
                            return Ok($"Product Name changed succesfully to {newParameter}");
                        default:
                            connection.Close();
                            return InternalServerError();
                    }
                }
            }
            else if (action.Equals("shop"))
            {
                var QueryString = "Update ProductsTable set Shop = @NewShop where ID = @Id";

                connection.Open();
                using (SqlCommand command = new SqlCommand(QueryString, connection))
                {
                    command.Parameters.Add("@NewShop", SqlDbType.NChar).Value = newParameter;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = UpId;

                    switch (command.ExecuteNonQuery())
                    {
                        case 1:
                            connection.Close();
                            return Ok($"Product Shop changed succesfully to {newParameter}");
                        default:
                            connection.Close();
                            return InternalServerError();
                    }
                }
            }
            else if (action.Equals("price"))
            {
                var QueryString = "Update ProductsTable set Price = @NewPrice where ID = @Id";
                var Message = "Something went wrong!";

                connection.Open();
                using (SqlCommand command = new SqlCommand(QueryString, connection))
                {
                    command.Parameters.Add("@NewPrice", SqlDbType.Decimal).Value = Convert.ToDecimal(newParameter);
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = UpId;

                    switch (command.ExecuteNonQuery())
                    {
                        case 1:
                            switch (UpdateLastPrice(UpId))
                            {
                                case true:
                                    Message = "Last Price changed succesfully.";
                                    break;
                                default:
                                    Message = "Couldn't change last price. Try Again!";
                                    break;
                            }
                            connection.Close();
                            return Ok($"Product Price changed succesfully to {newParameter}. " + Message);
                        default:
                            connection.Close();
                            return InternalServerError();
                    }
                }
            }
            else if (action.Equals("discount"))
            {
                var QueryString = "Update ProductsTable set Discount = @NewDiscount where ID = @Id";
                var Message = "Something went wrong!";
                connection.Open();
                using (SqlCommand command = new SqlCommand(QueryString, connection))
                {
                    command.Parameters.Add("@NewDiscount", SqlDbType.Decimal).Value = Convert.ToDecimal(newParameter);
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = UpId;

                    switch (command.ExecuteNonQuery())
                    {
                        case 1:
                            switch (UpdateLastPrice(UpId))
                            {
                                case true:
                                    Message = "Last Price changed succesfully.";
                                    break;
                                default:
                                    Message = "Couldn't change last price. Try Again!";
                                    break;
                            }
                            connection.Close();
                            return Ok($"Product Discount changed succesfully to {newParameter}. " + Message);
                        default:
                            connection.Close();
                            return InternalServerError();
                    }
                }
            }
            else
            {
                return BadRequest("Invalid action entry. Action must be 'name', 'shop', 'price' or 'discount'.");
            }
        }

        [NonAction]
        private bool UpdateLastPrice(int Id)
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            var QueryString2 = "Select Price, Discount from ProductsTable Where ID = @Id";
            decimal Price = 0, Discount = 0, LastPrice = 0;

            using (SqlCommand commannd2 = new SqlCommand(QueryString2, connection)) 
            {
                commannd2.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                using (SqlDataReader reader = commannd2.ExecuteReader()) 
                {
                    while (reader.Read())
                    {
                        Price = Convert.ToDecimal(reader["Price"]);
                        Discount = Convert.ToDecimal(reader["Discount"]);
                    }
                }
            }

            var QueryString = "Update ProductsTable Set LastPrice=@LPrice Where ID=@Id";
            LastPrice = Price * Discount / 100;

            using (SqlCommand command = new SqlCommand(QueryString, connection)) 
            {
                command.Parameters.Add("@LPrice", SqlDbType.Decimal).Value = LastPrice;
                command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                switch (command.ExecuteNonQuery())
                {
                    case 1:
                        connection.Close();
                        return true;
                    default:
                        connection.Close();
                        return false;
                }
            }
        }
        
        /*[NonAction]
        //public DateTime GetDate(string raw)
        //{
        //    string[] monthes = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        //    var arr = raw.Split(' ');

        //    int day = Convert.ToInt32(arr[0]);
        //    int month = Array.IndexOf(monthes, arr[1]) + 1;
        //    int year = Convert.ToInt32(arr[2]);

        //    return new DateTime(year, month, day);
        //}

        //[NonAction]
        //public string GetString(DateTime date)
        //{
        //    string[] monthes = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        //    int day = date.Day;
        //    string mon = monthes[date.Month - 1];
        //    int year = date.Year;

        //    string res = day.ToString() + " " + mon + " " + year.ToString();
        //    return res;
        //}*/
    }
}
