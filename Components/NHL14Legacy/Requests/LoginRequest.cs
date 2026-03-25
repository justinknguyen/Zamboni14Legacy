using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct LoginRequest
{
    [TdfMember("CP")]
    public uint mCreatePlayer;

    [TdfMember("PERS")]
    public string mPersona;

    [TdfMember("PUR")]
    public uint mPurchased;

    [TdfMember("UID")]
    public ulong mUserId;
}