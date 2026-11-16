
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.InterviewRound
{
    public class InterviewRoundCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string RoundName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RoundNumber { get; set; }

        public string? Description { get; set; }
    }
}
