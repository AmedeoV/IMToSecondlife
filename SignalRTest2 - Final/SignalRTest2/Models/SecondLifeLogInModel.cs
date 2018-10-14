using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Textosecondlife.Models
{
    public class SecondLifeLogInModel
    {

        [Display(Name = "Second Life Username: ")]
        public string SlUsernameLabel { get; set; }


        [Display(Name = "SL Username: ")]
        public string SlUsername { get; set; }


        [Display(Name = "Second Life Password: ")]
        public string SlPasswordLabel { get; set; }

        [Display(Name = "SL Password: ")]
        public string SlPassword { get; set; }

        public List<string> Usernames { get; set; }
    }
}