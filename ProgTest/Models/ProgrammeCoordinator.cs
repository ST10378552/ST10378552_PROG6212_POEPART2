﻿using System.ComponentModel.DataAnnotations;

namespace ProgTest.Models
{
    public class ProgrammeCoordinator
    {
        [Key]
        public int CoordinatorId { get; set; } // Primary Key

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } // Full name of the Programme Coordinator

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Email address of the Coordinator

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } // Phone number of the Coordinator
    }
}