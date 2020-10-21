﻿using System;
using System.Text.Json;
using AkiraserverV4.Http.Helper;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class JsonResult : ResponseResult
    {
        public JsonResult(object obj) : base(obj)
        {
            ContentType = ContentType.JSON;
        }

        public override string Serialize()
        {
            return JsonSerializer.Serialize(Content);
        }
    }

    public class JsonDeserialize
    {
        public const string ContentType = "application/json";

        public static object DeSerialize(Type type, string data)
        {
            return JsonSerializer.Deserialize(data, type);
        }
    }
}