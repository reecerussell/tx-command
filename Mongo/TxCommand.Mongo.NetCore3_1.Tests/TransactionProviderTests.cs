using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TxCommand.Abstractions.Exceptions;
using Xunit;

namespace TxCommand.Mongo.NetCore3_1.Tests
{
    public class TransactionProviderTests
    {
        [Fact]
        public void Ctor_GivenNullClient_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TransactionProvider(null, new MongoOptions()));
            ex.ParamName.Should().Be("client");
        }

        [Fact]
        public void Ctor_GivenNullOptions_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TransactionProvider(Mock.Of<IMongoClient>(), null));
            ex.ParamName.Should().Be("options");
        }

        [Fact]
        public async Task EnsureTransactionAsync_WhereNoSessionExists_StartsNewSession()
        {
            var session = new Mock<IClientSessionHandle>();
            session.SetupGet(x => x.IsInTransaction).Returns(true);

            var cancellationToken = CancellationToken.None;
            var options = new MongoOptions();

            var client = new Mock<IMongoClient>();
            client.Setup(x => x.StartSessionAsync(options.SessionOptions, cancellationToken))
                .ReturnsAsync(session.Object)
                .Verifiable();

            var provider = new TransactionProvider(client.Object, options);
            await provider.EnsureTransactionAsync(cancellationToken);

            client.VerifyAll();
            client.Verify(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task EnsureTransactionAsync_WhereSessionIsNotInTransaction_StartsTransaction()
        {
            var cancellationToken = CancellationToken.None;
            var options = new MongoOptions();

            var session = new Mock<IClientSessionHandle>();
            session.SetupGet(x => x.IsInTransaction).Returns(false);
            session.Setup(x => x.StartTransaction(options.TransactionOptions)).Verifiable();

            var client = new Mock<IMongoClient>();

            var provider = new TransactionProvider(client.Object, options);

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);

            await provider.EnsureTransactionAsync(cancellationToken);

            client.VerifyAll();
            client.Verify(x => x.StartSessionAsync(options.SessionOptions, cancellationToken), Times.Never);
            session.VerifyAll();
        }

        [Fact]
        public async Task EnsureTransactionAsync_WhereSessionIsInTransaction_DoesNothing()
        {
            var session = new Mock<IClientSessionHandle>();
            session.SetupGet(x => x.IsInTransaction).Returns(true);

            var cancellationToken = CancellationToken.None;
            var options = new MongoOptions();

            var client = new Mock<IMongoClient>();

            var provider = new TransactionProvider(client.Object, options);

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);

            await provider.EnsureTransactionAsync(cancellationToken);

            client.VerifyAll();
            client.Verify(x => x.StartSessionAsync(options.SessionOptions, cancellationToken), Times.Never);
            session.VerifyAll();
            session.Verify(x => x.StartTransaction(options.TransactionOptions), Times.Never);
        }

        [Fact]
        public async Task EnsureTransactionAsync_WhereDisposed_Throws()
        {
            var provider = new TransactionProvider(Mock.Of<IMongoClient>(), new MongoOptions());

            provider.GetType().GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, true);

            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                provider.EnsureTransactionAsync(CancellationToken.None));
        }

        [Fact]
        public void Commit_WhereSessionIsActive_Commits()
        {
            var session = new Mock<IClientSessionHandle>();
            session.SetupGet(x => x.IsInTransaction).Returns(true);

            var cancellationToken = CancellationToken.None;
            var options = new MongoOptions();

            var client = new Mock<IMongoClient>();

            var provider = new TransactionProvider(client.Object, options);

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);

            provider.Commit();

            client.VerifyAll();
            session.VerifyAll();
            session.Verify(x => x.CommitTransaction(default), Times.Once);
        }

        [Fact]
        public void Commit_WhereSessionIsNotActive_Throws()
        {
            var session = new Mock<IClientSessionHandle>();
            session.SetupGet(x => x.IsInTransaction).Returns(false);

            var cancellationToken = CancellationToken.None;
            var options = new MongoOptions();

            var client = new Mock<IMongoClient>();

            var provider = new TransactionProvider(client.Object, options);

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);

            Assert.Throws<TransactionNotStartedException>(() => provider.Commit());

            client.VerifyAll();
            session.VerifyAll();
            session.Verify(x => x.CommitTransaction(default), Times.Never);
        }

        [Fact]
        public void Commit_WhereDisposed_Throws()
        {
            var provider = new TransactionProvider(Mock.Of<IMongoClient>(), new MongoOptions());

            provider.GetType().GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, true);

            Assert.Throws<ObjectDisposedException>(() => provider.Commit());
        }

        [Fact]
        public async Task RollbackAsync_WhereSessionIsActive_Aborts()
        {
            var session = new Mock<IClientSessionHandle>();
            session.SetupGet(x => x.IsInTransaction).Returns(true);

            var cancellationToken = CancellationToken.None;
            var options = new MongoOptions();

            var client = new Mock<IMongoClient>();

            var provider = new TransactionProvider(client.Object, options);

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);

            await provider.RollbackAsync(cancellationToken);

            client.VerifyAll();
            session.VerifyAll();
            session.Verify(x => x.AbortTransactionAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task RollbackAsync_WhereSessionIsNotActive_Throws()
        {
            var session = new Mock<IClientSessionHandle>();
            session.SetupGet(x => x.IsInTransaction).Returns(false);

            var cancellationToken = CancellationToken.None;
            var options = new MongoOptions();

            var client = new Mock<IMongoClient>();

            var provider = new TransactionProvider(client.Object, options);

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);

            await Assert.ThrowsAsync<TransactionNotStartedException>(() => provider.RollbackAsync(cancellationToken));

            client.VerifyAll();
            session.VerifyAll();
            session.Verify(x => x.AbortTransactionAsync(cancellationToken), Times.Never);
        }

        [Fact]
        public async Task RollbackAsync_WhereDisposed_Throws()
        {
            var provider = new TransactionProvider(Mock.Of<IMongoClient>(), new MongoOptions());

            provider.GetType().GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, true);

            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                provider.RollbackAsync(CancellationToken.None));
        }

        [Fact]
        public void GetExecutionArguments_WhereSessionIsNotActive_ReturnsDatabase()
        {
            var client = Mock.Of<IMongoClient>();

            var provider = new TransactionProvider(client, new MongoOptions());

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, null);

            var (db, tx) = provider.GetExecutionArguments();
            db.Should().Be(client);
            tx.Should().BeNull();
        }

        [Fact]
        public void GetExecutionArguments_WhereSessionIsActive_ReturnsDatabaseAndSession()
        {
            var client = Mock.Of<IMongoClient>();
            var session = Mock.Of<IClientSessionHandle>();

            var provider = new TransactionProvider(client, new MongoOptions());

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session);

            var (db, tx) = provider.GetExecutionArguments();
            db.Should().Be(client);
            tx.Should().Be(session);
        }

        [Fact]
        public void GetExecutionArguments_WhereDisposed_Throws()
        {
            var provider = new TransactionProvider(Mock.Of<IMongoClient>(), new MongoOptions());

            provider.GetType().GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, true);

            Assert.Throws<ObjectDisposedException>(() => provider.GetExecutionArguments());
        }

        [Fact]
        public void Dispose_WhereNotAlreadyDisposed_CleansUpSession()
        {
            var client = Mock.Of<MongoClient>();
            var session = new Mock<IClientSessionHandle>();
            session.Setup(x => x.Dispose()).Verifiable();

            var provider = new TransactionProvider(client, new MongoOptions());

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);

            provider.Dispose();

            session.VerifyAll();
        }

        [Fact]
        public void Dispose_WhereAlreadyDisposed_DoesNothing()
        {
            var client = Mock.Of<MongoClient>();
            var session = new Mock<IClientSessionHandle>();

            var provider = new TransactionProvider(client, new MongoOptions());

            provider.GetType().GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, session.Object);
            provider.GetType().GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)?
                .SetValue(provider, true);

            provider.Dispose();

            session.VerifyAll();
            session.Verify(x => x.Dispose(), Times.Never);
        }
    }
}
