using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account.Manage.Profile;

public class ProfileModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IUserService userService) : PageModel
{
    public string? Username { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return LocalRedirect("/Identity/Account/Login");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        await userService.UpdateUserAsync(new UpdateUserParams(user.Id)
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            PhoneNumber = Input.PhoneNumber
        });

        //_signInManage
        //var phoneNumber = await userManager.GetPhoneNumberAsync(user);
        //if (Input.PhoneNumber != phoneNumber)
        //{
        //    var setPhoneResult = await userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
        //    if (!setPhoneResult.Succeeded)
        //    {
        //        StatusMessage = "Unexpected error when trying to set phone number.";
        //        return RedirectToPage();
        //    }
        //}

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }

    private async Task LoadAsync(User user)
    {
        var userName = await userManager.GetUserNameAsync(user);
        var phoneNumber = await userManager.GetPhoneNumberAsync(user);
        Username = userName;

        Input = new InputModel
        {
            PhoneNumber = phoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
