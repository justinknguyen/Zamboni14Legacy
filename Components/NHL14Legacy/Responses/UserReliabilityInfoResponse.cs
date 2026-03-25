using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct UserReliabilityInfoResponse
{
    [TdfMember("DISC")]
    public byte mPreviousMatchUnfinished;

    [TdfMember("MFI")]
    public uint mMatchesFinished;

    [TdfMember("MST")]
    public uint mMatchesStarted;

    [TdfMember("REL")]
    public uint mReliability;

    [TdfMember("UID")]
    public ulong mUserId;

}