using AkiraserverV4.Http.ContextFolder;
using AkiraserverV4.Http.Helper;
using System.Threading.Tasks;

namespace SampleServer
{
    [Controller("/Test")]
    public class SampleContext2 : Context
    {
        public SampleService Service { get; set; }

        public SampleContext2(SampleService service)
        {
            Service = service;
        }

        [Get("/Potato")]
        public async Task Counter()
        {
            byte[] test = "Potato".ToByteArray();
            Response.AddContentLenghtHeader(test.Length);
            await WriteData(test);
        }
    }
}