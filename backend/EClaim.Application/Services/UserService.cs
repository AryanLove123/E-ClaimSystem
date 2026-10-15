using EClaim.Application.DTOs.Users;
using EClaim.Application.Interfaces;
using EClaim.Application.Mappings;
using EClaim.Domain.Entities;
using EClaim.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EClaim.Application.Services;

public class UserService : IUserService
{
    private IEClaimDbContext _db;
    private IAuditService _auditService; 

    public UserService(IEClaimDbContext db, IAuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId, ct) 
            ?? throw new NotFoundException("User", userId);
        return user.ToProfileDto();
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User", userId);

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync(ct);

        await _auditService.LogAsync(userId, "ProfileUpdated", "User", userId.ToString(), ct: ct);

        return user.ToProfileDto();
    }

    public async Task<List<UserListDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _db.Users.Include(u => u.Role).OrderBy(u => u.FullName).ToListAsync(ct);
        return users.Select(u => u.ToAdminListDto()).ToList();
    }

    public async Task SetUserActiveStatusAsync(int userId, bool isActive, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("User", userId);

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _auditService.LogAsync(userId, isActive ? "UserActivated" : "UserDeactivated", "User", userId.ToString(), ct: ct);
    }
}