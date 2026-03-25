using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct SquadLoadActiveRequest
{

    [TdfMember("TUID")]
    public ulong mTargetUserId;

    [TdfMember("UID")]
    public ulong mUserId;

}