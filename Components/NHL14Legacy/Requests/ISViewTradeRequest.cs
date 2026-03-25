using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ISViewTradeRequest
{
    [TdfMember("REM")]
    public uint mRemove;

    [TdfMember("TID")]
    public long mTradeId;

    [TdfMember("UID")]
    public ulong mUserId;

}