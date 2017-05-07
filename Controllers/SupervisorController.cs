using SampleProject_BusinessEntities;
using SampleProject_BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleProject.Controllers
{
    public class SupervisorController : SupervisorSessionCheckingController
    {
        SupervisorBusinessLogic objSupervisorBL = new SupervisorBusinessLogic();
        // GET: Supervisor
        public ActionResult ViewProfile(string UserID)
        {
            CommonBusinessLogic objCommonBL = new CommonBusinessLogic();
            Users objUsers = new Users();
            if (Session["Supervisor"] != null)
            {
                UserID = Session["Supervisor"].ToString();
            }
            return View("ViewProfile", objCommonBL.GetUserDetailsByUserID(UserID));
        }

        [HttpGet]
        public ActionResult UpdateSupervisorProfile(string UserID)
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
        public ActionResult UpdateSupervisorProfile(string UserID, string NAME, string MOBILE, string DOB, string STATE, string CITY)
        {
            if (ModelState.IsValid)
            {
                if (Session["Supervisor"] != null)
                {
                    UserID = Session["Supervisor"].ToString();
                }
                int? res = objSupervisorBL.UpdateSupervisorDetails(UserID, NAME, MOBILE, DOB, STATE, CITY);
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