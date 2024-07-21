using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Common.HttpCustom
{
    public class SysException : Exception
    {
        public string ErrorCode { get; }
        public SysException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
        public SysException(string errorCode, string message, object additionalData) : base(FormatMessage(message, additionalData))
        {
            ErrorCode = errorCode;
        }

        private static string FormatMessage(string message, object additionalData)
        {
            if (additionalData != null)
            {
                var properties = additionalData.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var placeholder = $"{{{property.Name}}}";
                    var value = property.GetValue(additionalData)?.ToString();
                    message = message.Replace(placeholder, value);
                }
            }
            return message;
        }
    }
}
