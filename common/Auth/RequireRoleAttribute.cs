using System.Collections.Generic;
using System.Linq;

namespace AIQXCommon.Auth
{
    public class RequireRole : System.Attribute
    {
        public List<UseCaseAppRole> allowedRoles = new List<UseCaseAppRole>();

        public RequireRole(params UseCaseAppRole[] roles)
        {
            allowedRoles = roles.ToList<UseCaseAppRole>();
        }
    }

}
