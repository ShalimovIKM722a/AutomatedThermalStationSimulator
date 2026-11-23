using System.Net.NetworkInformation;

namespace ThermalStationScada.Web.Models;

public class UserRoleRow
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
