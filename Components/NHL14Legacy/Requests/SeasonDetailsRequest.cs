using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct SeasonDetailsRequest
{
    [TdfMember("SID")]
    public uint mSeasonId;

}