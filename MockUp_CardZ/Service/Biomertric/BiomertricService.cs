using MockUp_CardZ.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Biomertric
{
    public class BiomertricService : IBiomertricService
    {
        private readonly AppDbContext _context;
        public BiomertricService(AppDbContext context)
        {
            _context = context;
        }
        public async ValueTask<object> RegisterBiomertric(string userName, string passWord, string serviceId)
        {
            return true;
        }
        public async ValueTask<object> PositivelyBiomertric(string userName, string passWord, string serviceId)
        {
            return true;
        }
    }
}
