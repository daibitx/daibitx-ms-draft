namespace Daibitx.Module.EventBus.Abstraction
{
    /// <summary>
    /// 事件总线
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventBus
    {
        Task Publish(EventMessage data);
        Task Publish(string topic, EventMessage data);
        Guid Subscriber(Action<EventMessage> action);
        Guid Subscriber(string topic, Action<EventMessage> action);
        void UnSubscriber(Guid id);
        void UnSubscriber(string topic, Guid id);
    }
}
