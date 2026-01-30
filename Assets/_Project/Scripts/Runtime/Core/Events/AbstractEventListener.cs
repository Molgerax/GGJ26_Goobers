namespace GGJ.Core.Events
{
    public interface IEventListener<T>
    {
        void OnEventRaised(T data);
    }
}