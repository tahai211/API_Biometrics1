using Microsoft.EntityFrameworkCore;
using MockUp_CardZ.Data;
using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.Infra.Common.HttpCustom;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Role
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        public RoleService(AppDbContext context)
        {
            _context = context;
        }
        public async ValueTask<object> GetListRoleManagement(string roleName, string usertype, string serviceId, int pageIndex = 0, int pageSize = 15)
        {
            int skip = ((pageIndex - 1) * pageSize);

            var lstPages = _context.SysRoles.AsQueryable().Where(x => x.RoleName.Contains(roleName)
                && (serviceId == null || serviceId == "" || x.ServiceId == serviceId)
                && (string.IsNullOrEmpty(usertype) || x.UserType == usertype)
                ).Select(x => new
                {
                    RoleId = x.RoleId,
                    RoleName = x.RoleName,
                    Service = _context.SysService.Any(a => a.ServiceId == x.ServiceId) ? _context.SysService.Where(a => a.ServiceId == x.ServiceId).FirstOrDefault().ServiceName : "",
                    //UserType = Sẻ.IdpUserTypes.Any(a => a.UserTypeId == x.UserType) ? digitalContext.IdpUserTypes.Where(a => a.UserTypeId == x.UserType).FirstOrDefault().UserTypeName : x.ServiceId == "BO" ? "USER" : "",
                    RoleType = x.RoleType,
                    Active = x.Active,
                    Description = x.Description,
                    ServiceId = x.ServiceId
                });
            var total = lstPages.Count();
            if (pageSize != 0)
            {
                var data = lstPages.Skip(skip).Take(pageSize);
                int pages = lstPages.Count() % pageSize >= 1 ? lstPages.Count() / pageSize + 1 : lstPages.Count() / pageSize;
                return new { data = data, pages = pages, total = total };
            }
            else
            {
                return new { data = lstPages, pages = 0 };
            }
        }
        public async ValueTask<object> GetDetailRoleManagement(int roleId)
        {
            var role = await _context.SysRoles.AsQueryable().Where(x => x.RoleId == roleId)
            .Select(m => new
            {
                RoleId = m.RoleId,
                RoleName = m.RoleName,
                Description = m.Description,
                ServiceId = m.ServiceId,
                CChannel = _context.SysService.AsQueryable().Where(x => x.ServiceId == m.ServiceId).SingleOrDefault().CustomerChannel,
                // Service = digitalContext.IdpServices.AsQueryable().Select(x => new { ServiceId = x.ServiceId, ServiceName = x.ServiceName }).SingleOrDefault(x => x.ServiceId == m.ServiceId),
                // // UserType = digitalContext.IdpUserTypes.AsQueryable().Select(x=>new{ServiceId = x.ServiceId,UserTypeId=x.UserTypeId,UserTypeName=x.UserTypeName})
                // //     .SingleOrDefault(x=>x.ServiceId==m.ServiceId && x.UserTypeId==m.UserType),
                UserType = m.UserType,
                RoleType = m.RoleType,
                Active = m.Active
            }).FirstOrDefaultAsync();
            return role;
        }
        public async ValueTask<object> UpdateRoleManagement(int roleId, string roleName, string serviceId, string desc, string usertype, string userId,string actionType)
        {
            if (!actionType.Equals("ADD") && !actionType.Equals("EDIT"))
                throw new SysException("page_action_invalid", "Invalid action");
            if (string.IsNullOrEmpty(roleName))
                throw new SysException("rolenamecannotbeempty", "Role Name can not be empty");
            SysRole role = await _context.SysRoles.AsQueryable().Where(x => x.RoleId.ToString() == roleId.ToString()).FirstOrDefaultAsync();

            if (actionType.Equals("ADD"))
            {
                if (role != null) throw new SysException("roleisexisted", "Role ID is existed");
                role = new SysRole();
                role.DateCreated = DateTime.Now;
                role.UserCreated = userId;
            }
            else
            {
                if (role == null) throw new SysException("roleisnotexisted", "Role ID is not existed");
                role.DateModified = DateTime.Now;
                role.UserModified = userId;
            }
            role.RoleName = roleName;
            role.Description = desc;
            role.ServiceId = serviceId;
            role.UserType = usertype;
            role.Active = true;
            if (actionType.Equals("ADD"))
            {
                _context.SysRoles.Add(role);
            }
            else
            {
                // HaiTX todo
                //var removeLst = _context.IdpTranRights.AsQueryable().Where(x => x.RoleId == role.RoleId);
                //foreach (var removeItem in removeLst)
                //{
                //    digitalContext.IdpTranRights.Remove(removeItem);
                //}
            }
            List<string> keyAdded = new List<string>();
            // List<IdpTranRight> idptranlist = new List<IdpTranRight>();
            if (actionType.Equals("ADD"))
            {
                await _context.SaveChangesAsync();
            }
            //foreach (string action in permission)
            //{
            //    IdpTranRight right = new IdpTranRight();
            //    right.RoleId = role.RoleId;
            //    right.ActionId = action;
            //    right.TransactionCode = (await digitalContext.IdpActionMappings.AsQueryable().Where(m => m.ActionId == action).FirstOrDefaultAsync())?.TransactionCode;
            //    if (!keyAdded.Contains(action))
            //        digitalContext.IdpTranRights.AddRange(right);
            //    keyAdded.Add(action);
            //}
            await _context.SaveChangesAsync();
            return role.RoleId;
        }
        public async ValueTask<object> DeleteRoleManagement(dynamic role)
        {
            JArray idArr = new JArray();

            if (role is long)
            {
                idArr.Add(role);
            }
            else
            {
                idArr = role;
            }

            foreach (var item in idArr)
            {
                int id = (int)item;
                if (_context.SysUserInRoles.AsQueryable().Where(x => x.RoleId == id).Count() > 0)
                    throw new SysException("roleisused", "Role is used");

                var record = await _context.SysRoles.AsQueryable().Where(x => x.RoleId == id).FirstOrDefaultAsync();

                if (record != null)
                {
                    _context.SysRoles.Remove(record);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
