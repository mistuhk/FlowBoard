using System.Text.RegularExpressions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Tasks.Domain.ValueObjects;

/// <summary>
/// Value object for a comment's body. Between 1 and 10,000 characters, preserved as-is (rendered as
/// Markdown client-side). On construction it extracts the <c>@handle</c> mentions it contains.
/// </summary>
public sealed class CommentContent : ValueObject
{
    private CommentContent(string value, IReadOnlyList<string> mentions)
    {
        Value = value;
        Mentions = mentions;
    }

    /// <summary>The raw comment text.</summary>
    public string Value { get; }

    /// <summary>The distinct @handles mentioned in the text (without the leading @).</summary>
    public IReadOnlyList<string> Mentions { get; }

    /// <summary>Creates a validated <see cref="CommentContent"/>, extracting any @mentions.</summary>
    /// <param name="value">The raw comment text.</param>
    /// <exception cref="DomainException">Thrown if the value is empty or exceeds 10,000 characters.</exception>
    public static CommentContent Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Comment content cannot be empty.");

        return value.Length > 10_000
            ? throw new DomainException("Comment content must not exceed 10,000 characters.")
            : new CommentContent(value, ExtractMentions(value));
    }

    /// <summary>
    /// Rehydrates a <see cref="CommentContent"/> from stored text, used by EF Core. Re-extracts
    /// mentions; skips length validation.
    /// </summary>
    /// <param name="value">The stored comment text.</param>
    public static CommentContent FromPersistence(string value) => new(value, ExtractMentions(value));

    // Matches @handles: an @ not preceded by a word character, @, or dot (so email addresses such as
    // "ada@example.com" are not treated as mentions), then dot-separated alphanumeric/_/- segments.
    private static readonly Regex MentionPattern = new(
        @"(?<![\w@.])@([a-zA-Z0-9_-]+(?:\.[a-zA-Z0-9_-]+)*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    // Extracts the distinct, lower-cased @handles in the text (without the leading @).
    private static IReadOnlyList<string> ExtractMentions(string value) =>
        MentionPattern.Matches(value)
            .Select(match => match.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .ToList();

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
