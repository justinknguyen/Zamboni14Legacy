using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct GamerGetInfoRequest
{
    [TdfMember("TUID")]
    public ulong mTargetUserId;

    [TdfMember("UID")]
    public ulong mUserId;

}