using Microsoft.AspNetCore.Mvc;

namespace NursingHome.Controllers
{
    public class SalarySlipController : Controller
    {
        public SalarySlipController()
        {

        }



        public IActionResult SalarySlip()
        {
            return View();
        }
    }
}
