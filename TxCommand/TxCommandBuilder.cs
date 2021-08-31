using Microsoft.Extensions.DependencyInjection;
using TxCommand.Abstractions;

namespace TxCommand
{
    internal class TxCommandBuilder : ITxCommandBuilder 
    {
        public IServiceCollection Services { get; }

        public TxCommandBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
