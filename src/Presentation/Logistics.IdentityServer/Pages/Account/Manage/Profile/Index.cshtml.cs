using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account.Manage.Profile;

public class ProfileModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserService _userService;

    public ProfileModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserService userService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
    }

    public string Username { get; set; }

    [TempData] public string StatusMessage { get; set; }

    [BindProperty] public InputModel Input { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return LocalRedirect("/Identity/Account/Login");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        await _userService.UpdateUserAsync(new UpdateUserParams(user.Id)
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            PhoneNumber = Input.PhoneNumber
        });

        //_signInManage
        //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        //if (Input.PhoneNumber != phoneNumber)
        //{
        //    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
        //    if (!setPhoneResult.Succeeded)
        //    {
        //        StatusMessage = "Unexpected error when trying to set phone number.";
        //        return RedirectToPage();
        //    }
        //}

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }

    private async Task LoadAsync(User user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        Username = userName;

        Input = new InputModel
        {
            PhoneNumber = phoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
