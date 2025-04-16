namespace galaxy_match_make.Models;

public class ReactionDto
{
    public int Id { get; set; }
    public Guid ReactorId { get; set; }
    public Guid TargetId { get; set; }
    public bool IsPositive { get; set; }

}
