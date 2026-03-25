using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ChangePlayersRequest
{
    [TdfMember("CARD")]
    public List<CardData> mCardDataList;

    [TdfMember("UID")]
    public long mUserId;

}