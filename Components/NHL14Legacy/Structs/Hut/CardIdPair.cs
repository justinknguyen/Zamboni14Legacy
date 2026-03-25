using System.Collections.Generic;
using Blaze3SDK.Blaze.Example;
using Tdf;

namespace Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;

[TdfStruct]
public struct CardIdPair
{
    [TdfMember("CID")]
    public long mCardId;

    [TdfMember("DCID")]
    public long mDuplicateCardId;
}