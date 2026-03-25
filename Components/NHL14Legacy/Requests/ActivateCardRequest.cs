using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ActivateCardRequest
{
    [TdfMember("ATYP")]
    public CardState mActiveState;

    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("UID")]
    public long mUserId;

}