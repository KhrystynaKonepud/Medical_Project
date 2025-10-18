namespace Medical_center.Validators
{
    using Microsoft.AspNetCore.Identity;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MaxLengthPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        private const int MaxLength = 16;

        public Task<IdentityResult> ValidateAsync(
            UserManager<TUser> manager, TUser user, string password)
        {
            var errors = new List<IdentityError>();
            if (!string.IsNullOrEmpty(password) && password.Length > MaxLength)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooLong",
                    Description = $"Password must be at most {MaxLength} characters."
                });
            }
            return Task.FromResult(
                errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
        }
    }
}
