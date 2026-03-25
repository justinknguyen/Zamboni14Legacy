using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct ViewCardsResponse
{
    [TdfMember("CDAT")]
    public List<CardData> mCardDataList;

}