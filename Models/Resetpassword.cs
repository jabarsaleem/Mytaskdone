using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Task.Models
{
    public class Resetpassword


    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Enter new passowrd")]
    [DataType(DataType.Password)]
        public string newpassoword { get; set; }

        [Required(ErrorMessage = "Enter confirm passowrd")]
        [DataType(DataType.Password)]
        [Compare("newpassoword",ErrorMessage ="password does not match")]
        public string Confirmpassoword { get; set; }
        [Required]
        public string resendcode { get; set; }

        public static implicit operator Resetpassword(List<User> v)
        {
            throw new NotImplementedException();
        }
    }
}