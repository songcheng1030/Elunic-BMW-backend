using System.Collections.Generic;

namespace AIQXCommon.Auth
{
    public enum UseCaseAppRole
    {
        AIQX_TEAM,
        REQUESTOR
    }

    public static class GroupRolesMapping
    {

        public readonly static IDictionary<UseCaseAppRole, IList<string>> Mappings = new Dictionary<UseCaseAppRole, IList<string>>()
        {
            [UseCaseAppRole.AIQX_TEAM] = new List<string>() {
                "/AIQX",
                "/AIQX_Team",
            },

            [UseCaseAppRole.REQUESTOR] = new List<string>() {
                "/Requestor",
                "/AIQX_Requestor",
            },
        };
    }
}
