using MockUp_CardZ.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Portal
{
    public interface IPortalService
    {
        ValueTask<object> GetListPortalManagement(string portalName, string portaiId, string status, int pageSize = 0, int pageIndex = 1);
        ValueTask<object> GetDetailPortalManagement( string serviceId);
        ValueTask<object> UpdatePortalManagement(string serviceId, string serviceName, string status, string customerChannel, int checkUserAction, int timeRevokeToken, int timeShowCountDown, string actionType);
        ValueTask<object> DeletePortalManagement(dynamic serviceId);
    }
}
