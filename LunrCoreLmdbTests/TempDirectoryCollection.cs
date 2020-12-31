using Xunit;

namespace LunrCoreLmdbTests
{
    [CollectionDefinition(nameof(TempDirectory))]
    public class TempDirectoryCollection : ICollectionFixture<TempDirectory> { }
}