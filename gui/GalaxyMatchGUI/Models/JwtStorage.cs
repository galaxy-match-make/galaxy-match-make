using System;

namespace GalaxyMatchGUI.Models;

public class JwtStorage
{
    private static readonly JwtStorage _instance = new JwtStorage();

    public static JwtStorage Instance => _instance;

    private JwtStorage() { }

    public AuthResponse authDetails { get; set; }
}

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string JwtToken { get; set; }
    public bool ProfileComplete { get; set; }
}
