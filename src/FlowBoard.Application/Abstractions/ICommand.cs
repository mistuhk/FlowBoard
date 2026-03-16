using MediatR;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Marker interface for commands, state-mutating operations that return no value.
/// The <c>TransactionBehaviour</c> wraps all <see cref="ICommand"/> handlers in a
/// database transaction. Commands that need to return a value should use
/// <see cref="ICommand{TResponse}"/> instead.
/// </summary>
public interface ICommand : IRequest<Unit>
{
}

/// <summary>
/// Marker interface for commands that return a typed response.
/// Like <see cref="ICommand"/>, these are wrapped in a database transaction
/// by the <c>TransactionBehaviour</c>.
/// </summary>
/// <typeparam name="TResponse">The type returned on successful execution.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
}
