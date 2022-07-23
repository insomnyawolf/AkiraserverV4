using AkiraserverV4.Http.Helper;
using Extensions;
using System.Collections.Generic;

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
        public SubStream Content { get; set; }
    }
}
