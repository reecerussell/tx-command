using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Data;
using TxCommand.Abstractions;

namespace TxCommand
{
    public static class BuilderExtensions
    {
        public static ITxCommandBuilder AddSql(this ITxCommandBuilder builder)
            => AddSql(builder, sqlOptions =>
            {
                sqlOptions.IsolationLevel = IsolationLevel.ReadUncommitted;
            });

        public static ITxCommandBuilder AddSql(this ITxCommandBuilder builder, Action<SqlOptions> options)
        {
            var sqlOptions = new SqlOptions();
            options?.Invoke(sqlOptions);

            builder.Services.AddSingleton(sqlOptions);
            builder.Services.TryAddTransient<ITransactionProvider<IDbConnection, IDbTransaction>, TransactionProvider>();
            builder.Services.TryAddTransient<ISession, Session>();
            builder.Services.TryAddSingleton<ISessionFactory, SessionFactory>();

            return builder;
        }
    }
}
