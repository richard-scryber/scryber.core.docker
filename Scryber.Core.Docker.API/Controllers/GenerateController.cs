using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Buffers.Text;
using System.Security.Cryptography;
using Scryber.Online.API.Models;

namespace Scryber.Online.API.Controllers
{
    [ApiController()]
    [Route("[controller]")]
    public class GenerateController : ControllerBase
    {

        private readonly ILogger<GenerateController> _logger;
        private readonly IWebHostEnvironment _env;

        public GenerateController(ILogger<GenerateController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        [Produces("application/pdf")]
        public async Task<FileStreamResult> GetFromUrl(string outputfile, string template)
        {
            using (_logger.BeginScope("Generate Direct " + template))
            {
                var data = new GenerationRequest()
                {
                    Template = new GenerationTemplateData() { Url = template },
                    Output = new GenerationOutputData() { FileName = outputfile }
                };

                _logger.LogInformation("Beginning Document Creation");

                var result = await InvokeCreation(data);

                result.Flush();
                result.Position = 0;

                _logger.LogInformation("Completed Document Creation");

                return new FileStreamResult(result, "application/pdf");
            }
        }


        [HttpPost]
        public async Task<Models.GenerationResult> Get(Models.GenerationRequest request)
        {

            bool success = false;
            string errors = "";
            long binlength = -1;
            string base64 = "";

            var name = request.Output.FileName;
            if (string.IsNullOrEmpty(name))
                name = "Document.pdf";


            MemoryStream content = null;
            try
            {
                content = await InvokeCreation(request);
                content.Flush();

                binlength = content.Length;
                content.Position = 0;
                base64 = Convert.ToBase64String(content.ToArray());
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                errors = ex.Message;
            }
            finally
            {
                if (null != content)
                    await content.DisposeAsync();
            }

            GenerationResultData data = null;

            if(success)
            {
                data = new GenerationResultData() { Base64 = base64, Length = base64.Length, DecodedLength = binlength };
            }

            GenerationResult result = new GenerationResult()
            {
                Data = data,
                Success = success,
                Errors = errors,
                OriginalSource = request.Template.Url,
                FileName = name
            };

            return result;

        }


        [HttpPost]
        [Route("inline")]
        public async Task<FileStreamResult> Inline(Models.GenerationRequest data)
        {

            var content = await Task.Run(() => { return InvokeCreation(data); });
            

            return File(content, "application/pdf");
        }



        private async Task<MemoryStream> InvokeCreation(Models.GenerationRequest data)
        {
            MemoryStream ms;
            var path = data.Template.Url;
            var client = new System.Net.WebClient();

            var src = await client.DownloadDataTaskAsync(path);
            Scryber.Components.Document doc = null;

            using (ms = new MemoryStream(src))
            {
                doc = Scryber.Components.Document.ParseDocument(ms, path, ParseSourceType.RemoteFile);
            }

            if (data.Params != null)
            {
                foreach (var p in data.Params)
                {
                    doc.Params[p.Key] = p.Value;
                }
            }
            
            

            ms = new MemoryStream();

            doc.SaveAsPDF(ms, true);
            

            return ms;
        }
    }
}
