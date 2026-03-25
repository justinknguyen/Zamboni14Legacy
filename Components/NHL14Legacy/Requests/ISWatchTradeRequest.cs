using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ISWatchTradeRequest
{
    [TdfMember("TID")]
    public long mTradeId;

    [TdfMember("UID")]
    public long mUserId;

}