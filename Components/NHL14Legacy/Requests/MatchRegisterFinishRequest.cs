using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct MatchRegisterFinishRequest
{
    [TdfMember("ID")]
    public long mId;

    [TdfMember("STAT")]
    public MatchState mMatchState;

    [TdfMember("UID")]
    public ulong mUserId;
}