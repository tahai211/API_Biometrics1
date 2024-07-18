using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common.HttpCustom
{
    public class HttpResponseExtensions : HttpBase
    {
        public object response { get; set; }
        public object errors { get; set; }
        public HttpResponseExtensions(HttpStatusCode code, string message, object response) : base(code, message)
        {
            this.response = response;
        }
        public HttpResponseExtensions(HttpStatusCode code, string message, object response, object errors) : base(code, message)
        {
            this.response = response;
            this.errors = errors;
        }
    }
}
