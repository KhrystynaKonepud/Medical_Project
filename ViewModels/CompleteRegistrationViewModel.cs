using Medical_center.Models;
using System.ComponentModel.DataAnnotations;

namespace Medical_center.ViewModels
{
    public class CompleteRegistrationViewModel
    {
        [Required(ErrorMessage = "Повне ім'я є обов'язковим")]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Адреса є обов'язковою")]
        [Display(Name = "Адреса")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Дата народження є обов'язковою")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата народження")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Стать є обов'язковою")]
        [Display(Name = "Стать")]
        public Gender Gender { get; set; }
    }
}