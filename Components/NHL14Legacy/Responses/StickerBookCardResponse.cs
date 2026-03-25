using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct StickerBookCardResponse
{
    [TdfMember("CRED")]
    public uint mTotalCredits;

    [TdfMember("VER")]
    public VersionInfo mVersionInfo;

}