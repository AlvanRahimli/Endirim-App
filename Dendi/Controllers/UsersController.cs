using Dendi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dendi.Controllers
{
    public class UsersController : ApiController
    {        
        SqlConnection con = new SqlConnection("Server=den1.mssql8.gear.host;Database=dendi;Uid=dendi;Pwd=Be1W!dsz0_f6;");        

        [HttpPost]
        public IHttpActionResult Register([FromBody]User user)
        {
            // READY ---------->
            if (!ModelState.IsValid)
                return BadRequest(ModelState + "\n-Try Again");

            IList<string> MatchingUsers = new List<string>();
            var QueryString = $"Select Username from UsersTable where Username like '%@Username%'";

            con.Open();

            using (var command1 = new SqlCommand(QueryString, con))                
            {
                command1.Parameters.Add(new SqlParameter("@Username", user.UserName));
                using (SqlDataReader reader = command1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MatchingUsers.Add(reader["Username"].ToString());
                    }
                }
            }

            if (MatchingUsers.Count != 0)
            {
                con.Close();
                return BadRequest("Unavailable Username.\nMaybe this is already taken.");
            }

            var QueryString1 = "Insert into UsersTable (Username,Password,IsAdmin,Email,Budget,Phone) " +
                "values ('@Un','@P','" + user.IsAdmin + "','@E','@B','@Ph')";

            using (SqlCommand command = new SqlCommand(QueryString1, con))
            {
                command.Parameters.Add("Un", SqlDbType.Text);
                command.Parameters["Un"].Value = user.UserName;
                command.Parameters.Add("P", SqlDbType.Text);
                command.Parameters["P"].Value = user.Password;
                command.Parameters.Add("E", SqlDbType.Text);
                command.Parameters["E"].Value = user.Email;
                command.Parameters.Add("B", SqlDbType.Text);
                command.Parameters["B"].Value = user.Budget.Amount + "-" + user.Budget.Currency;
                command.Parameters.Add("Ph", SqlDbType.VarChar);
                command.Parameters["Ph"].Value = user.Phone;

                if (command.ExecuteNonQuery().Equals(0))
                {
                    con.Close();
                    return InternalServerError();
                }
                con.Close();
                return Ok();
            }
        }

        [HttpPost]
        public IHttpActionResult Login([FromUri]string username, [FromUri]string password)
        {
            // READY ---------->
            var QueryString = "Select * from UsersTable " +
                "Where Username like '" + username.TrimEnd() + "' and Password like '" + password.TrimEnd() + "'";
            var loginUser = new LoginUser();
            IList<User> loginUsers = new List<User>();
            con.Open();

            using(SqlCommand command = new SqlCommand(QueryString, con))
            {
                using(SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        loginUsers.Add(new User()
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            UserName = reader["Username"].ToString(),
                            Password = reader["Password"].ToString(),
                            Email = reader["Email"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Budget = new Budget()
                            {
                                Amount = Convert.ToDecimal(reader["Budget"].ToString().Split('-')[0]),
                                Currency = reader["Budget"].ToString().Split('-')[1]
                            },
                            IsAdmin = false
                        });
                    }
                }
            }

            switch (loginUsers.Count)
            {
                case 0:
                    con.Close();
                    return Unauthorized();
                case 1:
                    con.Close();
                    return Ok(loginUsers[0]);
                default:
                    con.Close();
                    return InternalServerError();
            }
        }

        //[HttpPost]
        //public IHttpActionResult RestorePassword()
        //{
        //    return null;
        //}
    }
}
