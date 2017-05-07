using SampleProject_BusinessEntities;
using SampleProject_BusinessLogic;
using SampleProject_DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace SampleProject.Controllers
{
    public class SharedController : SharedSessionCheckingController
    {
        CommonBusinessLogic objCommonBL = new CommonBusinessLogic();

        public JsonResult CityList(int id)
        {
            List<Cities> cities = objCommonBL.GetCities(id);
            return Json(new SelectList(cities.ToArray(), "CityId", "Cityname"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult EditUser(string UserID)
        {
            //object initialization
            Users objUsers = new Users();

            // get user details by userid
            objUsers = objCommonBL.GetUserDetailsByUserID(UserID);

            ViewBag.States = new SelectList(objCommonBL.GetStates(), "StateId", "Statename");
            ViewBag.Cities = new SelectList(objCommonBL.GetCities(objUsers.StateID), "CityId", "Cityname");
            ViewBag.Roles = new SelectList(objCommonBL.GetRoles(), "RoleId", "Rolename");
            return View(objUsers);
        }

        public JsonResult JsonEdit(Users objUsers)
        {
            ViewBag.States = new SelectList(objCommonBL.GetStates(), "StateId", "Statename");
            ViewBag.Cities = new SelectList(objCommonBL.GetCities(objUsers.StateID), "CityId", "Cityname");
            ViewBag.Roles = new SelectList(objCommonBL.GetRoles(), "RoleId", "Rolename");

            ModelState.Remove("EmailID");
            ModelState.Remove("Gender");
            ModelState.Remove("Password");
            ModelState.Remove("Confirm_Password");
            ModelState.Remove("OldPassword");
            ModelState.Remove("Confirm_Password");
            ModelState.Remove("FileName");

            if (ModelState.IsValid) //checking model is valid or not
            {
                int? result = objCommonBL.UpdateData(objUsers);
                var fileName = Path.GetFileName(objUsers.File.FileName);
                if (objUsers.File.ContentLength > 0)
                {
                    var path = Path.Combine(Server.MapPath("~/Profile_Images"), fileName);
                    objUsers.File.SaveAs(path);
                }
               // int? imgResult = objCommonBL.InsertImage(result, objUsers.File.FileName);
                ViewBag.upload = "Success! Photo was uploaded successfully.";
                ViewBag.Message = "User details updated successfully";
                ViewData["result"] = result;
                ModelState.Clear(); //clearing model
                return Json("sucMsg", JsonRequestBehavior.AllowGet);
            }
            else
            {
                ModelState.AddModelError("", "Error in saving data");
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditUser(Users objUsers)
        {
            ViewBag.States = new SelectList(objCommonBL.GetStates(), "StateId", "Statename");
            ViewBag.Cities = new SelectList(objCommonBL.GetCities(objUsers.StateID), "CityId", "Cityname");
            ViewBag.Roles = new SelectList(objCommonBL.GetRoles(), "RoleId", "Rolename");

            ModelState.Remove("EmailID");
            ModelState.Remove("Gender");
            ModelState.Remove("Password");
            ModelState.Remove("Confirm_Password");
            ModelState.Remove("OldPassword");
            ModelState.Remove("Confirm_Password");
            ModelState.Remove("FileName"); 

            if (ModelState.IsValid) //checking model is valid or not
            {
                objUsers.FileName = objUsers.File.FileName.ToString();
                int? result = objCommonBL.UpdateData(objUsers);
                var fileName = Path.GetFileName(objUsers.File.FileName);
                if (objUsers.File.ContentLength > 0)
                {
                    var path = Path.Combine(Server.MapPath("~/Profile_Images"), fileName);
                    objUsers.File.SaveAs(path);
                }
               // int? imgResult = objCommonBL.InsertImage(result, objUsers.File.FileName);
                ViewBag.upload = "Success! Photo was uploaded successfully.";
                ViewBag.Message = "User details added successfully";
                ViewData["result"] = result;
                ModelState.Clear(); //clearing model
                if (Session["Employee"] != null || Session["Supervisor"] != null)
                {
                    return RedirectToAction("Viewprofile", "Shared");
                }

                return RedirectToAction("ShowAllUserDetails", "Administrator");
            }
            else
            {
                ModelState.AddModelError("", "Error in saving data");
                return View(objUsers);
            }
        }
        [HttpGet]
        public ActionResult ViewProfile(string UserID)
        {
            
            Users objUsers = new Users();
            if (Session["Administrator"] != null && UserID == null)
            {
                UserID = Session["Administrator"].ToString();
            }
            else
            if (Session["Supervisor"] != null && UserID == null)
            {
                UserID = Session["Supervisor"].ToString();
            }
            else if (Session["Employee"] != null && UserID == null)
            {
                UserID = Session["Employee"].ToString();
            }
            return View("ViewProfile", objCommonBL.GetUserDetailsByUserID(UserID));
         }
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(Users cp)
        {
            Users objUsers = new Users();
            // checking session values for admin/employee/supervisor
            string UserID = null;
            if (Session["Administrator"] != null)
            {
                UserID = Session["Administrator"].ToString();
            }
            else if (Session["Employee"] != null)
            {
                UserID = Session["Employee"].ToString();
            }
            else if (Session["Supervisor"] != null)
            {
                UserID = Session["Supervisor"].ToString();
            }
            //get userdetails by userid
            objUsers = objCommonBL.GetUserDetailsByUserID(UserID);
            //  compare old password

            if (objUsers.Password == cp.OldPassword)
            {
                /// data access for new password update
                int? res = objCommonBL.ChangePassword(cp.OldPassword, cp.Password, objUsers.ID);
                ViewBag.Message = "Password Changed  successfully";
            }
            else
            {
                ViewBag.Message = "Password doesnot match";
            }
            return View(cp);
        }

        // json for validate oldpassword
        public JsonResult ValidatePassword(string OldPassword)
        {
            string UserID = "";
            //session validate for employee, supervisor and administrator
            if (Session["Supervisor"] != null)
            {
                UserID = Session["Supervisor"].ToString();
            }
            else if (Session["Employee"] != null)
            {
                UserID = Session["Employee"].ToString();
            }
            else
                UserID = Session["Administrator"].ToString();
            // get primary key from users list
            Users objUsers = new Users();
            // get user details by userid
            objUsers = objCommonBL.GetUserDetailsByUserID(UserID);
            object res = new object();
            if (objUsers.Password != OldPassword.Trim().ToString())
            {
                res = 1;
            }
            else
                res = 0;

            //object res = objCommonBL.ValidatePassword(OldPassword, objUsers.ID);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

    }
}
