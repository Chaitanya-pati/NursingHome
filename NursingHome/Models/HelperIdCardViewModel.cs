namespace NursingHome.Models
{
    /// <summary>
    /// The small data contract needed by the ID-card presentation partial.
    /// The partial uses these IDs to retain the existing authenticated AJAX data flow.
    /// </summary>
    public sealed class HelperIdCardViewModel
    {
        public int HelperId { get; init; }
        public int UserId { get; init; }
    }
}