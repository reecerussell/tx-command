using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using System;
using TxCommand.Abstractions;

namespace TxCommand
{
    public static class BuilderExtensions
    {
        /// <summary>
        /// Adds core services used to transactionally execute Mongo commands.
        /// A IMongoClient singleton is expected to have been setup.
        /// </summary>
        public static ITxCommandBuilder AddMongo(this ITxCommandBuilder builder)
            => AddMongo(builder, options => { });

        /// <summary>
        /// Adds core services used to transactionally execute Mongo commands.
        /// A IMongoClient singleton is expected to have been setup.
        /// </summary>
        public static ITxCommandBuilder AddMongo(this ITxCommandBuilder builder, Action<MongoOptions> options)
        {
            var mongoOptions = new MongoOptions();
            options?.Invoke(mongoOptions);

            builder.Services.AddSingleton(mongoOptions);
            builder.Services.TryAddTransient<ITransactionProvider<IMongoClient, IClientSessionHandle>, TransactionProvider>();
            builder.Services.TryAddTransient<ISession, Session>();
            builder.Services.TryAddSingleton<ISessionFactory, SessionFactory>();

            return builder;
        }
    }
}
