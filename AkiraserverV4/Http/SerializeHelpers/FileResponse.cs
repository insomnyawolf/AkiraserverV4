using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using System.IO;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class FileResponse : ResponseResult
    {
        // maybe create a mime guesser?
        public override ContentType ContentType { get; set; } = ContentType.Binary; 
        public string Filename { get; set; }
        public FileStream FileStream { get; set; }

        public FileResponse(FileStream FileStream)
        {
            this.FileStream = FileStream;
            this.Filename = Path.GetFileName(FileStream.Name); 
        }

        public override async Task SerializeToNetworkStream(Response Response)
        {
            Response.AddContentType(ContentType);
            Response.AddContentLenght((int)FileStream.Length);
            Response.AddContentDisposition($"attachment; {Filename ?? ""}");

            FileStream.Position = 0;
            await FileStream.CopyToAsync(Response.NetworkStream);
        }
    }
}
