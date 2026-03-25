using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Requests;

[TdfStruct]
public struct StorePackQuantitiesRequest
{
    [TdfMember("PTIL")]
    public List<short> mPackTypeIdList;

}