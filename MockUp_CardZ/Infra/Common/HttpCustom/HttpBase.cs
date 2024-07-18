using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common.HttpCustom
{
    public class HttpBase
    {
        public HttpStatusCode code { get; set; }
        public object message { get; set; }

        public HttpBase(HttpStatusCode code, object message)
        {
            this.code = code;
            this.message = message;
        }
    }
}
