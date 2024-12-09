namespace Bureau.UI.Events
{
    public class EventMessenger<T> : IEventMessenger<T>
    {

        public event Action<T>? OnEventReceived;

        public void PublishEvent(T e)
        {
            OnEventReceived?.Invoke(e);
        }
    }
}
