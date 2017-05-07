using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SampleProject.Models;
using SampleProject_BusinessLogic;
using SampleProject_BusinessEntities;
using System.Web.Security;
using PagedList;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;

namespace SampleProject.Controllers
{
    public class AdministratorController : AdministratorSessionCheckingController
    {
        CommonBusinessLogic objCommonBL = new CommonBusinessLogic();
        // GET: Administrator
        [HttpGet]
        public ActionResult CreateUser()
        {
           // CommonBusinessLogic objCommonBL = new CommonBusinessLogic();
            ViewBag.States = new SelectList(objCommonBL.GetStates(), "StateId", "Statename");
            ViewBag.Roles = new SelectList(objCommonBL.GetRoles(), "RoleId", "Rolename");
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(Users objUsers)
        {
            AdministratorBusinessLogic objAdministratorBL = new AdministratorBusinessLogic();
            ViewBag.States = new SelectList(objCommonBL.GetStates(), "StateId", "Statename");
            ViewBag.Roles = new SelectList(objCommonBL.GetRoles(), "RoleId", "Rolename");
            //ModelState.Remove("Status");
            if (ModelState.IsValid)
            {
                string password = Membership.GeneratePassword(8, 1);
                objUsers.Password = Encrypt(password);

                int? result = objAdministratorBL.InsertData(objUsers);
                ViewBag.Message = "User details Created successfully";
                //mail content

                StringBuilder sb = new StringBuilder();
                MailMessage mail = new MailMessage();
                mail.To.Add(objUsers.EmailID);
                mail.From = new MailAddress("bhavya.vootla@gmail.com");
                mail.Subject = "New user creation";
                sb.Append("First name: " + objUsers.Name);
                sb.Append(Environment.NewLine);
                sb.Append("Email: " + objUsers.EmailID);
                sb.Append(Environment.NewLine); 
                sb.Append("Password: " + objUsers.Password);
                sb.Append(Environment.NewLine);
                mail.IsBodyHtml = true;
                mail.Body = sb.ToString();
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                ("bhavya.vootla@gmail.com", "bhavyasuper7");// Enter senders User name and password  
                smtp.EnableSsl = true;
                smtp.Send(mail);

                //end mail content
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Error in saving data");
                return View();
            }
        }

        public JsonResult IsEmailAvailable(string EmailID)
        {
            AdministratorBusinessLogic objAdministratorBL = new AdministratorBusinessLogic();
            Users objUsers = new Users();
            int res = objAdministratorBL.ValidateEmailID(EmailID);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsUser_IDAvailable(string User_ID)
        {
            AdministratorBusinessLogic objAdministratorBL = new AdministratorBusinessLogic();
            Users objUsers = new Users();
            int res = objAdministratorBL.ValidateEmailID(User_ID);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowAllUserDetails(string SortBy, string Search_Data, string Filter_Value, int? Page_No)
        {
            if (Session["Administrator"] != null)
            {
                Users objUsers = new Users();
                AdministratorBusinessLogic objAdministratorBL = new AdministratorBusinessLogic();
                objUsers.ShowallUsers = objAdministratorBL.Selectalldata();
                ViewBag.CurrentSortOrder = SortBy;
                ViewBag.SortName = string.IsNullOrEmpty(SortBy) ? "Name desc" : "";
                ViewBag.SortGender = SortBy == "Gender" ? "Gender desc" : "Gender";
                if (Search_Data != null)
                {
                    Page_No = 1;
                }
                else
                {
                    Search_Data = Filter_Value;
                }

                ViewBag.FilterValue = Search_Data;

                var users = from usr in objUsers.ShowallUsers select usr;
                if (!string.IsNullOrEmpty(Search_Data))
                {
                    users = users.Where(usr => usr.Name.ToUpper().Contains(Search_Data.ToUpper())
                                          || usr.EmailID.ToUpper().Contains(Search_Data.ToUpper())
                                          || usr.Mobile.ToUpper().Contains(Search_Data.ToUpper()));
                }
                switch (SortBy)
                {
                    case "Name desc":
                        users = users.OrderByDescending(x => x.Name);
                        break;
                    case "Gender desc":
                        users = users.OrderByDescending(x => x.Gender);
                        break;
                    case "Gender":
                        users = users.OrderByDescending(x => x.Gender);
                        break;
                    default:
                        users = users.OrderBy(x => x.Name);
                        break;
                }
                int Size_Of_Page = 4;
                int No_Of_Page = (Page_No ?? 1);
                return View(users.ToPagedList(No_Of_Page, Size_Of_Page));
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ViewProfile(string UserID)
        {
            CommonBusinessLogic objCommonBL = new CommonBusinessLogic();
            Users objUsers = new Users();
            if (Session["Administrator"] != null)
            {
                UserID = Session["Administrator"].ToString();
            }
            return View("ViewProfile", objCommonBL.GetUserDetailsByUserID(UserID));
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            AdministratorBusinessLogic objAdministratorBL = new AdministratorBusinessLogic();
            int? result = objAdministratorBL.DeleteData(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private string Encrypt(string PasswordForEncrypt)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(PasswordForEncrypt);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    PasswordForEncrypt = Convert.ToBase64String(ms.ToArray());
                }
            }
            return PasswordForEncrypt;
        }

    }
}