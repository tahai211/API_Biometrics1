using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ.Service.Policy
{
    public interface IPolicyService
    {
        ValueTask<object> GetListPolicyManagement( string accessGroupId, string desc, string isCms, int pageIndex = 1, int pageSize = 0);
        ValueTask<object> GetDetailPolicyManagement(int policyId);
        ValueTask<object> UpdatePolicyManagement(int policyId, string description, string efFrom, string efTo, string ctmTypeId, string accessGroupId, int pwHis,
    int pwAge, int pwMinLength, int pwMaxLength, bool pwComplex, bool pwLowerCase, bool pwUpperCase, bool pwSymbols, bool pwNumber, int resetPwTime,
    string accessTimeFrom, string accessTimeTo, int failAccessNumber, string actionType,string userId);
        ValueTask<object> DeletePolicyManagement(dynamic ids);
    }
}
