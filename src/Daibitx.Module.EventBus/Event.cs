using Daibitx.Module.EventBus.Imp;

namespace Daibitx.Module.EventBus
{
    public class Event
    {
        private static readonly Lazy<EventBusFactory> _factoryInstance = new Lazy<EventBusFactory>();

        /// <summary>
        /// Global EventBus Instance
        /// </summary>
        public static EventBusFactory CrossEventInstance => _factoryInstance.Value;
    }
}
