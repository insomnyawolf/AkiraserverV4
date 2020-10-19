﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
    public class GeneralSettings
    {
        public int Port { get; set; }
        public bool ExclusiveAddressUse { get; set; }
        public bool UseOnlyOverlappedIO { get; set; }
        public short Ttl { get; set; }
        public RequestSettings RequestSettings { get; set; }
        public ResponseSettings ResponseSettings { get; set; }
    }

    public class RequestSettings
    {
        public int ReciveTimeout { get; set; }
        public int ReadPacketSize { get; set; }
    }
    
    public class ResponseSettings
    {
        public int SendTimeout { get; set; }
    }
}
