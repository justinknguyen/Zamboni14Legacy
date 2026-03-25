using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ISAdminOfferRequest
{
    [TdfMember("OID")]
    public long mOfferId;

    [TdfMember("STAT")]
    public OfferState mOfferState;

    [TdfMember("UID")]
    public ulong mUserId;

}