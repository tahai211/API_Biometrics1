using Microsoft.EntityFrameworkCore;
using MockUp_CardZ.Data;
using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.Infra.Common.HttpCustom;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Portal
{
    public class PortalService : IPortalService
    {
        private readonly AppDbContext _context;

        public PortalService(AppDbContext context)
        {
            _context = context;
        }
        public async ValueTask<object> GetListPortalManagement(string portalName, string portaiId,string status, int pageSize = 0, int pageIndex = 1)
        {
            bool? _status = String.IsNullOrEmpty(status) ? null : (status.Equals("1") ? true : false);

            int skip = ((pageIndex - 1) * pageSize);

            var lst = _context.SysService
                   .Where(x => (portaiId == null || x.ServiceId.Contains(portaiId))
                               && (portalName == null || x.ServiceName.Contains(portalName))
                               && (_status == null || x.Status == _status)
                           );

            var total = lst.Count();
            if (pageSize != 0)
            {
                var data = lst.Skip(skip).Take(pageSize);
                int pages = lst.Count() % pageSize >= 1 ? lst.Count() / pageSize + 1 : lst.Count() / pageSize;
                return new { data = data, pages = pages, total = total, pageIndex = pageIndex };
            }
            else
            {
                return new { data = lst, pages = 0, total = total, pageIndex = 1 };
            }
        }
        public async ValueTask<object> GetDetailPortalManagement( string serviceId)
        {
            var obj = await _context.SysService.Where(x => x.ServiceId.Equals(serviceId)).FirstOrDefaultAsync();
            if (obj == null) return new SysException("serviceIdisnotexist", "Service ID is not exist");
            return obj;
        }
        public async ValueTask<object> UpdatePortalManagement(string serviceId, string serviceName, string status, string customerChannel,int checkUserAction,int timeRevokeToken,int timeShowCountDown, string actionType)
        {
            if (!actionType.Equals("ADD") && !actionType.Equals("EDIT"))
            {
                throw new SysException("page_action_invalid", "Invalid action");
            }

            var serviceInfo = await _context.SysService.Where(x => x.ServiceId.Equals(serviceId)).FirstOrDefaultAsync();
            if (actionType.Equals("ADD"))
            {
                if (serviceInfo != null)
                {
                    throw new SysException("serviceidisexisted", "ServiceId is existed");
                }
                serviceInfo = new SysService { ServiceId = serviceId };
            }
            else
            {
                if (serviceInfo == null)
                {
                    throw new SysException("serviceidisnotexisted", "ServiceId is not existed");
                }
            }

            serviceInfo.ServiceName = serviceName;
            serviceInfo.Status = status.Equals("1") ? true : false;
            serviceInfo.CustomerChannel = customerChannel.Equals("1") ? true : false;
            serviceInfo.CheckUserAction = checkUserAction;
            serviceInfo.TimeRevokeToken = timeRevokeToken;
            serviceInfo.TimeShowCountDown = timeShowCountDown;

            if (actionType.Equals("ADD"))
            {
                _context.SysService.Add(serviceInfo);
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async ValueTask<object> DeletePortalManagement( dynamic serviceId)
        {
            JArray idArr = new JArray();
            
            if (serviceId is long)
            {
                idArr.Add(serviceId);
            }
            else
            {
                idArr = serviceId;
            }

            foreach (var item in idArr)
            {
                var record = _context.SysService.AsQueryable().SingleOrDefault(x => x.ServiceId.Equals(item.ToString()));

                if (record != null)
                {
                    _context.SysService.Remove(record);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
