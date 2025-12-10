using Daibitx.Extension.Modularize.EventBus.Abstraction;
using System.Collections.Concurrent;

namespace Daibitx.Extension.Modularize.EventBus.Imp
{
    /// <summary>
    /// 事件工厂，所有事件通过 EventMessage 传递，自动桥接类型转换
    /// </summary>
    public class CrossEventBusFactory : ICrossEventBusFactory
    {
        private readonly ICrossEventBus _eventBus;
        private readonly ConcurrentDictionary<Delegate, (Guid Id, Action<EventMessage> Wrapper)> _actionMappings = new();
        private readonly ConcurrentDictionary<(Type source, Type target), List<(Func<object, object> getter, Action<object, object> setter)>> _compiledMappings = new();
        public CrossEventBusFactory()
        {
            _eventBus = new CrossEventBus();
        }

        public async Task PublishAsync<T>(T data) where T : new()
        {
            if (data == null) return;
            var message = new EventMessage(data);
            await _eventBus.Publish(message);
        }

        public async Task PublishAsync<T>(string topic, T data) where T : new()
        {
            if (data == null) return;
            var message = new EventMessage(data);
            await _eventBus.Publish(topic, message);
        }

        public Guid Subscriber<T>(Action<T> action) where T : new()
        {
            Action<EventMessage> wrapper = CreateWrapper<T>(action);
            var id = _eventBus.Subscriber(wrapper);
            _actionMappings.TryAdd(action, (id, wrapper));
            return id;
        }

        public Guid Subscriber<T>(string topic, Action<T> action) where T : new()
        {
            Action<EventMessage> wrapper = CreateWrapper<T>(action);
            var id = _eventBus.Subscriber(topic, wrapper);
            _actionMappings.TryAdd(action, (id, wrapper));
            return id;
        }

        public void UnSubscriber<T>(Action<T> action) where T : new()
        {
            if (_actionMappings.TryRemove(action, out var mapping))
            {
                _eventBus.UnSubscriber(mapping.Id);
            }
        }

        public void UnSubscriber<T>(string topic, Action<T> action) where T : new()
        {
            if (_actionMappings.TryRemove(action, out var mapping))
            {
                _eventBus.UnSubscriber(topic, mapping.Id);
            }
        }

        private Action<EventMessage> CreateWrapper<T>(Action<T> action) where T : new()
        {
            return msg =>
            {
                if (msg.TypeName == typeof(T).Name)
                {
                    try
                    {
                        var sourceObj = msg.Payload;
                        var mapped = EventMsgMapper.Map<T>(sourceObj);
                        action(mapped);
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (msg.Payload is T typed)
                {
                    action(typed);
                }
            };
        }




    }

}
