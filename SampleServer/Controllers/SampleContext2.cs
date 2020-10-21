﻿using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.SerializeHelpers;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class Sample2Context : CustomBaseContext
    {
        [Get("/Potato")]
        public string Potato()
        {
            return "Potato";
        }

        [Get("/[method]")]
        public string Test()
        {
            return "ASDASDFASD";
        }

        [Get("/[method]")]
        public string Suraimu()
        {
            return "Nombre";
        }
    }
}