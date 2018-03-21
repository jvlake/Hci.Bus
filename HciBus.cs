using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Hci.Bus
{
    public interface IHciBus
    {
        Task Publish<T>(T t);
        Guid Subscribe<T>(Action<T> t);
        void Unsubscribe(Guid id);
    }

    public class HciBus : IHciBus
    {
        public Task Publish<T>(T t)
        {
            return HciBusInstance.Instance.Value.Publish<T>(t);
        }

        public Guid Subscribe<T>(Action<T> handler)
        {
            return HciBusInstance.Instance.Value.Subscribe<T>(handler);
        }

        public void Unsubscribe(Guid id)
        {
            HciBusInstance.Instance.Value.Unsubscribe(id);
        }
    }

    internal class HciBusInstance 
    {
        private readonly ConcurrentQueue<Subscribe> _subscriptionRequests = new ConcurrentQueue<Subscribe>();
        private readonly ConcurrentQueue<Guid> _unsubscribeRequests = new ConcurrentQueue<Guid>();
        private readonly ActionBlock<Publish> _messageProcessor;

        public static Lazy<HciBus> Instance = new Lazy<HciBus>(() => new HciBus());

        private HciBusInstance()
        {
            var subscriptions = new List<Subscribe>();

            _messageProcessor = new ActionBlock<Publish>(async request =>
            {
                while (_unsubscribeRequests.TryDequeue(out var subscriptionId))
                {
                    var id = subscriptionId;
                    subscriptions.RemoveAll(s => s.Id == id);
                }

                while (_subscriptionRequests.TryDequeue(out var newSubscription))
                {
                    subscriptions.Add(newSubscription);
                }

                var result = true;
                
                foreach (var subscription in subscriptions)
                {
                    if (request.CancellationToken.IsCancellationRequested)
                    {
                        result = false;
                        break;
                    }
                    
                    try
                    {
                        await subscription.Handler.Invoke(request.Payload, request.CancellationToken);
                    }
                    catch (Exception)
                    {                        
                        result = false;
                    }
                }
                request.OnSendComplete(result);
            });
        }

        internal Task Publish<T>(T message)
        {
            var tcs = new TaskCompletionSource<bool>();
            _messageProcessor.Post(new Publish(message, CancellationToken.None, result => tcs.SetResult(result)));
            return tcs.Task;
        }

        internal Guid Subscribe<T>(Action<T> handler)
        {
            var subscription = Bus.Subscribe.Create<T>((message, cancellationToken) =>
            {
                handler.Invoke(message);
                return Task.FromResult(0);
            });

            _subscriptionRequests.Enqueue(subscription);
            
            return subscription.Id;
        }
        
        internal void Unsubscribe(Guid subscriptionId)
        {
            _unsubscribeRequests.Enqueue(subscriptionId);
        }
    }
}
