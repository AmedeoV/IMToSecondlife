
using System.ComponentModel.DataAnnotations;
namespace MvcMobile.Models
{
    public class InstanteMessage
    {
        public string MyUserUiid { get; set; }

        public string UserUiid { get; set; }

        [Display(Name = "To Username: ")]
        public string ToUsername { get; set; }

        [Required]
        [Display(Name = "Message * : ")]
        public string Message { get; set; }

        [Display(Name = "Chat : ")]
        public string ChatBox { get; set; }

        public string UserIp { get; set; }


    }
}