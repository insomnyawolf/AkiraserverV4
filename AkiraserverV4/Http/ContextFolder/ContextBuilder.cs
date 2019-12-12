using AkiraserverV4.Http.ContextFolder.RequestFolder;
using AkiraserverV4.Http.ContextFolder.ResponseFolder;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.ContextFolder
{
    class ContextBuilder
    {
        public static async Task<T> CreateContext<T>(NetworkStream networkStream) where T : Context, new()
        {
            List<byte> buffer = new List<byte>();

            do
            {
                const int defaultSize = 8192;
                byte[] currentBuffer = new byte[defaultSize];
                int dataRead = await networkStream.ReadAsync(buffer: currentBuffer);

                if (dataRead == defaultSize)
                {
                    buffer.AddRange(currentBuffer);
                }
                else
                {
                    byte[] partialBuffer = new byte[dataRead];
                    Buffer.BlockCopy(currentBuffer, 0, partialBuffer, 0, dataRead);
                    buffer.AddRange(partialBuffer);
                }
            } while (networkStream.DataAvailable);

#if DEBUG
            var data = Encoding.ASCII.GetString(buffer.ToArray(), 0, buffer.Count);
            Console.WriteLine("Received: {0}", data);
#endif
            Type BaseTypeOfContext = typeof(T);

            while (BaseTypeOfContext != typeof(Context))
            {
                BaseTypeOfContext = BaseTypeOfContext.BaseType;
            }

            Type TypeOfContext = typeof(T);

            T Context = (T)Activator.CreateInstance(TypeOfContext);
            BaseTypeOfContext.GetProperty("NetworkStream").SetValue(obj: Context, value: networkStream);
            BaseTypeOfContext.GetProperty("Request").SetValue(obj: Context, value: new Request(buffer.ToArray()));
            BaseTypeOfContext.GetProperty("Response").SetValue(obj: Context, value: new Response());
            return Context;
        }
    }
}
