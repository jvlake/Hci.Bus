using System;
using System.Threading;

namespace Hci.Bus
{
    internal sealed class Publish
    {
        public object Payload { get; }
        public CancellationToken CancellationToken { get; }
        public Action<bool> OnSendComplete { get; }

        public Publish(object payload, CancellationToken cancellationToken)
            : this(payload, cancellationToken, success => { })
        {

        }

        public Publish(object payload, CancellationToken cancellationToken, Action<bool> onSendComplete)
        {
            Payload = payload;
            CancellationToken = cancellationToken;
            OnSendComplete = onSendComplete;
        }
    }
}
