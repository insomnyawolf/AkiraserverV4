
using AkiraserverV4.Http.Anotations;
using AkiraserverV4.Http.ContextFolder;
using SuperSimpleHttpListener.Http.Helper;
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

        
        public async Task Test()
        {
            i++;
            var test = $"Hello World!\nRequest: {i}\n".ToByteArray();
            var second = JsonSerializer.Serialize(Request.Headers).ToByteArray();
            Response.AddContentLenghtHeader(test.Length + second.Length);
            await WriteData(test);
            await WriteData(second);
        }

        [DefaultRouting]
        public async Task Test2()
        {
            i++;
            StringBuilder sb = new StringBuilder();
            sb.Append($"Hello World!\nRequest: {i}\n");

            var second = JsonSerializer.Serialize(Request.Headers);

            for (i = 0; i < 1000000; i++)
            {
                sb.Append(second);
            }

            byte[] data = sb.ToString().ToByteArray();
            Response.AddContentLenghtHeader(data.Length);
            await WriteData(data);
        }
    }
}
