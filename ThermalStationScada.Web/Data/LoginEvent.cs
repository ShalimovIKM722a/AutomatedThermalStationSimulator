namespace ThermalStationScada.Web.Data
{
    public class LoginEvent
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool Succeeded { get; set; }
        public string Result { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTimeOffset TimeUtc { get; set; }
    }
}
