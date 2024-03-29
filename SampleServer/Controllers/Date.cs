﻿using AkiraserverV4.Http.Context;
using System;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class Date : CustomBaseContext
    {
        [Get]
        public string Now(bool IsShort, string random, int value)
        {
            if (IsShort)
            {
                return DateTime.Now.ToShortDateString();
            }

            if (random != null)
            {
                return random;
            }

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