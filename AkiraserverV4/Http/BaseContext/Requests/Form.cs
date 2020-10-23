using AkiraserverV4.Http.Helper;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace AkiraserverV4.Http.Context.Requests
{
    public class Form
    {
        public List<FormInput> FormInput { get; set; } = new List<FormInput>();
        public List<FormFile> FormFile { get; set; } = new List<FormFile>();
    }

    public class BaseFormInput
    {
        public string Name { get; set; }
    }

    public class FormInput : BaseFormInput
    {
        public string Value { get; set; }
    }

    public class FormFile : BaseFormInput
    {
        public string Filename { get; set; }
        public ContentType ContentType { get; set; }
        [JsonIgnore]
        public MemoryStream Content { get; set; }
    }
}
