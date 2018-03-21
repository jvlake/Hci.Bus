using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hci.Bus
{
    internal sealed class Subscribe
    {
        public Guid Id { get; }
        public Func<object, CancellationToken, Task> Handler { get; }

        private Subscribe(Func<object, CancellationToken, Task> handler)
        {
            Id = Guid.NewGuid();
            Handler = handler;
        }

        public static Subscribe Create<TMessage>(Func<TMessage, CancellationToken, Task> handler)
        {
            async Task HandlerWithCheck(object message, CancellationToken cancellationToken)
            {
                if (message.GetType().IsAssignableFrom(typeof(TMessage)))
                {
                    await handler.Invoke((TMessage) message, cancellationToken);
                }
            }

            return new Subscribe(HandlerWithCheck);
        }
    }
}
