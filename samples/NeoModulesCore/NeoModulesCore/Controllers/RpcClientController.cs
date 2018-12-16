using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NeoModulesCore.Controllers
{
    public class RpcClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}