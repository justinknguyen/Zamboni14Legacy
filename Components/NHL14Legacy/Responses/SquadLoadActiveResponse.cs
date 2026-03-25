using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct SquadLoadActiveResponse
{
    [TdfMember("ACTV")]
    public List<CardData> mActiveCards;

    [TdfMember("SQAD")]
    public SquadInfo mSquadInfo;

    [TdfMember("TUID")]
    public long mTargetUserId;

}