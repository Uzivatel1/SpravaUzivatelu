using Microsoft.AspNetCore.Identity;

namespace SpravaUzivatelu.Models
{
    public class CzechIdentityErrorDescriberViewModel : IdentityErrorDescriber
    {
        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"Uživatelské jméno '{userName}' je již obsazené."
            };
        }
    }
}
