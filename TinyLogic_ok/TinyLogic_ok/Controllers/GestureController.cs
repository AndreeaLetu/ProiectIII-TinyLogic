using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace TinyLogic_ck.Controllers
{
    [ApiController]
    [Route("api/gesture")]
    public class GestureController : ControllerBase
    {
        private static Process pythonProcess;
        private static string CurrentGesture = "NONE";

        [HttpPost("start")]
        public IActionResult StartGestures()
        {
            if (pythonProcess == null || pythonProcess.HasExited)
            {
                pythonProcess = new Process();
                pythonProcess.StartInfo.FileName = "python";
                pythonProcess.StartInfo.Arguments =
                    @"C:\Users\Deea\Desktop\AI-Virtual-Mouse-main\aivirtualmouseproject.py";

                pythonProcess.StartInfo.UseShellExecute = true;
                pythonProcess.Start();
            }

            return Ok(new { status = "started" });
        }

    
        [HttpPost]
        public IActionResult SetGesture([FromBody] GestureDto dto)
        {
            CurrentGesture = dto.Gesture;
            return Ok();
        }


        [HttpGet]
        public IActionResult GetGesture()
        {
            return Ok(new { gesture = CurrentGesture });
        }
    }

    public class GestureDto
    {
        public string Gesture { get; set; }
    }
}
