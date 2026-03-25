using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct LogoutRequest
{
    [TdfMember("DU")]
    public uint mDiscardUnassigned;

    [TdfMember("UID")]
    public long mUserId;

}