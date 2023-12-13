namespace FastLane.Chatbot.Contract.Model;
public interface IAction
{
}

/// <summary>
/// Represents some action to be executed by chatbot
/// </summary>
/// <typeparam name="TArg">Custom input for action</typeparam>
/// <typeparam name="TResult">Value returned after action execution completes</typeparam>
public interface IAction<TArg, TResult> : IAction
{
	Task<TResult> InvokeActionAsync(TArg argument, CancellationToken cancellationToken);
}

