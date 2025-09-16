using System.ComponentModel.DataAnnotations;

namespace PersonalProjectNotes.Domain.Enums
{
   public enum PriorityNote
    {
        [Display(Name = "Низький")]
        Low = 0,

        [Display(Name = "Середній")]
        Medium = 1,

        [Display(Name = "Високий")]
        High = 2,

        [Display(Name = "Терміновий")]
        Urgent = 3
    }
}
