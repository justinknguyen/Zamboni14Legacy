using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct StickerBookCardRequest
{
    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("SWAP")]
    public long mSwapCardId;

    [TdfMember("UID")]
    public ulong mUserId;
}