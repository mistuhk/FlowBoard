namespace FlowBoard.Modules.Tasks.Presentation.Contracts;

/// <summary>Request body for adding a comment.</summary>
/// <param name="Content">The comment body (1 to 10,000 characters).</param>
public sealed record AddCommentRequest(string Content);

/// <summary>Request body for editing a comment.</summary>
/// <param name="Content">The new body (1 to 10,000 characters).</param>
public sealed record EditCommentRequest(string Content);
