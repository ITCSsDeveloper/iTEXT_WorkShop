using Microsoft.AspNetCore.Mvc;


namespace MyWebAPI.Controllers
{

    public class HomeController: ControllerBase
    {
        [HttpGet("home")]
        public async Task<IActionResult> home(){
            return BadRequest();
        }

        [HttpGet("home2")]
        public async Task<IActionResult> home2(){
            return BadRequest();
        }



    }

}