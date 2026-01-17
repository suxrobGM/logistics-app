using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account.AcceptInvitation;

[AllowAnonymous]
public class Index(
    SignInManager<User> signInManager,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<Index> logger)
    : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? TenantName { get; set; }
    public string? RoleDisplayName { get; set; }
    public string? ErrorMessage { get; set; }
    public bool UserExists { get; set; }
    public bool InvitationValid { get; set; }

    public async Task<IActionResult> OnGetAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            ErrorMessage = "Invalid invitation link.";
            return Page();
        }

        Token = token;
        Input.Token = token;

        var validation = await ValidateTokenAsync(token);

        if (validation is null || !validation.IsValid)
        {
            ErrorMessage = validation?.ErrorMessage ?? "Invalid or expired invitation.";
            return Page();
        }

        InvitationValid = true;
        Email = validation.Email;
        TenantName = validation.TenantName;
        RoleDisplayName = validation.RoleDisplayName;
        UserExists = validation.UserExists;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Re-validate token
        var validation = await ValidateTokenAsync(Input.Token);
        if (validation is null || !validation.IsValid)
        {
            ErrorMessage = validation?.ErrorMessage ?? "Invalid or expired invitation.";
            return Page();
        }

        Email = validation.Email;
        TenantName = validation.TenantName;
        RoleDisplayName = validation.RoleDisplayName;
        UserExists = validation.UserExists;
        InvitationValid = true;
        Token = Input.Token;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await AcceptInvitationAsync();

        if (result is null || !result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result?.ErrorMessage ?? "Failed to accept invitation.");
            return Page();
        }

        logger.LogInformation("User {Email} accepted invitation to {TenantName}", result.Email, result.TenantName);

        // Sign in the user
        var user = await signInManager.UserManager.FindByEmailAsync(result.Email);
        if (user is not null)
        {
            await signInManager.SignInAsync(user, false);
        }

        return RedirectToPage("/Account/AcceptInvitation/Success", new
        {
            tenantName = result.TenantName,
            role = result.RoleDisplayName
        });
    }

    private async Task<InvitationValidationResult?> ValidateTokenAsync(string token)
    {
        try
        {
            var apiUrl = configuration["Api:BaseUrl"] ?? "http://localhost:7000";
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{apiUrl}/invitations/validate/{token}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InvitationValidationResult>();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating invitation token");
        }

        return null;
    }

    private async Task<AcceptInvitationApiResult?> AcceptInvitationAsync()
    {
        try
        {
            var apiUrl = configuration["Api:BaseUrl"] ?? "http://localhost:7000";
            var client = httpClientFactory.CreateClient();

            var request = new
            {
                token = Input.Token,
                firstName = Input.FirstName,
                lastName = Input.LastName,
                password = Input.Password
            };

            var response = await client.PostAsJsonAsync($"{apiUrl}/invitations/accept", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AcceptInvitationResult>();
                return new AcceptInvitationApiResult
                {
                    IsSuccess = true,
                    UserId = result?.UserId ?? Guid.Empty,
                    Email = result?.Email ?? string.Empty,
                    TenantName = result?.TenantName ?? string.Empty,
                    RoleDisplayName = result?.RoleDisplayName ?? string.Empty
                };
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return new AcceptInvitationApiResult
            {
                IsSuccess = false,
                ErrorMessage = error.Error
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error accepting invitation");
            return new AcceptInvitationApiResult
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while accepting the invitation."
            };
        }
    }

    private class AcceptInvitationApiResult
    {
        public bool IsSuccess { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string RoleDisplayName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
