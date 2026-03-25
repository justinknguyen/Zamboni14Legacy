using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct MoveCardResponse
{
    [TdfMember("CID")]
    public long mDisplacedCardId;

    [TdfMember("DECK")]
    public DeckType mDisplacedDeckType;

    [TdfMember("POS")]
    public uint mDisplacedCardPosition;

    [TdfMember("VER")]
    public VersionInfo mVersionInfo;
}