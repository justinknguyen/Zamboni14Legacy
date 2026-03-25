using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct StickerBookSearchRequest
{
    [TdfMember("COLL")]
    public byte mCollectionYearId;

    [TdfMember("COUN")]
    public int mCountryId;

    [TdfMember("CTYP")]
    public CollectionSearchType mCollectionSearchCardType;

    [TdfMember("FORM")]
    public int mFormation;

    [TdfMember("LEAG")]
    public int mLeagueId;

    [TdfMember("LEV")]
    public CardLevel mCardLevel;

    [TdfMember("NAT")]
    public int mNation;

    [TdfMember("NUMR")]
    public int mNumRetreive;

    [TdfMember("POS")]
    public int mPosition;

    [TdfMember("STAT")]
    public CardState mCardState;

    [TdfMember("STRT")]
    public int mStart;

    [TdfMember("TEAM")]
    public int mTeamId;

    [TdfMember("UID")]
    public long mUserId;

}