using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Medical_center.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medical_center.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ProviderDisplayName { get; set; } = "";
        public string ReturnUrl { get; set; } = "/";

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";
        }

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // 1) Якщо цей зовнішній логін уже прив’язаний – просто увійти
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
                return LocalRedirect(returnUrl);

            // 2) Отримати email із клейму
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                Input.Email = email;

                // 3) Якщо користувач з таким email вже існує – прив’язати до нього зовнішній логін і увійти
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    // Якщо вже прив’язано – теж нормально, просто увійдемо
                    if (addLoginResult.Succeeded || addLoginResult.Errors.Any(e => e.Code == "LoginAlreadyAssociated"))
                    {
                        // (за бажанням) позначити email підтвердженим, бо Google дає підтверджений email
                        if (!existingUser.EmailConfirmed)
                        {
                            existingUser.EmailConfirmed = true;
                            await _userManager.UpdateAsync(existingUser);
                        }

                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                    foreach (var e in addLoginResult.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                }

                // 4) Якщо користувача ще немає – створити нового і прив’язати логін
                ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
                return Page();
            }

            // Якщо email не прийшов – попросимо ввести (рідкісний випадок)
            ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (!ModelState.IsValid)
                return Page();

            // Якщо користувач уже існує з таким email – прив’язуємо і входимо
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (addLoginResult.Succeeded)
                {
                    if (!existingUser.EmailConfirmed)
                    {
                        existingUser.EmailConfirmed = true;
                        await _userManager.UpdateAsync(existingUser);
                    }
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var e in addLoginResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
                return Page();
            }

            // Інакше створюємо нового
            var user = new ApplicationUser();
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            user.Email = Input.Email;
            user.EmailConfirmed = true; // бо Google

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var e in addLoginResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
            }
            else
            {
                foreach (var e in createResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
            return Page();
        }
    }
}
