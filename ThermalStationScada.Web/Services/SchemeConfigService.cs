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
            X = 620, Y = 205, Max = 1000
        },
        new SensorView
        {
            Name = "Температура зовні",
            Topic = "thermalStation/1/sensors/outside/thermal",
            X = 90, Y = 160, Max = 40
        },
        new SensorView
        {
            Name = "Рівень води",
            Topic = "thermalStation/1/sensors/boiler/waterLevel",
            X = 95, Y = 260, Max = 100
        },
        new SensorView
        {
            Name = "Тиск пари",
            Topic = "thermalStation/1/sensors/outside/steam",
            X = 720, Y = 305, Max = 100
        },
        new SensorView
        {
            Name = "Оберти турбіни",
            Topic = "thermalStation/1/sensors/turbine/rotation",
            X = 1045, Y = 245, Max = 10000
        },
        new SensorView
        {
            Name = "Вхідний струм",
            Topic = "thermalStation/1/sensors/inside/current",
            X = 180, Y = 490, Max = 100
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
            X = 1200 , Y = 300, Max = 1000
        },
        new SensorView
        {
            Name = "Рівень заряду акумулятора",
            Topic = "thermalStation/1/sensors/battery/soc",
            X = 1250, Y = 350, Max = 100
        },
        new SensorView
        {
            Name = "Витрата води",
            Topic = "thermalStation/1/sensors/boiler/outputFlow",
            X = 425, Y = 500, Max = 100
        }
    };

    public IReadOnlyList<SensorView> GetSensors()
    {
        return _sensors;
    }
}
