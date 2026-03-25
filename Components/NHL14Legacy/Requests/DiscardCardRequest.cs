using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct DiscardCardRequest
{
    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("CRED")]
    public int mCredits;

    [TdfMember("UID")]
    public long mUserId;

}