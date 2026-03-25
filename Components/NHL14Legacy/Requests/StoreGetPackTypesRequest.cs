using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct StoreGetPackTypesRequest
{
    [TdfMember("GPID")]
    public int mGroupId;

    [TdfMember("UID")]
    public ulong mUserId;
}