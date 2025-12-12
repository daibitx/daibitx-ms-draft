namespace Daibitx.Module.EventBus
{
    public class EventMessage
    {
        public string TypeName { get; set; }
        public object Payload { get; set; }
        /// <summary>
        /// Transform layer
        /// </summary>
        public EventMessage(object payload)
        {
            TypeName = payload.GetType().Name!;
            Payload = payload;
        }
    }

   
}
