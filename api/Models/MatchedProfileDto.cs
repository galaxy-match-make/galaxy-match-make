namespace galaxy_match_make.Models;

public class MatchedProfileDto
{
    public Guid UserId { get; }
    public string DisplayName { get; } = string.Empty;
    public string? AvatarUrl { get; }
}
