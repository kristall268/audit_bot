using EnergyAuditBot.Domain.Entities;

namespace EnergyAuditBot.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long chatId);
    Task<bool> ExistsAsync(long chatId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}