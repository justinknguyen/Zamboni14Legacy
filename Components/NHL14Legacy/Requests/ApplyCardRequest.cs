using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct ApplyCardRequest
{
    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("CIDT")]
    public List<long> mTargetCards;

    [TdfMember("UID")]
    public ulong mUserId;

}