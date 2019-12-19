﻿using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Extensions
{
    internal static class ContextExtensions
    {
        internal static async Task SendText(this BaseContext context, object text)
        {
#warning Optimize Already String Inputs

            byte[] responseBytes = Encoding.UTF8.GetBytes(text.ToString());
            if (!context.Response.Headers.ContainsKey("Content-Length"))
            {
                context.Response.AddContentLenghtHeader(responseBytes.Length);
            }
            await context.WriteData(responseBytes);
        }

        internal static async Task SendRaw(this BaseContext context, object data)
        {
            throw new NotImplementedException();
        }

        internal static async Task SendObject(this BaseContext context, object data)
        {
            throw new NotImplementedException();
        }

        internal static async Task SendJson<T>(this BaseContext context, T data) where T: JsonResult
        {
            await context.SendText(data.SerializedJson);
        }

        
    }
}