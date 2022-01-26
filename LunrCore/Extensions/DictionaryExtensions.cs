using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lunr;

internal static class DictionaryExtensions
{
    public static void Increment(this Dictionary<string, int> dic, string fieldName, int amount = 1)
    {
#if NET6_0_OR_GREATER
        ref int value = ref CollectionsMarshal.GetValueRefOrAddDefault(dic, fieldName, out _);

        value += amount;
#else
        if (dic.ContainsKey(fieldName))
        {
            dic[fieldName] += amount;
        }
        else 
        {
            dic.Add(fieldName, amount);
        }
#endif

    }

    public static void Increment(this Dictionary<string, double> dic, string fieldName, double amount = 1)
    {
#if NET6_0_OR_GREATER
        ref double value = ref CollectionsMarshal.GetValueRefOrAddDefault(dic, fieldName, out _);

        value += amount;
#else
        if (dic.ContainsKey(fieldName))
        {
            dic[fieldName] += amount;
        }
        else 
        {
            dic.Add(fieldName, amount);
        }
#endif

    }

}
