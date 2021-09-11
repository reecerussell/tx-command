namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used to instantiate new instances of <see cref="ISession"/>.
    /// </summary>
    public interface ISessionFactory : ISessionFactory<ISession>
    {
    }
}
