namespace CampusPlacementManagementAPI.DTOs.Job
{
    public class JobEligibilityResponseDto
    {
        public int JobId { get; set; }

        public bool IsEligible { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}