﻿using AkiraserverV4.Http.BaseContex;
using System;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class Date : Context
    {
        [Get]
        public string Now()
        {
            return DateTime.Now.ToString();
        }

        [Get("/[method]")]
        public string ShortDate()
        {
            return DateTime.Now.ToShortDateString();
        }

        [Get("/[method]")]
        public string ShortTime()
        {
            return DateTime.Now.ToShortTimeString();
        }
    }
}