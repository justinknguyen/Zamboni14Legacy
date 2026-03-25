using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct DeckInfoResponse
{
    [TdfMember("DUPE")]
    public List<CardIdPair> mDuplicateEscrowCardIdPairList;

    [TdfMember("DUPU")]
    public List<CardIdPair> mDuplicateUnassignedCardIdPairList;

    [TdfMember("ECDL")]
    public List<CardData> mEscrowCardDataList;

    [TdfMember("ECNT")]
    public byte mEscrowCount;

    [TdfMember("GEN")]
    public GeneralInfo mGeneralInfo;

    [TdfMember("RATE")]
    public uint mTeamRating;

    [TdfMember("UCDL")]
    public List<CardData> mUnassignedCardDataList;

    [TdfMember("UID")]
    public ulong mUserId;

    [TdfMember("VER")]
    public VersionInfo mVersionInfo;
}