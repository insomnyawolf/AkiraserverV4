using AkiraserverV4.Http.BaseContex;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class SampleContext2 : BaseContext
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

        [Get("/test2")]
        public string Test2()
        {
            return "ASDASDFASD";
        }

        [Post("/test2")]
        public object Test2Post()
        {
            return Request.GetUrlEncodedForm();
        }
    }
}