using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ViewCardsRequest
{
    [TdfMember("CARD")]
    public List<long> mCardIdList;

    [TdfMember("UID")]
    public ulong mUserId;
}