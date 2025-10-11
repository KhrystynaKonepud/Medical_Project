using System.Threading.Tasks;
using Medical_center.Models;
using Microsoft.AspNetCore.Identity;

namespace Medical_center.Validators
{
    /// <summary>
    /// Додає серверну перевірку: пароль не довший за 16 символів.
    /// </summary>
    public class MaxLengthPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        private const int MaxLen = 16;

        public Task<IdentityResult> ValidateAsync(
            UserManager<ApplicationUser> manager,
            ApplicationUser user,
            string password)
        {
            if (password is null)
            {
                return Task.FromResult(IdentityResult.Failed(
                    new IdentityError { Description = "Пароль є обовʼязковим." }));
            }

            if (password.Length > MaxLen)
            {
                return Task.FromResult(IdentityResult.Failed(
                    new IdentityError { Description = $"Пароль не може перевищувати {MaxLen} символів." }));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
