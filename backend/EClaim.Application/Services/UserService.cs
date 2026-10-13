using EClaim.Application.DTOs.Users;
using EClaim.Application.Interfaces;
using EClaim.Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace EClaim.Application.Services;

public class UserService : IUserService
{
    private IEClaimDbContext _db;

    public UserService(IEClaimDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserListDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _db.Users.Include(u => u.Role).OrderBy(u => u.FullName).ToListAsync(ct);
        return users.Select(u => u.ToAdminListDto()).ToList();
    }
}