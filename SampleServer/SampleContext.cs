using AkiraserverV4.Http.ContextFolder;
using AkiraserverV4.Http.ContextFolder.RequestFolder;
using AkiraserverV4.Http.Helper;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleServer
{
    [Controller]
    public class SampleContext : Context
    {
        public SampleService Service { get; set; }

        public SampleContext(SampleService service)
        {
            Service = service;
        }

        [Get("/Count")]
        public async Task Counter()
        {
            byte[] test = $"Hola!\nRequest: {Service.RequestNumber()}".ToByteArray();
            Response.AddContentLenghtHeader(test.Length);
            await WriteData(test);
        }

        [Get("/Time")]
        public async Task Time()
        {
            byte[] test = $"Now: '{DateTime.Now}'".ToByteArray();
            Response.AddContentLenghtHeader(test.Length);
            await WriteData(test);
        }

        [Get("/Request")]
        public async Task RequestMethod()
        {
            byte[] second = JsonSerializer.Serialize(Request.Headers).ToByteArray();
            Response.Status = AkiraserverV4.Http.ContextFolder.ResponseFolder.HttpStatus.NotFound;
            Response.AddContentLenghtHeader(second.Length);
            await WriteData(second);
        }

        [DefaultEndpoint]
        public async Task SampleFallback()
        {
            byte[] second = "404 NotFound".ToByteArray();
            Response.Status = AkiraserverV4.Http.ContextFolder.ResponseFolder.HttpStatus.NotFound;
            Response.AddContentLenghtHeader(second.Length);
            await WriteData(second);
        }
    }
}