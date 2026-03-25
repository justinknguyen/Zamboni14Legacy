using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct Division
{
    [TdfMember("NUM")]
    public uint mNumber;

    [TdfMember("SIZE")]
    public byte mSize;

    [TdfMember("TRUL")]
    public TournamentRule mTournamentRule;
}