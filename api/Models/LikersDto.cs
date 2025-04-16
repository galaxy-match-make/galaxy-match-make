using System;

namespace galaxy_match_make.Models;

public class LikersDto
{
    public Guid ReactorId { get; }
    public string DisplayName { get; } = string.Empty;
    public string? AvatarUrl { get; }

}
