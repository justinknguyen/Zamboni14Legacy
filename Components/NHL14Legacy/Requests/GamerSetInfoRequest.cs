using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct GamerSetInfoRequest
{
    [TdfMember("INFO")]
    public GamerInfo mGamerInfo;

    [TdfMember("UID")]
    public long mUserId;

}