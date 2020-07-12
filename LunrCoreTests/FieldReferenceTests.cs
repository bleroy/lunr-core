using Lunr;
using System;
using Xunit;

namespace LunrCoreTests
{
    public class FieldReferenceTests
    {
        [Fact]
        public void FieldReferenceToStringCombinesDocumentReferenceAndFieldName()
        {
            var fieldRef = new FieldReference("123", "title");

            Assert.Equal("title/123", fieldRef.ToString());
        }

        [Fact]
        public void FieldReferenceFromStringSplitsTheStringIntoParts()
        {
            var fieldRef = FieldReference.FromString("title/123");

            Assert.Equal("title", fieldRef.FieldName);
            Assert.Equal("123", fieldRef.DocumentReference);
        }

        [Fact]
        public void FromStringLeavesJoinCharacterInDocRef()
        {
            var fieldRef = FieldReference.FromString("title/http://example.com/123");

            Assert.Equal("title", fieldRef.FieldName);
            Assert.Equal("http://example.com/123", fieldRef.DocumentReference);
        }

        [Fact]
        public void FromStringWithoutJoinCharacterThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                FieldReference.FromString("docRefOnly");
            });
        }
    }
}
