using Microsoft.EntityFrameworkCore;
using MockUp_CardZ.Data;
using MockUp_CardZ.DTO.Entity;
using MockUp_CardZ.Infra.Common.HttpCustom;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Policy
{
    public class PolicyService : IPolicyService
    {
        private readonly AppDbContext _context;
        public PolicyService(AppDbContext context)
        {
            _context = context;
        }
        public async ValueTask<object> GetListPolicyManagement(string? accessGroupId, string? desc, string? isCms, int pageIndex = 1, int pageSize = 0)
        {
            var services = await _context.SysServices.ToListAsync();
            var policies = _context.SysPolicies.GroupBy(y => new { y.PolicyId, y.Description, y.EfFrom, y.EfTo, y.ServiceId })
            .Select(y => new { AccessGroupId = y.Key.ServiceId, PolicyId = y.Key.PolicyId, Description = y.Key.Description, EfFrom = y.Key.EfFrom, EfTo = y.Key.EfTo });
            var pls = await _context.SysPolicies.Where(x => x.Description.Contains(desc)
                && (string.IsNullOrEmpty(accessGroupId) || x.ServiceId == accessGroupId)
                && (string.IsNullOrEmpty(isCms) || x.ServiceId != isCms)
                )
                .Join(policies, x => x.PolicyId, y => y.PolicyId, (x, y) => new { x, y })
                .Where(item =>  item.x.Description == item.y.Description && item.x.EfFrom == item.y.EfFrom && item.x.EfTo == item.y.EfTo)
                .Select(x => x.y).Distinct().ToListAsync();

            var lst = pls.GroupBy(x => new { x.PolicyId, x.Description, x.EfFrom, x.EfTo, x.AccessGroupId })
                .Select(x => new
                {
                    PolicyId = x.Key.PolicyId,
                    Description = x.Key.Description,
                    AccessGroupId = x.Key.AccessGroupId,
                    EfFrom = ((DateTime)x.Key.EfFrom).ToString("dd-MM-yyyy"),
                    EfTo = ((DateTime)x.Key.EfTo).ToString("dd-MM-yyyy"),
                    //CtmTypeIdStr = GetCtmName(x.Key.CtmTypeId, custtypes),
                    //AccessGroupName = GetAccessGroupName(x.Key.AccessGroupId),
                    //IsDefault = isDefault(x.Key.PolicyId, x.Key.AccessGroupId)
                });

            int skip = ((pageIndex - 1) * pageSize);
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
            return true;
        }
        public async ValueTask<object> GetDetailPolicyManagement(int policyId)
        {
            var detail = await _context.SysPolicies.Where(x => x.PolicyId == policyId)
            .Select(x => new
            {
                PolicyId = x.PolicyId,
                Description = x.Description,
                AccessGroupId = x.ServiceId,
                EfFrom = x.EfFrom,
                EfTo = x.EfTo,
                PwHis = x.PwHis,
                PwAge = x.PwAge,
                PwMinLength = x.PwMinLength,
                PwMaxLength = x.PwMaxLength,
                PwComplex = x.PwComplex,
                PwLowerCase = x.PwLowerCase,
                PwUpperCase = x.PwUpperCase,
                PwSymbols = x.PwSymbols,
                PwNumber = x.PwNumber,
                AccessTimeFrom = x.AccessTimeFrom,
                AccessTimeTo = x.AccessTimeTo,
                FailAccessNumber = x.FailAccessNumber,
                ResetPwTime = x.ResetPwTime,
                UserCreated = x.UserCreated,
                DateCreated = x.DateCreated,
                UserModified = x.UserModified,
                DateModified = x.DateModified
            }).FirstOrDefaultAsync();
            return detail;
        }
        public async ValueTask<object> UpdatePolicyManagement(int? policyId, string? description, string? efFrom, string? efTo, string? ctmTypeId, string? accessGroupId, int? pwHis,
    int? pwAge, int? pwMinLength, int? pwMaxLength, bool pwComplex, bool pwLowerCase, bool pwUpperCase, bool pwSymbols, bool pwNumber, int resetPwTime,
    string accessTimeFrom, string accessTimeTo, int failAccessNumber, string actionType, string userId)
        {
            if (!actionType.Equals("ADD") && !actionType.Equals("EDIT"))
                throw new SysException("page_action_invalid", "Invalid action");
            var checkPolicyId = await _context.SysPolicies.OrderByDescending(x => x.PolicyId).FirstOrDefaultAsync();
            List<SysPolicy> listpolicyinfo = new List<SysPolicy>();
            if (actionType.Equals("ADD"))
            {
                if (pwComplex == true && pwLowerCase == false && pwUpperCase == false && pwSymbols == false && pwNumber == false)
                {
                    throw new SysException("policychooserequiment", "Please choose 1 requirement!", "");
                }
                var policyCheck = await _context.SysPolicies.Where(x => x.ServiceId.Contains(accessGroupId)
                && x.Description.Contains(description)
                && x.EfFrom == DateTime.Parse(efFrom)
                && x.EfTo == DateTime.Parse(efTo)
                && x.PwHis == pwHis
                && x.PwAge == pwAge
                && x.PwMinLength == pwMinLength
                && x.PwMaxLength == pwMaxLength
                && x.PwComplex == pwComplex
                && x.PwLowerCase == pwLowerCase
                && x.PwUpperCase == pwUpperCase
                && x.PwSymbols == pwSymbols
                && x.PwNumber == pwNumber
                && x.AccessTimeFrom == TimeSpan.Parse(accessTimeFrom)
                && x.AccessTimeTo == TimeSpan.Parse(accessTimeTo)
                && x.FailAccessNumber == failAccessNumber
                && x.ResetPwTime == resetPwTime
                ).FirstOrDefaultAsync();
                if (policyCheck != null)
                {
                    throw new SysException("policyisexisted", "Policy is existed", "");
                }
                SysPolicy policy = new SysPolicy();
                if (checkPolicyId == null)
                {
                    policy.PolicyId = 1;
                }
                else
                {
                    policy.PolicyId = checkPolicyId.PolicyId + 1;
                }
                policy.ServiceId = accessGroupId;
                policy.Description = description;
                policy.EfFrom = DateTime.Parse(efFrom);
                policy.EfTo = DateTime.Parse(efTo);
                policy.PwHis = pwHis;
                policy.PwAge = pwAge;
                policy.PwMinLength = pwMinLength;
                policy.PwMaxLength = pwMaxLength;
                policy.PwComplex = pwComplex;
                policy.PwLowerCase = pwLowerCase;
                policy.PwUpperCase = pwUpperCase;
                policy.PwSymbols = pwSymbols;
                policy.PwNumber = pwNumber;
                policy.AccessTimeFrom = TimeSpan.Parse(accessTimeFrom);
                policy.AccessTimeTo = TimeSpan.Parse(accessTimeTo);
                policy.FailAccessNumber = failAccessNumber;
                policy.UserCreated = userId;
                policy.DateCreated = DateTime.Now;
                policy.ResetPwTime = resetPwTime;
                _context.SysPolicies.Add(policy);
            }

            if (actionType.Equals("EDIT"))
            {
                if (pwComplex == true && pwLowerCase == false && pwUpperCase == false && pwSymbols == false && pwNumber == false)
                {
                    throw new SysException("policychooserequiment", "Please choose 1 requirement!", "");
                }
                var policy = await _context.SysPolicies.Where(x => x.PolicyId == policyId).FirstOrDefaultAsync();
                policy.Description = description;
                policy.EfFrom = DateTime.Parse(efFrom);
                policy.EfTo = DateTime.Parse(efTo);
                policy.PwHis = pwHis;
                policy.PwAge = pwAge;
                policy.PwMinLength = pwMinLength;
                policy.PwMaxLength = pwMaxLength;
                policy.PwComplex = pwComplex;
                policy.PwLowerCase = pwLowerCase;
                policy.PwUpperCase = pwUpperCase;
                policy.PwSymbols = pwSymbols;
                policy.PwNumber = pwNumber;
                policy.AccessTimeFrom = TimeSpan.Parse(accessTimeFrom);
                policy.AccessTimeTo = TimeSpan.Parse(accessTimeTo);
                policy.FailAccessNumber = failAccessNumber;
                policy.ResetPwTime = resetPwTime;
                policy.UserModified = userId;
                policy.DateModified = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async ValueTask<object> DeletePolicyManagement(dynamic ids)
        {
            if (ids is string)
            {
                int id = ids;

                if (await _context.SysLoginInfos.AnyAsync(x => x.PolicyId == id && x.Status == "A"))
                    throw new SysException("policyisused", "Policy is used in User!");
                //if (id == agentDefault || id == custDefault)
                //{
                //    throw new SysException("policyisdefault", "this policy is default");
                //}
                var policies = await _context.SysPolicies.AsQueryable().Where(x => x.PolicyId == id).ToListAsync();
                foreach (var action in policies)
                    _context.SysPolicies.Remove(action);
            }
            else
            {
                JArray idArr = ids;
                foreach (var item in idArr)
                {
                    int id = (int)item;
                    if (await _context.SysLoginInfos.AnyAsync(x => x.PolicyId == id && x.Status == "A"))
                        throw new SysException("policyisused", "Policy is used in User!");
                    //if (id == agentDefault || id == custDefault)
                    //{
                    //    throw new SysException("policyisdefault", "this policy is default");
                    //}
                    var policies = await _context.SysPolicies.AsQueryable().Where(x => x.PolicyId == id).ToListAsync();
                    foreach (var action in policies)
                        _context.SysPolicies.Remove(action);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
