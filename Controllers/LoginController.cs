using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;
using System.Web.Security;
using PagedList;
using PagedList.Mvc;


using Task.Models;
using System.Data.Entity;

namespace Task.Controllers
{
    public class LoginController : Controller
    { 
       
      
        public ActionResult Login()
        {
            return View();
        }

       private mytaskEntities1 db = new mytaskEntities1();
        string message = "";
        
        public ActionResult logout()
        {
            Session.Clear();

            return RedirectToAction("login", "login");
        }
       
        [HttpPost]
        public ActionResult Login(User user)
        {
           
            try
            {
               

                var data = db.Users.Where(x => x.Emailaddress == user.Emailaddress && x.Password == user.Password && x.isactive == true && x.Role == "user" && x.isblock == false).FirstOrDefault();
                var data2 = db.Users.Where(x => x.Emailaddress == user.Emailaddress && x.Password == user.Password && x.isactive == true && x.Role == "Admin"&& x.isblock==false).FirstOrDefault();
                bool captcha = this.IsCaptchaValid("success");
                if (captcha && data != null)
                {

                    Session["id"] = user.Id;
                    Session["Emailaddress"] = user.Emailaddress;
                    Session["Name"] = user.Name;
                    Session["Role"] = "user";
                    var email = Session["Emailaddress"].ToString();

                    TempData["email"] = email;
                    return RedirectToAction("UserLogin");
                }
                else if (captcha && data2 != null)

                {
                    Session["id"] = user.Id;
                    Session["Emailaddress"] = user.Emailaddress;
                    Session["Name"] = user.Name;
                    Session["Role"] = "Admin";
                    return RedirectToAction("admin");
                }
                else if (captcha == false)
                {
                    ViewBag.message = "invaild code";
                }

                else
                {
                    ViewBag.message = "User Name or Password is Incorrect.";
                }
                return View();
                
            }
            catch
            {
                return View();
            }
        }
      

      public ActionResult Userlogin(int? i, string searchString, string sortby, string searchby)
        {
            ViewBag.SortName = string.IsNullOrEmpty(sortby) ? "Name desc" : "";
            ViewBag.Sortemail = string.IsNullOrEmpty(sortby) ? "email desc" : "";
            ViewBag.Sortrole = string.IsNullOrEmpty(sortby) ? "role desc" : "";
            string uemail = Session["EmailAddress"].ToString();
            ViewBag.email = uemail;

            var email = db.Users.AsQueryable();

           
            if (!string.IsNullOrEmpty(searchString))
            {
                email = email.Where(c => c.Emailaddress == searchString || c.Name == searchString || c.Role == searchString || c.Password == searchString);

            }

            switch (sortby)
            {
                case "Name desc":
                    email = email.OrderByDescending(x => x.Name);
                    break;
                case "email desc":
                    email = email.OrderByDescending(x => x.Emailaddress);
                    break;
                case "Emailaddress":
                    email = email.OrderBy(x => x.Emailaddress);
                    break;
                case "role desc":
                    email = email.OrderByDescending(x => x.Role);
                    break;
                default:
                    email = email.OrderBy(x => x.Name);
                    break;


            }
            Response.Write("Reset link is sent");
            return View(email.Where(x => x.Role == "user" && x.isblock == true).ToList().ToPagedList(i ?? 1, 10));
        }

        public ActionResult Forgotpassword(string email)

        {

            if (email == null)

            {
                return View();

            }

            else if (email != null)
            {
                var role = Session["Role"].ToString();
                User user = db.Users.Where(x => x.Emailaddress == email).FirstOrDefault();
                ViewBag.Id = new SelectList(db.Users, "Id", "Email", user.Emailaddress);

                string resetcode = Guid.NewGuid().ToString();

                sendemail(user.Emailaddress, resetcode, "resetpassword");

                user.Resetpasswordcode = resetcode;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();


                ViewBag.Message = message;
           
                if (role == "user"|| role == "User")
                {

                return RedirectToAction("Userlogin");

                }
                else
                {
                    return RedirectToAction("admin");
                }
                
            }

            return View();


           
 
        }
        [HttpPost]
        public ActionResult Forgotpassword(string emailID,int? id)
        {
         
            var account= db.Users.Where(x => x.Emailaddress == emailID).FirstOrDefault();
            if (account != null)
            {
                string resetcode = Guid.NewGuid().ToString();

                sendemail(emailID, resetcode, "resetpassword");

                account.Resetpasswordcode = resetcode;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
               
                message = "Please check your email to reset your password";
            }
            else
            {
                message = "account not found";
            }
            ViewBag.Message = message;
            return View();
        }
        [NonAction]
        public void sendemail(string email, string code, string emailfor = "verifyaccount")
        {
            var url = "/login/" + emailfor+"/" + code;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, url);

            var fromemail = new MailAddress("jabzahmy@gmail.com");
            var toemail = new MailAddress(email);
            var fromemailpassword = "JabarSaleem@123";

            string subject = ""; string body = "";

            if (emailfor == "resetpassword")
            {
                subject = "Reset your password";
                body = "Please click on the following link to reset your password"
                    + "<br/><br/><a href=" + link + ">Reset Password</a>";
            }
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromemail.Address, fromemailpassword)

            };

          using(  var message = new MailMessage(fromemail, toemail)
            {
                Subject = subject,
                 Body=body,
                 IsBodyHtml=true

           
         })
                smtp.Send(message);
        }

        
        public ActionResult resetpassword(string id)
        {
            var user = db.Users.Where(x => x.Resetpasswordcode == id).FirstOrDefault();
            if (user != null)
            {
                Resetpassword mode = new Resetpassword();
                mode.resendcode = id;
                return View(mode);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [HttpPost]

        public ActionResult resetpassword(Resetpassword obj)
        {
            var message = "";

           
            

                var user = db.Users.Where(x => x.Resetpasswordcode == obj.resendcode).FirstOrDefault();
                if (user != null)
                {
                    user.Password = obj.newpassoword;
                    user.confirmpassword = obj.newpassoword;
                    user.Resetpasswordcode = "";
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                    message = "New Password Is Saved Successfully";
                }
                
                else
                {

                    message = "Something Went Wrong";
                }
                    

            
            ViewBag.Message = message;
            return View(obj);
                
        }

        
        public ActionResult Admin(int?i, string searchString, string sortby,string searchby)
        {
            ViewBag.SortName = string.IsNullOrEmpty(sortby) ? "Name desc" : "";
            ViewBag.Sortemail = string.IsNullOrEmpty(sortby) ? "email desc" : "";
            ViewBag.Sortrole = string.IsNullOrEmpty(sortby) ? "role desc" : "";
            ViewBag.role = Session["Role"];

            var email = db.Users.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchString))
            {
                email = email.Where(c => c.Emailaddress == searchString||c.Name==searchString||c.Role==searchString||c.Password==searchString);
               
            }
            
            switch(sortby)
            {
                case "Name desc":
                    email = email.OrderByDescending(x => x.Name);
                    break;
                case "email desc":
                    email = email.OrderByDescending(x => x.Emailaddress);
                    break;
                case "Emailaddress":
                    email = email.OrderBy(x => x.Emailaddress);
                    break;
                case "role desc":
                    email = email.OrderByDescending(x => x.Role);
                    break;
                default:
                    email = email.OrderBy(x => x.Name);
                    break;


            }
          
            return View(email.Where(x => x.Role == "user" && x.isactive == true).ToList().ToPagedList(i ?? 1, 10));
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);

           
            //List<User>findrole = user.ToList();
            
            //SelectList list = new SelectList(findrole, "id", "Role");
          
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id = new SelectList(db.Users, "Id", "Name", user.Id);
            ViewBag.Userole = new SelectList(user.Role.ToList(), "ID", "Role");
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,password,confirmpassword,EmailAddress,,role,isactive,isblock")] User user)
        {
            if (true)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("admin");
            }
            ViewBag.Id = new SelectList(db.Users, "Id", "Name", user.Id);
            return View(user);
        }

        public ActionResult delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

           
            ViewBag.Id = new SelectList(db.Users, "Id", "Name", user.Id);
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        
        [ValidateAntiForgeryToken]
        public ActionResult delete(int id)

        {
            User user = db.Users.Find(id);
            user.isactive = false;

            db.SaveChanges();
            return RedirectToAction("admin");
        }
        public ActionResult block(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id = new SelectList(db.Users, "Id", "Name", user.Id);
            return View(user);
        }
        [HttpPost, ActionName("block")]
        [ValidateAntiForgeryToken]
        public ActionResult block(int id)

        {
            User user = db.Users.Find(id);

             if (user.isblock == false)

                {
                    user.isblock = true;
                    db.SaveChanges();
                    return RedirectToAction("admin");
                }
                else
            
            message = "User is already Blocked";
            ViewBag.Message = message;
            ViewBag.Id = new SelectList(db.Users, "Id", "Name", user.Id);
           
            return View (user);


        }
           

           
        public ActionResult unblock(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id = new SelectList(db.Users, "Id", "Name", user.Id);
            return View(user);
        }
        [HttpPost, ActionName("unblock")]
        [ValidateAntiForgeryToken]
        public ActionResult Unblock(int id)

        {
            User user = db.Users.Find(id);
            if (user.isblock == true)

            {
                user.isblock = false;
                db.SaveChanges();
                return RedirectToAction("admin");
            }
            else

                message = "User is already Unblocked";
            ViewBag.Message = message;
            ViewBag.Id = new SelectList(db.Users, "Id", "Name", user.Id);

            return View(user);
        }

        public ActionResult Add()
        {
            return View();
        }
        

        [HttpPost]
        public ActionResult Add(User users)
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
                    return RedirectToAction("admin");
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
