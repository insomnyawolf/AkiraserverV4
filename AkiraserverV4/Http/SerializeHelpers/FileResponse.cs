using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using System.IO;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class FileResponse : BinaryResponseResult
    {
        public ContentType ContentType { get; set; }
        public string Filename { get; set; }

        public FileResponse(MemoryStream obj)
        {
            Content = obj;
        }

        public FileResponse()
        {
        }

        public override async Task CustomResponse(BaseContext ctx)
        {
            ctx.Response.AddContentDisposition($"attachment; {Filename ?? ""}");
            ctx.Response.AddContentType(ContentType);
            ctx.Response.AddContentLenght((int)Content.Length);
        }
    }
}
