using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common
{
    public static class HttpHelper
    {
        private static IHttpContextAccessor _accessor;
        private static int _formatNumber;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }
        public static void SetFormatNumber(int formatNumber)
        {
            _formatNumber = formatNumber;
        }

        public static HttpContext HttpContext => _accessor.HttpContext;
        public static int FormatNumber => _formatNumber;
    }
}
