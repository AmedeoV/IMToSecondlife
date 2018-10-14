using MvcMobile.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Textosecondlife.Models
{
    public class HomeModel
    {
        [Display(Name = "From: ")]
        public string FromUsername { get; set; }

        [Display(Name = "SL FirstName: ")]
        public string slFirstName { get; set; }

        [Display(Name = "SL LastName: ")]
        public string slLastName { get; set; }

        [Required]
        [Display(Name = "Message * : ")]
        public string Message { get; set; }

        [Display(Name = "Submit")]
        public string SubmitText { get; set; }

        public string userIP { get; set; }

        public List<Friend> OnlineUsers { get; set; }

        public List<Friend> OfflineUsers { get; set; }

        public List<Friend> FriendList { get; set; }

        public string MyUserUiid { get; set; }


    }
}