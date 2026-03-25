using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct SeasonConfiguration
{
    [TdfMember("DIVL")]
    public List<Division> mDivisionList;

    [TdfMember("LGID")]
    public uint mLeagueID;

    [TdfMember("LNAM")]
    public string mLeagueName;

    [TdfMember("MTYP")]
    public MemberType mMemberType;

    [TdfMember("SID")]
    public uint mSeasonID;

    [TdfMember("SPRT")]
    public StatPeriod mStatPeriodEnum;

    [TdfMember("TID")]
    public uint mTeamID;
}