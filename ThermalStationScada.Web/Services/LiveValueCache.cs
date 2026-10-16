using Models.Entities;
using System.Collections.Concurrent;

namespace ThermalStationScada.Web.Services;

public class LiveValuesCache
{
    private readonly ConcurrentDictionary<string, SensorReading> _values = new();

    public void Update(SensorReading reading)
    {
        _values[reading.Topic] = reading;
    }

    public List<SensorReading> GetAll()
    {
        return _values.Values.ToList();
    }
}