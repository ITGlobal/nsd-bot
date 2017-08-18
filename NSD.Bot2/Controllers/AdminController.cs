using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSD.Bot2.Models.Qna;
using System.Xml.Serialization;
using QnaMakerApi;
using QnaMakerApi.Requests;
using System.Collections.Generic;
using NSD.Bot2.Models;

namespace NSD.Bot2.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHostingEnvironment hostingEnv;

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

            MakeRequest(data);
            return new RedirectResult("/success.html");
        }

        async void MakeRequest(Qna data)
        {
            foreach (Section section in data.Section)
            {
                List<QnaPair> qnapairs = new List<QnaPair>();
                foreach (Answer answer in section.Answer)
                {
                    foreach (string question in answer.Question)
                    {
                        QnaPair qnapair = new QnaPair(question, answer.text);
                        qnapairs.Add(qnapair);
                    }
                }
                using (var client = new QnaMakerClient("e6a93449fd254d11a7857b500be15be3"))
                {
                    var result = await client.CreateKnowledgeBase(section.name, qnapairs);
                    UpdateActions deleteDefault = new UpdateActions();
                    deleteDefault.QnaPairs.Add(new QnaPair("Hi", "Hello"));
                    await client.UpdateKownledgeBase(result.Guid, null, deleteDefault);
                    using (var db = new KnowledgeBasesContext())
                    {
                        db.KB.Add(new KB(result.Guid, section.name));
                        var count = db.SaveChanges();
                    }
                }
            }
        }

    }
}
