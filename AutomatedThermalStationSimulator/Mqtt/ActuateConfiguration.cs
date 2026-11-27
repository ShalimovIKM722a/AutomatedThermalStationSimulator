using AutomatedThermalStationSimulator.Systems;
using Models;

namespace AutomatedThermalStationSimulator.Mqtt
{
    public class ActuatorConfiguration
    {
        public string Name { get; set; }
        public IActuator Actuator { get; set; }
        public Sensor ControlSensor { get; set; }
        public string Topic { get; set; }
        // Рівень, при якому механізм вмикається, і рівень, при якому вимикається.
        // Різниця між ними (гістерезис) не дає механізму перемикатися на межі.
        public double OnLevel { get; set; }
        public double OffLevel { get; set; }
        // true - вмикати, коли значення ВИЩЕ за OnLevel (вентиляція при спеці)
        // false - вмикати, коли значення НИЖЧЕ за OnLevel (полив при сухості)
        public bool OnWhenAbove { get; set; }
        // Останній стан, який було опубліковано в MQTT
        public bool? PublishedState { get; set; }
        public ActuatorConfiguration(
        string name,
        IActuator actuator,
        Sensor controlSensor,
        string topic,
        double onLevel,
        double offLevel,
        bool onWhenAbove)
        {
            Name = name;
            Actuator = actuator;
            ControlSensor = controlSensor;
            Topic = topic;
            OnLevel = onLevel;
            OffLevel = offLevel;
            OnWhenAbove = onWhenAbove;
        }
        // Повертає причину, якщо стан механізму змінився, інакше null
        public string? Evaluate()
        {
            double value = ControlSensor.Value;
            if (!Actuator.IsOn && ShouldTurnOn(value))
            {
                Actuator.TurnOn();
                return $"{ControlSensor.Name} = {Math.Round(value, 1)}, " +
                $"{(OnWhenAbove ? "above" : "below")} {OnLevel}";
            }
            if (Actuator.IsOn && ShouldTurnOff(value))
            {
                Actuator.TurnOff();
                return $"{ControlSensor.Name} = {Math.Round(value, 1)}, " +
                $"back to normal ({OffLevel})";
            }
            return null;
        }
        private bool ShouldTurnOn(double value)
        {
            if (OnWhenAbove)
            {
                return value >= OnLevel;
            }
            return value <= OnLevel;
        }
        private bool ShouldTurnOff(double value)
        {
            if (OnWhenAbove)
            {
                return value <= OffLevel;
            }
            return value >= OffLevel;
        }
    }
}
