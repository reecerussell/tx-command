using System;

namespace TxCommand.Abstractions.Exceptions
{
    public class TransactionNotStartedException : InvalidOperationException
    {
        public TransactionNotStartedException(string methodName)
            : base($"Cannot call {methodName} as a transaction has either not started, or been completed.")
        {
        }
    }
}
