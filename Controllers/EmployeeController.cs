using SampleProject_BusinessEntities;
using SampleProject_BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleProject.Controllers
{
    public class EmployeeController : EmployeeSessionCheckingController
    {
        EmployeeBusinessLogic objEmployeeBL = new EmployeeBusinessLogic();
        //GET: Employee
        public ActionResult ViewProfile(string UserID)
        {
            CommonBusinessLogic objCommonBL = new CommonBusinessLogic();
            Users objUsers = new Users();
            if (Session["Employee"] != null)
            {
                UserID = Session["Employee"].ToString();
            }
            return View("ViewProfile", objCommonBL.GetUserDetailsByUserID(UserID));
        }

        [HttpGet]
        public ActionResult UpdateProfile(string UserID)
        {
            Users objUsers = new Users();
            CommonBusinessLogic objCommonBL = new CommonBusinessLogic();
            objUsers = objCommonBL.GetUserDetailsByUserID(UserID);
            ViewBag.States = new SelectList(objCommonBL.GetStates(), "StateId", "Statename");
            ViewBag.Cities = new SelectList(objCommonBL.GetCities(objUsers.StateID), "CityId", "Cityname");
           // ViewBag.Roles = new SelectList(objCommonBL.GetRoles(), "RoleId", "Rolename");
            return View(objUsers);
        }

        [HttpPost]
        public ActionResult UpdateProfile(string UserID, string NAME, string MOBILE, string DOB, string STATE, string CITY)
        {
            //EmployeeBusinessLogic objEmployeeBL = new EmployeeBusinessLogic();
            if (ModelState.IsValid)
            {
               if (Session["Employee"] != null)
                {
                    UserID = Session["Employee"].ToString();
                }
                int? res = objEmployeeBL.UpdateEmployeeDetails(UserID, NAME, MOBILE, DOB, STATE, CITY);
                return Json("sucMsg", JsonRequestBehavior.AllowGet);
            }
            else
            {
                ModelState.AddModelError("", "Error in saving data");
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
