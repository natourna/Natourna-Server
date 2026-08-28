using NatournaServer.Data;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class UserContextManager : IUserContextManager
    {
        private readonly NatournaServerContext _context;

        public UserContextManager(NatournaServerContext context)
        {
            _context = context;
        }

        public async Task<List<UserEntity>> GetAllAsync()
        {
            return await _context.Users
                .ToListAsync();
        }

        public async Task<UserEntity?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserEntity> CreateAsync(UserEntity user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<UserEntity?> UpdateAsync(int id, UserEntity user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Only update password if it's provided and different
            if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
            {
                // In production, you should hash the password here
                existingUser.Password = user.Password;
            }

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
