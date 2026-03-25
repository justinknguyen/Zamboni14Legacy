using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ISWatchListRequest
{
    [TdfMember("NUM")]
    public byte mPageSize;

    [TdfMember("ST")]
    public short mStart;

    [TdfMember("UID")]
    public ulong mUserId;

}