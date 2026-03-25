using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ApplySalaryCapRequest
{
    [TdfMember("PID")]
    public long mPlayerCardId;

    [TdfMember("SAL")]
    public short mSalaryCap;

    [TdfMember("UID")]
    public long mUserId;

}