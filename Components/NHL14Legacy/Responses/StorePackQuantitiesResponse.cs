using System.Collections.Generic;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Responses;

[TdfStruct]
public struct StorePackQuantitiesResponse
{
    [TdfMember("PQTL")]
    public List<int> mPackQuantitiesList;

}