using Dendi.Common;
using Dendi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace Dendi.Controllers
{
    /// <summary>
    /// Controller for User actions. Includes Registering, Logging in, Restoring passwords, Deleting user and Updating user
    /// </summary>
    public class UsersController : ApiController
    {        
        private SqlConnection con = new SqlConnection("Server=den1.mssql8.gear.host;Database=dendi;Uid=dendi;Pwd=Be1W!dsz0_f6;");        

        /// <summary>
        /// Registers new User to Database.
        /// </summary>
        /// <param name="user">Full defined User model.</param>
        /// <returns>IHttpActionResult. Ok(), BadRequest() or InternalServerError()</returns>
        [HttpPost]
        public IHttpActionResult Register([FromBody]User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState + "Try Again");

            IList<string> MatchingUsers = new List<string>();
            var QueryString = $"Select Username from UsersTable where Username like '%@Username%'";

            user.Password = Hashing(user.Password);

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
                "values (@Un,@P,'" + user.IsAdmin + "',@E,@B,@Ph)";

            using (SqlCommand command = new SqlCommand(QueryString1, con))
            {
                command.Parameters.Add("@Un", SqlDbType.Text).Value = user.UserName;
                command.Parameters.Add("@P", SqlDbType.Text).Value = user.Password;
                command.Parameters.Add("@E", SqlDbType.Text).Value = user.Email;
                command.Parameters.Add("@B", SqlDbType.Text).Value = user.Budget.Amount + "-" + user.Budget.Currency;
                command.Parameters.Add("@Ph", SqlDbType.VarChar).Value = user.Phone;

                if (command.ExecuteNonQuery().Equals(0))
                {
                    con.Close();
                    return InternalServerError();
                }
                con.Close();
                return Ok();
            }
        }

        /// <summary>
        /// Provides Logging into user's account.
        /// </summary>
        /// <param name="u">Entered Username</param>
        /// <param name="p">Entered Password</param>
        /// <returns>User's details, UnAuthorized(), Ok() or InternalServerError()</returns>
        [HttpPost]
        public IHttpActionResult Login([FromUri]string u, [FromUri]string p)
        {
            var QueryString = "Select * from UsersTable " +
                "Where Username like @Usern and Password like @Pass";
            var loginUser = new LoginUser();
            IList<User> loginUsers = new List<User>();
            var hashedPass = Hashing(p);

            con.Open();
            using(SqlCommand command = new SqlCommand(QueryString, con))
            {
                command.Parameters.Add("@Usern", SqlDbType.Text).Value = u;
                command.Parameters.Add("@Pass", SqlDbType.Text).Value = hashedPass;

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

        /// <summary>
        /// Restores spesific user's password. But it is obsolete. UpdateUser method does it.
        /// </summary>
        /// <param name="Id">User's ID</param>
        /// <param name="np">New Password</param>
        /// <returns>Ok() or InternalServerError()</returns>
        [Obsolete("UpdateUser does it all")]
        [HttpPost]
        public IHttpActionResult RestorePassword([FromUri]int Id, [FromUri]string np)
        {
            var hashed = Hashing(np);
            var QueryString = "Update UsersTable Set Password = @NewPas Where ID='" + Id + "'";

            con.Open();
            using (SqlCommand command = new SqlCommand(QueryString, con))
            {
                command.Parameters.Add("@NewPas", SqlDbType.Text).Value = hashed;

                switch (command.ExecuteNonQuery())
                {
                    case 1:
                        con.Close();
                        return Ok("Password succesfully updated!");                        
                    default:
                        con.Close();
                        return InternalServerError();
                }
            }
        }

        /// <summary>
        /// Deletes spesific User.
        /// </summary>
        /// <param name="LeavingUser">Leaving User's fully defined model.</param>
        /// <param name="d">Nothing but difference maker.</param>
        /// <returns>Ok() or InternalServerError()</returns>
        [HttpPost]
        public IHttpActionResult DeleteUser([FromBody]LeavingUser LeavingUser, [FromUri]bool d)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model State is not valid!");

            con.Open();
            var QueryString = "delete from User_Product where UserID=@ID;" +
                " delete from UsersTable where ID = @ID;" +
                " delete from User_SharedProduct where UserID = @ID;";

            var QueryString2 = "Insert into Deleted_User_Feedbacks " +
                "(Reason, Stars) values(@Reason, @Stars)";

            var Message = String.Empty;

            using(SqlCommand command = new SqlCommand(QueryString, con))
            {
                command.Parameters.Add("@ID", SqlDbType.Int).Value = LeavingUser.ID;                
                switch (command.ExecuteNonQuery())
                {
                    case 1:
                        using(SqlCommand command2 = new SqlCommand(QueryString2, con))
                        {
                            command2.Parameters.Add("@Reason", SqlDbType.VarChar).Value = LeavingUser.Reason.ReasonHeader + 
                                ";" + LeavingUser.Reason.ReasonContent;
                            command2.Parameters.Add("@Stars", SqlDbType.Int).Value = LeavingUser.Stars;
                            switch(command2.ExecuteNonQuery())
                            {
                                case 1:
                                    Message = "User deleted succesfully! Feedback succesfully added!";
                                    break;
                                default:
                                    Message = "User deleted succesfully! An error occured while adding feedback.";
                                    break;
                            }
                        }
                        con.Close();
                        return Ok(Message);
                    default:
                        con.Close();
                        return InternalServerError();
                }
            }
        }

        /// <summary>
        /// Updates User's Username, Password or Email.
        /// </summary>
        /// <param name="NewParameter">New parameter for selected action.</param>
        /// <param name="action">Represents the parameter's name that user wants to update. Can be 'username', 'password' or 'email'</param>
        /// <param name="Id">User's ID</param>
        /// <returns>Ok() or InternalServerError()</returns>
        [HttpPost]
        public IHttpActionResult UpdateUser([FromUri]string NewParameter, [FromUri]string action, [FromUri]int Id)
        {
            if (action.Equals("username"))
            {
                var QueryString1 = "Update UsersTable set Username = @NewUsername Where ID = @UserID";
                con.Open();
                using (SqlCommand command = new SqlCommand(QueryString1, con))
                {
                    command.Parameters.Add("@NewUsername", SqlDbType.Text).Value = NewParameter;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = Id;
                    switch (command.ExecuteNonQuery())
                    {
                        case 1:
                            con.Close();
                            return Ok($"{action} succesfully changed to {NewParameter}");
                        default:
                            con.Close();
                            return InternalServerError();
                    }
                }
            }
            else if (action.Equals("password")) 
            {
                var QueryString2 = "Update UsersTable set Password = @NewPassword Where ID = @UserID";
                con.Open();
                using (SqlCommand command = new SqlCommand(QueryString2, con))
                {
                    command.Parameters.Add("@NewPassword", SqlDbType.Text).Value = Hashing(NewParameter);
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = Id;
                    switch (command.ExecuteNonQuery())
                    {
                        case 1:
                            con.Close();
                            return Ok($"{action} succesfully changed to {NewParameter}");
                        default:
                            con.Close();
                            return InternalServerError();
                    }
                }
            }
            else if (action.Equals("email"))
            {
                var QueryString3 = "Update UsersTable set Email = @NewEmail Where ID = @UserID";
                con.Open();
                using (SqlCommand command = new SqlCommand(QueryString3, con))
                {
                    command.Parameters.Add("@NewEmail", SqlDbType.Text).Value = NewParameter;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = Id;
                    switch (command.ExecuteNonQuery())
                    {
                        case 1:
                            con.Close();
                            return Ok($"{action} succesfully changed to {NewParameter}");
                        default:
                            con.Close();
                            return InternalServerError();
                    }
                }
            }
            else
            {
                return BadRequest("action name must be 'username', 'password' or 'email'. Your request did not match with none of them.");
            }
        }

        /// <summary>
        /// Hashes given string With SHA256
        /// </summary>
        /// <param name="inputString">Input string. Usually is User's password</param>
        /// <returns>Hashed string</returns>
        [NonAction]
        private static string Hashing(string inputString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte i in hash)
            {
                hashString += String.Format("{0:x2}", i);
            }
            return hashString;
        }
    }
}
