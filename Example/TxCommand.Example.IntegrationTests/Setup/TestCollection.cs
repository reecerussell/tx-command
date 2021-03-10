using Xunit;

namespace TxCommand.Example.IntegrationTests.Setup
{
    [CollectionDefinition("IntegrationTests")]
    public class TestCollection : ICollectionFixture<DatabaseSetup>
    {
    }
}
