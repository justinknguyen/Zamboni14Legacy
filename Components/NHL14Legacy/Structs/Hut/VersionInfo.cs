using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct VersionInfo
{
    [TdfMember("VESC")]
    public uint mVersionEscrow;

    [TdfMember("VGEN")]
    public uint mVersionGeneral;

    [TdfMember("VUNA")]
    public uint mVersionUnassigned;
}