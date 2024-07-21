using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CheckAttribute : Attribute
    {
        public bool CheckRole { get; }
        public bool CheckToken { get; }

        public CheckAttribute(bool checkRole = true, bool checkToken = true)
        {
            CheckRole = checkRole;
            CheckToken = checkToken;
        }
    }
}
