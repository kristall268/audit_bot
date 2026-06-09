using EnergyAuditBot.Domain.Enums;

namespace EnergyAuditBot.Domain.Entities;

public class Appliance
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public float PowerKw { get; set; }
    public ApplianceCategory Category { get; set; }

    // Навигационное свойство
    public HousingProfile Profile { get; set; } = null!;
}