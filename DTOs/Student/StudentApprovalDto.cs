
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Student
{
    public class StudentApprovalDto
    {
        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
