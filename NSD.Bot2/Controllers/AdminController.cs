using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSD.Bot2.Models.Qna;
using System.Xml.Serialization;

namespace NSD.Bot2.Controllers
{
    public class AdminController : Controller
    {
        private IHostingEnvironment hostingEnv;

        public AdminController(IHostingEnvironment env)
        {
            this.hostingEnv = env;
        }

        [HttpPost]
        public IActionResult UploadFiles(IFormFile xmlfile)
        {
            if (xmlfile == null)
            {
                return new RedirectResult("/fail.html");
            }
            Qna data;
            using (var stream = xmlfile.OpenReadStream())
            {
                var serializer = new XmlSerializer(typeof(Qna));
                data = (Qna)serializer.Deserialize(stream);
            }
            return new RedirectResult("/success.html");
        }
    }
}
