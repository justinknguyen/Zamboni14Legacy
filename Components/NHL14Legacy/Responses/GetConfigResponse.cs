using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct GetConfigResponse
{

    [TdfMember("GCFL")]
    public List<uint> mConfigList;

}