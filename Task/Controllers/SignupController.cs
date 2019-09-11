using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Task.Models;

namespace Task.Controllers
{
    public class SignupController : Controller
    {
        public ActionResult Signup()
        {
            return View();
        }
        mytaskEntities1 db = new mytaskEntities1();
        string message = "";

        [HttpPost]
        public ActionResult Signup(User users)
        {
            try

            {
                var user = db.Users.Where(x => x.Emailaddress == users.Emailaddress).FirstOrDefault();
                if (user != null)
                {
                    message = "User already exists";
                    ViewBag.Message = message;
                    return View();
                }
                else
                {
                    users.isactive = true;
                    users.isblock = false;

                    db.Users.Add(users);

                    db.SaveChanges();

                    message = users.Name + " is Registed successfuly";
                    ViewBag.Message = message;
                    return View();
                }
            }
            catch
            {
                message = "Something went wrong";
                ViewBag.Message = message;
                return View();
            }

        }

       
    }

}