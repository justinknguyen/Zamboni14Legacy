using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct CreatePackRequest
{
    [TdfMember("DCID")]
    public uint mCardDbId;

    [TdfMember("PTYP")]
    public PackType mPackType;

    [TdfMember("UID")]
    public uint mUserId;

}