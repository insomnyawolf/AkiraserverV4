using AkiraserverV4.Http.BaseContext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OsuServer.Controllers.FrotEnd
{
    [Controller("/frontend/web")]
    class WebContext : Context
    {
        [InputUrlEncodedForm]
        [Get("/bancho_connect.php")]
        public async Task Login()
        {

        }
    }
}
