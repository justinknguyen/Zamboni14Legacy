using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct ISOfferInfo
{
    [TdfMember("CARD")]
    public List<long> mCardList;

    [TdfMember("CDAT")]
    public List<CardData> mCardDataList;

    [TdfMember("CRED")]
    public uint mCredits;

    [TdfMember("OID")]
    public long mOfferId;

    [TdfMember("STAT")]
    public OfferState mOfferState;

    [TdfMember("TID")]
    public long mTradeId;

    [TdfMember("UID")]
    public ulong mUserId;

}