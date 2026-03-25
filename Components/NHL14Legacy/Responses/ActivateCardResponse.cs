using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct ActivateCardResponse
{

    [TdfMember("CID")]
    public long mCardId;

}