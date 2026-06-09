using EnergyAuditBot.Application.Interfaces;
using EnergyAuditBot.Domain.Entities;
using EnergyAuditBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyAuditBot.Infrastructure.Repositories;

public class HousingProfileRepository(AppDbContext context) : IHousingProfileRepository
{
    public async Task<HousingProfile?> GetByUserIdAsync(long userId) =>
        await context.HousingProfiles.Include(p => p.Appliances).FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task AddAsync(HousingProfile profile)
    {
        await context.HousingProfiles.AddAsync(profile);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(HousingProfile profile)
    {
        context.HousingProfiles.Update(profile);
        await context.SaveChangesAsync();
    }
}