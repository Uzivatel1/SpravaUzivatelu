using System.ComponentModel.DataAnnotations;

namespace SpravaUzivatelu.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vyplňte uživatelské jméno")]
        [Display(Name = "Uživatelské jméno")]
        public string Login { get; set; } = "";

        [Required(ErrorMessage = "Vyplňte heslo")]
        [StringLength(100, ErrorMessage = "{0} musí mít délku alespoň {2} a nejvíc {1} znaků.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Heslo")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Vyplňte heslo")]
        [DataType(DataType.Password)]
        [Display(Name = "Potvrzení hesla")]
        [Compare(nameof(Password), ErrorMessage = "Zadaná hesla se musí shodovat.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
