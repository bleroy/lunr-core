using System;

namespace Lunr.Globalization.tr
{
    public sealed class TurkishStopWordFilter : StopWordFilterBase
    {
        private const string Data = @"acaba altmış altı ama ancak arada aslında ayrıca bana bazı belki ben benden beni benim beri beş bile bin bir biri birkaç birkez birçok birşey birşeyi biz bizden bize bizi bizim bu buna bunda bundan bunlar bunları bunların bunu bunun burada böyle böylece da daha dahi de defa değil diye diğer doksan dokuz dolayı dolayısıyla dört edecek eden ederek edilecek ediliyor edilmesi ediyor elli en etmesi etti ettiği ettiğini eğer gibi göre halen hangi hatta hem henüz hep hepsi her herhangi herkesin hiç hiçbir iki ile ilgili ise itibaren itibariyle için işte kadar karşın katrilyon kendi kendilerine kendini kendisi kendisine kendisini kez ki kim kimden kime kimi kimse kırk milyar milyon mu mü mı nasıl ne neden nedenle nerde nerede nereye niye niçin o olan olarak oldu olduklarını olduğu olduğunu olmadı olmadığı olmak olması olmayan olmaz olsa olsun olup olur olursa oluyor on ona ondan onlar onlardan onları onların onu onun otuz oysa pek rağmen sadece sanki sekiz seksen sen senden seni senin siz sizden sizi sizin tarafından trilyon tüm var vardı ve veya ya yani yapacak yapmak yaptı yaptıkları yaptığı yaptığını yapılan yapılması yapıyor yedi yerine yetmiş yine yirmi yoksa yüz zaten çok çünkü öyle üzere üç şey şeyden şeyi şeyler şu şuna şunda şundan şunları şunu şöyle";
        private static readonly ISet<string> WordList = new Set<string>(Data.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        protected override ISet<string> StopWords => WordList;
    }
}