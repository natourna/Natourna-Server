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

        public async Task<(List<UserEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(term) || u.PhoneNumber.Contains(term));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<UserEntity?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserEntity> CreateAsync(UserEntity user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await _context.Users
                .Include(u => u.Role)
                .FirstAsync(u => u.Id == user.Id);
        }

        public async Task<UserEntity?> UpdateAsync(int id, string email, string phoneNumber, int roleId, bool isActive, string? passwordHash)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            existingUser.Email = email;
            existingUser.PhoneNumber = phoneNumber;
            existingUser.RoleId = roleId;
            existingUser.IsActive = isActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(passwordHash))
            {
                existingUser.Password = passwordHash;
            }

            await _context.SaveChangesAsync();

            return await _context.Users
                .Include(u => u.Role)
                .FirstAsync(u => u.Id == id);
        }

        public async Task<UserEntity?> SetActiveAsync(int id, bool isActive)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await _context.Users
                .Include(u => u.Role)
                .FirstAsync(u => u.Id == id);
        }

        public async Task UpdatePasswordHashAsync(int id, string passwordHash)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return;

            user.Password = passwordHash;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
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
