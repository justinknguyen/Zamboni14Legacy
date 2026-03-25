using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct ApplyCardResponse
{

    [TdfMember("CDAT")]
    public List<CardData> mCardDataList;

    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("UID")]
    public ulong mUserId;

}