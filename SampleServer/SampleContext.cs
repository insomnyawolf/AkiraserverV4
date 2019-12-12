
using AkiraserverV4.Http.Anotations;
using AkiraserverV4.Http.ContextFolder;
using AkiraserverV4.Http.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleServer
{
    public class SampleContext : Context
    {
        private static int i = 0;

        [DefaultRouting]
        public async Task Test()
        {
            i++;
            var test = $"Hola!\nRequest: {i}\n".ToByteArray();
            var second = JsonSerializer.Serialize(Request.Headers).ToByteArray();
            Response.AddContentLenghtHeader(test.Length + second.Length);
            await WriteData(test);
            await WriteData(second);
        }
    }
}
