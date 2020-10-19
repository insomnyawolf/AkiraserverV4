using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
