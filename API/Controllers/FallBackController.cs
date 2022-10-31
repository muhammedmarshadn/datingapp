using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //we tell this fallbckcntlr  what file to serve , the we tell our api what to do with  routes that cnt understand 
    //we send them to index.html where angular app is served from, angular knw wht to do with this routes  
    public class FallBackController : Controller     //angular app view support
    {
        public ActionResult Index()             //returns the index.html
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","index.html"),"text/HTML");
        }
    }
}