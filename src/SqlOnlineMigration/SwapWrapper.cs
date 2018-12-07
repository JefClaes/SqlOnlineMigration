using System;
using System.Threading.Tasks;

namespace SqlOnlineMigration
{
    public delegate Task SwapWrapper(Func<Task> swap);
}
