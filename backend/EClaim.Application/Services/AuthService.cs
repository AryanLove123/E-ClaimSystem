using EClaim.Application.DTOs.Auth;
using EClaim.Application.Interfaces;
using EClaim.Domain.Entities;
using EClaim.Domain.Enums;
using EClaim.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EClaim.Application.Services;

public class AuthService : IAuthService
{
    private IEClaimDbContext _db;
    private IPasswordHasher _passwordHasher;

    private IJwtTokenGenerator _jwtTokenGenerator;
    private IEmailService _emailService;


    public AuthService(IEClaimDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IEmailService emailService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
    }

    public async Task<int> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var context = _db;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await context.Users.AnyAsync(u => u.Email == normalizedEmail,ct);

        if (exists)
        {
            throw new ValidationException("An account with this email already exist");
        }

        var claimantRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleType.Claimant, ct) ?? throw new NotFoundException("Role", RoleType.Claimant);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber,
            RoleId = claimantRole.Id,
            EmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        var verifyLink = $"https://localhost:4200/verify-email?email={Uri.EscapeDataString(user.Email)}&token={user.EmailVerificationToken}";
        await _emailService.SendEmailAsync(
            user.Email,
            "Verify your E-Claim account",
            $"Hi {user.FullName}, \n\nPlease verify email by visiting:\n{verifyLink}\n\nThis link expires in 24 hours",
            ct
        );

        return user.Id;
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var context = _db;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct) ?? throw new NotFoundException("User", request.Email);

        if(user.EmailVerified) return;

        if (user.EmailVerificationToken != request.Token ||
            user.EmailVerificationTokenExpiresAt is null ||
            user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationException("The verification link is invalid or has expired");
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;
        await context.SaveChangesAsync(ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var context = _db;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new ValidationException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenAccessException("This account has been deactivated.");

        if (!user.EmailVerified)
            throw new ValidationException("Please verify your email before logging in.");

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.Name.ToString()
        };
    }
}
