using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct ISWatchListResponse
{

    [TdfMember("SRES")]
    public List<ISTradeInfo> mTradeResults;

    [TdfMember("TOTC")]
    public int mTotalCount;

}