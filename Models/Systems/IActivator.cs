namespace Models.Systems
{
    public interface IActuator
    {
        bool IsOn { get; }
        void TurnOn();
        void TurnOff();
    }
}
