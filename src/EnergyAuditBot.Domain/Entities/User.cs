using EnergyAuditBot.Domain.Entities;

namespace EnergyAuditBot.Domain.Entities;

public class User
{
    public long ChatId { get; set; }
    public string? Username { get; set; }
    public bool IsOnboarded { get; set; }
    public DateTime CreatedAt { get; set; }

    // Навигационное свойство
    public HousingProfile? Profile { get; set; }
}