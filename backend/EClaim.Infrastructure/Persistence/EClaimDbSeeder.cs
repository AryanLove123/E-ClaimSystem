using EClaim.Domain.Entities;
using EClaim.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace EClaim.Infrastructure.Persistence;

public static class EClaimDbSeeder
{
    public const string DemoPassword = "Passw0rd!";

    public static async Task SeedAsync(EClaimDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Name = RoleType.Claimant, Description = "Submits and tracks insurance claims." },
                new Role { Name = RoleType.Adjuster, Description = "Reviews claims and assesses/adjusts amounts." },
                new Role { Name = RoleType.Approver, Description = "Approves or rejects claims after adjuster review." },
                new Role { Name = RoleType.Admin, Description = "Manages users, workflows and views system-wide reports." }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var roles = await context.Roles.ToDictionaryAsync(r => r.Name, r => r.Id);
            string Hash(string p) => BCrypt.Net.BCrypt.HashPassword(p);

            context.Users.AddRange(
                new User { FullName = "Alice Claimant", Email = "claimant@demo.com", PasswordHash = Hash(DemoPassword), RoleId = roles[RoleType.Claimant], EmailVerified = true, PhoneNumber = "+911234567890" },
                new User { FullName = "Bob Adjuster", Email = "adjuster@demo.com", PasswordHash = Hash(DemoPassword), RoleId = roles[RoleType.Adjuster], EmailVerified = true, PhoneNumber = "+911234567891" },
                new User { FullName = "Carol Approver", Email = "approver@demo.com", PasswordHash = Hash(DemoPassword), RoleId = roles[RoleType.Approver], EmailVerified = true, PhoneNumber = "+911234567892" },
                new User { FullName = "System Admin", Email = "admin@demo.com", PasswordHash = Hash(DemoPassword), RoleId = roles[RoleType.Admin], EmailVerified = true, PhoneNumber = "+911234567893" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Workflows.AnyAsync())
        {
            // Scenario 1: Low-value vehicle claim -> Adjuster -> Approver
            var vehicleStandard = new Workflow
            {
                Name = "Vehicle Standard",
                Description = "Standard 2-step workflow for vehicle claims up to 50,000.",
                ClaimType = ClaimType.Vehicle,
                MaxAmount = 50_000m,
                Priority = 10,
                IsActive = true,
                Steps =
                {
                    new WorkflowStep { StepOrder = 1, Name = "Adjuster Review", ResponsibleRole = ClaimStepRole.Adjuster },
                    new WorkflowStep { StepOrder = 2, Name = "Approver Approval", ResponsibleRole = ClaimStepRole.Approver }
                }
            };

            // Scenario 2: High-value vehicle claim -> Adjuster -> Senior Adjuster -> Approver
            var vehicleHighValue = new Workflow
            {
                Name = "Vehicle High Value",
                Description = "3-step workflow for vehicle claims over 50,000.",
                ClaimType = ClaimType.Vehicle,
                MinAmount = 50_000m,
                Priority = 20,
                IsActive = true,
                Steps =
                {
                    new WorkflowStep { StepOrder = 1, Name = "Adjuster Review", ResponsibleRole = ClaimStepRole.Adjuster },
                    new WorkflowStep { StepOrder = 2, Name = "Senior Adjuster Review", ResponsibleRole = ClaimStepRole.SeniorAdjuster },
                    new WorkflowStep { StepOrder = 3, Name = "Approver Approval", ResponsibleRole = ClaimStepRole.Approver }
                }
            };

            // Scenario: Critical severity (any type) -> 4-step escalation
            var criticalClaim = new Workflow
            {
                Name = "Critical Claim",
                Description = "4-step escalation workflow for any claim marked Critical severity.",
                Severity = ClaimSeverity.Critical,
                Priority = 100, // highest priority: overrides type-specific workflows when severity is Critical
                IsActive = true,
                Steps =
                {
                    new WorkflowStep { StepOrder = 1, Name = "Adjuster Review", ResponsibleRole = ClaimStepRole.Adjuster },
                    new WorkflowStep { StepOrder = 2, Name = "Senior Adjuster Review", ResponsibleRole = ClaimStepRole.SeniorAdjuster },
                    new WorkflowStep { StepOrder = 3, Name = "Approver Approval", ResponsibleRole = ClaimStepRole.Approver },
                    new WorkflowStep { StepOrder = 4, Name = "Senior Approver Approval", ResponsibleRole = ClaimStepRole.SeniorApprover }
                }
            };

            // Generic fallback for Health/Property/Travel claims not otherwise matched
            var generalStandard = new Workflow
            {
                Name = "General Standard",
                Description = "Default 2-step workflow for Health/Property/Travel claims up to 200,000.",
                MaxAmount = 200_000m,
                Priority = 5,
                IsActive = true,
                Steps =
                {
                    new WorkflowStep { StepOrder = 1, Name = "Adjuster Review", ResponsibleRole = ClaimStepRole.Adjuster },
                    new WorkflowStep { StepOrder = 2, Name = "Approver Approval", ResponsibleRole = ClaimStepRole.Approver }
                }
            };

            var generalHighValue = new Workflow
            {
                Name = "General High Value",
                Description = "3-step workflow for Health/Property/Travel claims over 200,000.",
                MinAmount = 200_000m,
                Priority = 15,
                IsActive = true,
                Steps =
                {
                    new WorkflowStep { StepOrder = 1, Name = "Adjuster Review", ResponsibleRole = ClaimStepRole.Adjuster },
                    new WorkflowStep { StepOrder = 2, Name = "Senior Adjuster Review", ResponsibleRole = ClaimStepRole.SeniorAdjuster },
                    new WorkflowStep { StepOrder = 3, Name = "Approver Approval", ResponsibleRole = ClaimStepRole.Approver }
                }
            };

            context.Workflows.AddRange(vehicleStandard, vehicleHighValue, criticalClaim, generalStandard, generalHighValue);
            await context.SaveChangesAsync();
        }
    }
}
