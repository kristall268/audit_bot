using EnergyAuditBot.Application.Interfaces;
using EnergyAuditBot.Domain.Entities;
using EnergyAuditBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyAuditBot.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(long chatId) => 
        await context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.ChatId == chatId);

    public async Task<bool> ExistsAsync(long chatId) => 
        await context.Users.AnyAsync(u => u.ChatId == chatId);

    public async Task AddAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}