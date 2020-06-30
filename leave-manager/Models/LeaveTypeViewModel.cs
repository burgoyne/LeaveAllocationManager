using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_manager.Models
{
    public class LeaveTypeViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Identifier")]
        public string Name { get; set; }
        [Required]
        [Range(1, 31, ErrorMessage = "You must not exceed 31 days. Please enter a valid number.")]
        [Display(Name = "Default Number of Days")]
        public int DefaultDays { get; set; }
        [Display(Name="Date Created")]
        public DateTime? DateCreated { get; set; }
    }
}
