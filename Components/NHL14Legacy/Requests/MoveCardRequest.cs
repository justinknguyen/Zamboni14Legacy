using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct MoveCardRequest
{

    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("DECK")]
    public DeckType mDeckType;

    [TdfMember("SWAP")]
    public long mSwapCardId;

    [TdfMember("UID")]
    public ulong mUserId;

}