using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct GeneralInfo
{
    [TdfMember("CRED")]
    public int mCredits; //EA Pucks

    [TdfMember("STAT")]
    public List<byte> mStats;
}