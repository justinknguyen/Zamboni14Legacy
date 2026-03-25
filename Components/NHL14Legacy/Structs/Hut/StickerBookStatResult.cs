using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct StickerBookStatResult
{
    [TdfMember("CTTP")]
    public ResultContext mContextId;

    [TdfMember("CTVL")]
    public int mContextValue;

    [TdfMember("TYPE")]
    public ResultType mTypeId;

    [TdfMember("VALU")]
    public int mValue;

}