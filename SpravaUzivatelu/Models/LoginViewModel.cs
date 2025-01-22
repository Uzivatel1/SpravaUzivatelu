using System.ComponentModel.DataAnnotations;

namespace SpravaUzivatelu.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vyplňte uživatelské jméno")]
        [Display(Name = "Uživatelské jméno")]
        public string Login { get; set; } = "";

        [Required(ErrorMessage = "Vyplňte heslo")]
        [DataType(DataType.Password)]
        [Display(Name = "Heslo")]
        public string Password { get; set; } = "";

        [Display(Name = "Pamatuj si mě")]
        public bool RememberMe { get; set; }
    }
}
