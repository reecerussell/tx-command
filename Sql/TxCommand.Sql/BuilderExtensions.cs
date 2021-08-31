using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;
using TxCommand.Abstractions;

namespace TxCommand
{
    public static class BuilderExtensions
    {
        public static ITxCommandBuilder AddSql(this ITxCommandBuilder builder)
        {
            builder.Services.TryAddTransient<ITransactionProvider<IDbConnection, IDbTransaction>, TransactionProvider>();
            builder.Services.TryAddTransient<ISession, Session>();
            builder.Services.TryAddSingleton<ISessionFactory, SessionFactory>();

            return builder;
        }
    }
}
