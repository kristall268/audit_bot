using EnergyAuditBot.Domain.Enums;

namespace EnergyAuditBot.Domain.Entities;

public class HousingProfile
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public HousingType HousingType { get; set; }
    public float AreaSqM { get; set; }
    public string RegionCode { get; set; } = string.Empty;
    public decimal ElectricityRate { get; set; }
    public int ResidentsCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Навигационные свойства
    public User User { get; set; } = null!;
    public ICollection<Appliance> Appliances { get; set; } = [];
}