using System.Data;

namespace TxCommand
{
    /// <summary>
    /// A set of options used to configure the Session.
    /// </summary>
    public class SqlOptions
    {
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadUncommitted;
    }
}
