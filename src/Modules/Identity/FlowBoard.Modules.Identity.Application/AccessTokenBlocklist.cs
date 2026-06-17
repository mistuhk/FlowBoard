namespace FlowBoard.Modules.Identity.Application;

/// <summary>
/// Builds the Redis keys for the access-token blocklist. Logging out adds the token's
/// <c>jti</c> to the blocklist for the remainder of its lifetime; the JWT bearer validation
/// rejects any token whose <c>jti</c> is present, so a revoked token stops working at once
/// rather than only when it would naturally expire.
/// </summary>
public static class AccessTokenBlocklist
{
    /// <summary>Builds the Redis key marking a token id (<c>jti</c>) as revoked.</summary>
    /// <param name="tokenId">The token's unique identifier (the <c>jti</c> claim).</param>
    public static string Key(string tokenId) => $"jwt_blocklist:{tokenId}";
}
