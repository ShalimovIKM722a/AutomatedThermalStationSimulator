using Models;
using Models.Entities;

namespace ThermalStationScada.Web.Services;

public class SchemeConfigService
{
    private readonly List<SensorView> _sensors = new List<SensorView>()
{
    new SensorView
    {
        Name = "Температура котла",
        Topic = "thermalStation/1/sensors/boiler/temperature",
        X = 540, Y = 145, Max = 1000
    },
    new SensorView
    {
        Name = "Температура зовні",
        Topic = "thermalStation/1/sensors/outside/thermal",
        X = 35, Y = 95, Max = 40
    },
    new SensorView
    {
        Name = "Рівень води",
        Topic = "thermalStation/1/sensors/boiler/waterLevel",
        X = 35, Y = 205, Max = 100
    },
    new SensorView
    {
        Name = "Тиск пари",
        Topic = "thermalStation/1/sensors/outside/steam",
        X = 470, Y = 300, Max = 100
    },
    new SensorView
    {
        Name = "Оберти турбіни",
        Topic = "thermalStation/1/sensors/turbine/rotation",
        X = 820, Y = 160, Max = 10000
    },
    new SensorView
    {
        Name = "Вхідний струм",
        Topic = "thermalStation/1/sensors/inside/current",
        X = 180, Y = 370, Max = 100
    },
    new SensorView
    {
        Name = "Вихідний струм",
        Topic = "thermalStation/1/sensors/inside/current",
        X = 1250, Y = 450, Max = 100
    },
    new SensorView
    {
        Name = "Вихідна напруга",
        Topic = "thermalStation/1/sensors/output/voltagOutput",
        X = 1180, Y = 245, Max = 1000
    },
    new SensorView
    {
        Name = "Рівень заряду акумулятора",
        Topic = "thermalStation/1/sensors/battery/soc",
        X = 1180, Y = 345, Max = 100
    },
    new SensorView
    {
        Name = "Витрата води",
        Topic = "thermalStation/1/sensors/boiler/outputFlow",
        X = 430, Y = 480, Max = 100
    }
};

    private readonly List<ActuatorView> _actuators = new List<ActuatorView>
    {
        new ActuatorView
    {
        Name = "Паливне горіння",
        Topic = "thermalStation/1/actuators/fuelCombustion/state",
        Kind = "fire",
        X = 330,
        Y = 250
    },
        new ActuatorView
    {
        Name = "Клапан подачі води",
        Topic = "thermalStation/1/actuators/waterFlowValve/state",
        Kind = "water",
        X = 220,
        Y = 420
    }
    };
    public IReadOnlyList<ActuatorView> GetActuators()
    {
        return _actuators;
    }

    public IReadOnlyList<SensorView> GetSensors()
    {
        return _sensors;
    }

    public bool UpdateMax(string topic, int max)
    {
        foreach (SensorView sensor in _sensors)
        {
            if (sensor.Topic == topic)
            {
                sensor.Max = max;
                return true;
            }
        }
        return false;
    }

}
