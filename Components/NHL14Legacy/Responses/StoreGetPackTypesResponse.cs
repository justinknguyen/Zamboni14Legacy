using System.Collections.Generic;
using Tdf;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct StoreGetPackTypesResponse
{
    [TdfMember("FRPK")]
    public short mFreePack;

    [TdfMember("PPH")]
    public byte mPremiumPacksHidden;

    [TdfMember("PTPS")]
    public List<StorePackTypeData> mPackTypeList;

    [TdfMember("SVTM")]
    public uint mServerTime;

}