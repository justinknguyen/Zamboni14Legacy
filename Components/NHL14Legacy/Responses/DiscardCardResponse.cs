using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct DiscardCardResponse
{
    [TdfMember("CRED")]
    public int mCredits;

    [TdfMember("VER")]
    public VersionInfo mVersionInfo;

}