using System.ComponentModel.DataAnnotations;

namespace SpravaUzivatelu.Models
{
    public class Uzivatel
    {
        public int Id { get; set; }

        [Display(Name = "Jméno")]
        public string Jmeno { get; set; }

        [Display(Name = "Příjmení")]
        public string Prijmeni { get; set; }
    }
}
