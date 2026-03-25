using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct AssignCardsRequest
{
    [TdfMember("LIST")]
    public List<AssignCardCard> mList;

    [TdfMember("UID")]
    public long mUserId;

}