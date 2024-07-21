using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Biomertric
{
    public interface IBiomertricService 
    {
        ValueTask<object> RegisterBiomertric(string userName, string passWord, string serviceId);
        ValueTask<object> PositivelyBiomertric(string userName, string passWord, string serviceId);
    }
}
