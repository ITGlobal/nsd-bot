using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSD.Bot2.Models.Qna;
using System.Xml.Serialization;
using QnaMakerApi;
using QnaMakerApi.Requests;
using System.Collections.Generic;
using NSD.Bot2.Models;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.IO;
using System.Xml.Schema;
using System.Xml;

namespace NSD.Bot2.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHostingEnvironment hostingEnv;

        public AdminController(IHostingEnvironment env)
        {
            this.hostingEnv = env;
        }

        async void MakeRequest(Qna data)
        {
            using (var db = new KnowledgeBasesContext())
            {
                using (var client = new QnaMakerClient("e6a93449fd254d11a7857b500be15be3"))
                {
                    foreach (var kb in db.KB)
                    {
                        await client.DeleteKnowledgeBase(kb.KBId);
                        db.Remove(kb);
                    }
                    var count = db.SaveChanges();
                }
            }
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


        [HttpPost]
        public IActionResult UploadFilesAjax()
        {
            var files = Request.Form.Files;
            foreach (var file in files)
            {
                XmlReaderSettings qnaSettings = new XmlReaderSettings();
                qnaSettings.Schemas.Add(null, "Models/Qna/Qna.xsd");
                qnaSettings.ValidationType = ValidationType.Schema;
                qnaSettings.ValidationEventHandler += new ValidationEventHandler(QnaSettingsValidationEventHandler);

                
                using (var stream = file.OpenReadStream())
                {
                    XmlReader qna = XmlReader.Create(stream, qnaSettings);
                    try
                    {
                        while (qna.Read()) { }
                    }
                    catch (XmlSchemaValidationException e)
                    {
                        return Json(e.Message);
                    }
                }

                Qna data;
                using (var stream = file.OpenReadStream())
                {
                    var serializer = new XmlSerializer(typeof(Qna));
                    data = (Qna)serializer.Deserialize(stream);
                }

                MakeRequest(data);

            }

            string message = $"Knowledge Database uploaded successfully!";
            return Json(message);
        }

        void QnaSettingsValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning || e.Severity == XmlSeverityType.Error)
                throw new XmlSchemaValidationException(e.Message);
        }

    }
}
