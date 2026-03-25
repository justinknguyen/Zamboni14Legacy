using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct PlayGameResponse
{

    [TdfMember("BNUS")]
    public byte mBonusAwarded;

    [TdfMember("CRED")]
    public uint mCredits;

    [TdfMember("GTIC")]
    public uint mGoldenTickets;

    [TdfMember("PRES")]
    public uint mPrestige;

    [TdfMember("TRPH")]
    public byte mTrophyCardCreated;

    [TdfMember("VER")]
    public VersionInfo mVersionInfo;

}