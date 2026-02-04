namespace GGJ.Mapping
{
    public interface ITriggerTarget
    {
        public void Trigger(TriggerData data);
    }


    public struct TriggerData
    {
        public bool Active;
    }
}
