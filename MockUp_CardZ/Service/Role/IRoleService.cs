using MockUp_CardZ.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Role
{
    public interface IRoleService
    {
        ValueTask<object> GetListRoleManagement(string roleName, string usertype, string serviceId,int pageIndex = 0 , int pageSize = 15);
        ValueTask<object> GetDetailRoleManagement(int roleId);
        ValueTask<object> UpdateRoleManagement(int roleId, string roleName, string serviceId,string desc,string usertype,string userId, string actionType);
        ValueTask<object> DeleteRoleManagement(dynamic role);

    }
}
