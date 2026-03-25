using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct DeckInfoRequest
{
    [TdfMember("PERS")]
    public string mPersona;

    [TdfMember("UID")]
    public ulong mUserId;

    [TdfMember("VER")]
    public VersionInfo mVersionInfo;

}