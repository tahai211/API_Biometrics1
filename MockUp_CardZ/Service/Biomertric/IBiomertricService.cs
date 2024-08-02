using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Biomertric
{
    public interface IBiomertricService 
    {
        ValueTask<object> RegisterBiomertric(string imagePath, string userId, string personalInfo, string exportFilePath);
        ValueTask<object> PositivelyBiomertric(string image, string dataQR);
    }
}
