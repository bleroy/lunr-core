// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Lunr;

namespace LunrCoreTests
{
    public static class IndexExtensions
    {
        public static DelegatedIndex AsDelegated(this Index index)
        {
            return new DelegatedIndex(key => index.InvertedIndex[key], () => index.FieldVectors.Keys,
                key => index.FieldVectors[key], other => index.TokenSet.Intersect(other), () => index.Fields,
                index.Pipeline);
        }
    }
}