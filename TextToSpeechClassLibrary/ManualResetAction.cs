using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommandWorker
{
    public class ManualResetAction
    {
        public ManualResetEvent ManualResetEvent { get; set; }
        public Action Action { get; set; }

        public ManualResetAction(ManualResetEvent manualResetEvent, Action action)
        {
            this.ManualResetEvent = manualResetEvent;
            this.Action = action;
        }
    }
}
