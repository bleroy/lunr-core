// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace LunrCore.Lmdb
{
    public static class FieldKeyBuilder
    {
        public static byte[] BuildAllFieldsKey() => Encoding.UTF8.GetBytes("F:");

        public static byte[] BuildFieldKey(string field)
        {
            return Encoding.UTF8.GetBytes($"F:{field}");
        }
    }
}