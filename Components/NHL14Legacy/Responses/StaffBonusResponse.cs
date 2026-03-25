using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct StaffBonusResponse
{
    [TdfMember("SDAT")]
    public StaffBonusInfo mStaffBonusInfo;

}