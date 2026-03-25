using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct ISViewTradeResponse
{

    [TdfMember("CRED")]
    public int mCredits;

    [TdfMember("INFO")]
    public ISTradeInfo mISTradeInfo;

}