using EnergyAuditBot.Domain.Entities;

namespace EnergyAuditBot.Application.Interfaces;

public interface IHousingProfileRepository
{
    Task<HousingProfile?> GetByUserIdAsync(long userId);
    Task AddAsync(HousingProfile profile);
    Task UpdateAsync(HousingProfile profile);
}