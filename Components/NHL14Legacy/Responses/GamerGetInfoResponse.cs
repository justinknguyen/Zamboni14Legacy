using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct GamerGetInfoResponse
{
    [TdfMember("INFO")]
    public GamerInfo mGamerInfo;

    [TdfMember("UID")]
    public ulong mUserId;

}