using System.Text.Json;
using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly DapperContext _context;
    private readonly IConfiguration _configuration;

    public ProfileRepository(DapperContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ProfileDto>> GetAllProfiles()
    {
        var sql = GetProfileSql(false, false);
        var profiles = await QueryProfiles(sql, null);
        return profiles;
    }

    public async Task<ProfileDto> GetProfileById(Guid id)
    {
        var sql = GetProfileSql(true, false);
        var profile = await QueryProfiles(sql, new { Id = id });

        return profile.FirstOrDefault();
    }

    public async Task<ProfileDto> UpdateProfile(Guid id, UpdateProfileDto profile)
    {
        var sql = GetUpsertProfileSql(true);

        using var connection = _context.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var updatedProfileId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserId = id,
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                AvatarUrl = profile.AvatarUrl
            }, transaction);

            var deleteInterestsSql = "DELETE FROM user_interests WHERE user_id = @UserId;";
            await connection.ExecuteAsync(deleteInterestsSql, new { UserId = id }, transaction);

            if (profile.UserInterestIds != null && profile.UserInterestIds.Any())
            {
                var insertInterestsSql = @"
                    INSERT INTO user_interests (user_id, interest_id)
                    VALUES (@UserId, @InterestId);";

                foreach (var interestId in profile.UserInterestIds)
                {
                    await connection.ExecuteAsync(insertInterestsSql, new
                    {
                        UserId = id,
                        InterestId = interestId
                    }, transaction);
                }
            }

            transaction.Commit();
            return await GetProfileById(id);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<ProfileDto> CreateProfile(Guid id, CreateProfileDto profile)
    {
        var sql = GetUpsertProfileSql(false);

        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));;
        using var transaction = connection.BeginTransaction();

        try
        {
            var profileId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserId = id,
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                AvatarUrl = profile.AvatarUrl
            }, transaction);

            if (profile.UserInterestIds != null && profile.UserInterestIds.Any())
            {
                var insertInterestsSql = @"
                    INSERT INTO user_interests (user_id, interest_id)
                    VALUES (@UserId, @InterestId);";

                foreach (var interestId in profile.UserInterestIds)
                {
                    await connection.ExecuteAsync(insertInterestsSql, new
                    {
                        UserId = id,
                        InterestId = interestId
                    }, transaction);
                }
            }

            transaction.Commit();
            return await GetProfileById(id);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<ProfileDto>> GetPendingMatchesByUserId(Guid id)
    {
        var sql = GetProfileSql(false, true);
        var pendingMatches = await QueryProfiles(sql, new { Id = id });

        return pendingMatches;
    }

    // Fixed the async method to actually use await
    private async Task<IEnumerable<ProfileDto>> QueryProfiles(string sql, object? parameters = null)
    {
        var profileDictionary = new Dictionary<int, ProfileDto>();
        using var connection = _context.CreateConnection();

        // Use QueryAsync instead of Query to make this truly asynchronous
        var profiles = await connection.QueryAsync<ProfileDto, string, ProfileDto>(
                sql,
                (profile, userInterestsJson) =>
                {
                    // Initialize UserInterests if it's null
                    // if (profile.UserInterests == null)
                    //     profile.UserInterests = new List<UserInterestsDto>();
                    //
                    // if (!string.IsNullOrEmpty(userInterestsJson))
                    // {
                    //     profile.UserInterests = JsonSerializer.Deserialize<List<UserInterestsDto>>(userInterestsJson);
                    // }
                    // else
                    // {
                    //     profile.UserInterests = new List<UserInterestsDto>();
                    // }

                    profileDictionary[profile.Id] = profile;
                    return profile;
                },
                parameters,
                splitOn: "id, id, id, user_interests"
            );

        return profileDictionary.Values;
    }


    public async Task<IEnumerable<MatchedProfileDto>> GetUserMatchedProfiles(Guid UserId)
    {
        var query = @"
            SELECT p.user_id, p.display_name, avatar_url
            FROM profiles p
            INNER JOIN reactions r1 ON r1.target_id = p.user_id
            WHERE r1.reactor_id = @UserId
              AND r1.is_positive = true                     
              AND EXISTS (                                  
                  SELECT *                                  
                  FROM reactions r2
                  WHERE r2.reactor_id = r1.target_id        
                    AND r2.target_id = r1.reactor_id        
                    AND r2.is_positive = true
              );";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<MatchedProfileDto>(query, new { UserId });
    }

    private string GetProfileSql(bool withWhereClause, bool pendingLikesClause)
    {
        var sql = @"
            WITH profile_interests AS (
                SELECT 
                    p.user_id,
                    CASE 
                        WHEN COUNT(i.id) = 0 THEN NULL
                        ELSE json_agg(
                            json_build_object(
                                'InterestId', i.id,
                                'InterestName', i.interest_name
                            )
                        )
                    END AS user_interests
                FROM profiles p
                LEFT JOIN user_interests ui ON p.user_id = ui.user_id
                LEFT JOIN interests i ON ui.interest_id = i.id
                GROUP BY p.user_id
            )
            
            SELECT 
                p.id,
                p.user_id,
                p.display_name,
                p.bio,
                p.avatar_url,
                
                -- Aggregated interests
                json_agg(
                    jsonb_build_object(
                        'InterestId', i.id,
                        'InterestName', i.interest_name
                    )
                ) FILTER (WHERE i.id IS NOT NULL) AS user_interests ";
        if (pendingLikesClause)
        {
            sql += @"FROM reactions r
                          JOIN profiles p ON r.reactor_id = p.user_id ";
        }
        else
        {
            sql += @" FROM profiles p ";
        }

        sql += @"
                LEFT JOIN species s ON p.species_id = s.id
                LEFT JOIN planets pl ON p.planet_id = pl.id
                LEFT JOIN genders g ON p.gender_id = g.id
                LEFT JOIN user_interests ui ON p.user_id = ui.user_id
                LEFT JOIN interests i ON ui.interest_id = i.id";

        if (pendingLikesClause)
        {
            sql += @"
                 LEFT JOIN reactions r2 
                    ON r2.reactor_id = @Id 
                    AND r2.target_id = r.reactor_id

                WHERE r.target_id = @Id
                  AND r.is_positive = TRUE
                  AND r2.id IS NULL";
        }

        if (withWhereClause)
        {
            sql += " WHERE p.user_id = @Id ";
        }

        sql += @"
            GROUP BY 
            p.id, p.user_id, p.display_name, p.bio, p.avatar_url;";

        return sql;
    }

    public string GetUpsertProfileSql(bool isUpdate)
    {
        return isUpdate ? @"
            UPDATE profiles 
            SET display_name = @DisplayName, 
                bio = @Bio, 
                avatar_url = @AvatarUrl
            WHERE user_id = @UserId
            RETURNING id;" : @"
            INSERT INTO profiles 
            (user_id, display_name, bio, avatar_url)
            VALUES 
            (@UserId, @DisplayName, @Bio, @AvatarUrl)
            RETURNING id;";
    }


    public async Task<IEnumerable<LikersDto>> GetUserLikersProfiles(Guid UserId)
    {
        var query = @"
            SELECT r.reactor_id, p.display_name, avatar_url
            FROM profiles p
            INNER JOIN reactions r ON r.reactor_id = p.user_id
            WHERE r.target_id = @UserId
              AND r.is_positive = true";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<LikersDto>(query, new { UserId });
    }
}
