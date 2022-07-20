using AkiraserverV4.Http.Context;

namespace AkiraserverV4.Http
{
    public class GeneralSettings
    {
        public int Port { get; set; } = 80;
        public bool ExclusiveAddressUse { get; set; } = true;
        public short Ttl { get; set; } = 128;
        public int BufferSize { get; set; } = 8192;
        public RequestSettings RequestSettings { get; set; } = new RequestSettings();
        public ResponseSettings ResponseSettings { get; set; } = new ResponseSettings();
    }

    public class RequestSettings
    {
        public int ReciveTimeout { get; set; }
        public int ReadPacketSize { get; set; }
    }
    
    public class ResponseSettings
    {
        public int SendTimeout { get; set; }
        public HttpResponseHeaders StaticHttpResponseHeaders { get; set; }
    }
}
