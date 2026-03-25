using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct StickerBookStats2Request
{
    [TdfMember("CONT")]
    public RequestContext mContextId;

    [TdfMember("UID")]
    public long mUserId;

    [TdfMember("VALU")]
    public int mValue;

    [TdfMember("YEAR")]
    public byte mYearId;

}