using System.Collections.Generic;
using System.IO;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public class Form
    {
        public List<FormInput> FormInput { get; set; }
        public List<FormFile> FormFile { get; set; }
    }

    public class FormInput
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class FormFile : FormInput
    {
        public string Filename { get; set; }
        public MemoryStream Data { get; set; }
    }
}
