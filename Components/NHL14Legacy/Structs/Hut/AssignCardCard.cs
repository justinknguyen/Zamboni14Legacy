using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct AssignCardCard
{
    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("CSTT")]
    public CardState mCardStateId;

    [TdfMember("DPOS")]
    public uint mDeckPos;

    [TdfMember("DTYP")]
    public DeckType mDeckType;

}