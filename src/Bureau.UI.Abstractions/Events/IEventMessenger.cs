namespace Bureau.UI.Events
{
    public interface IEventMessenger<T>
    {
        public event Action<T>? OnEventReceived;

        public void PublishEvent(T e);
    }
}
