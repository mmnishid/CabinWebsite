using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using CabinWebsite_Application.Filters;
using CabinWebsite_Application.Models;
using CabinWebsite_Application.DB_Data;

using System.Xml.Linq;



namespace CabinSiteProject.Controllers
{
    //[CustomErrorHandling]
    [Authorize]
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        // GET: /Account/Overview
        // Overview of transactions for the current user
        public ActionResult Index()
        {
            return View();
        }


        // GET: /Account/Create
        // Create a new transaction whether producer or consumer. Just returns the view
        public ActionResult ReadXML()
        {
            XDocument doc = XDocument.Load("test.xml");
            //List<Unit> units = LoadUnits(doc.Descendants("Units").Elements("Unit"));
            xmlModel testXML = new xmlModel();
            testXML.units = LoadUnits(doc.Descendants("Units").Elements("Unit"));
            return View(testXML);
        }

        //
        public static List<Unit> LoadUnits(IEnumerable<XElement> units)
        {
            return units.Select(x => new Unit()
            {
                Name = x.Attribute("Name").Value,
                Children = LoadUnits(x.Elements("Unit"))
            }).ToList();
        }
    }

    
}