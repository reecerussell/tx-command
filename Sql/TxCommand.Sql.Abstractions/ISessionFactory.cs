namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used to create new instance of <see cref="ISession"/> for Sql.
    /// </summary>
    public interface ISessionFactory : ISessionFactory<ISession>
    {
    }
}
