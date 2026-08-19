namespace MvvmToolkitPlus;

/// <summary>
/// Defines an asynchronous command with a parameter of type <typeparamref name="TParameter"/>.
/// </summary>
/// <typeparam name="T">The type of the parameter passed to the command.</typeparam>
public interface ICommandAsync<in TParameter>
{
    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    event AsyncEventHandler CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">The parameter to be passed to the command.</param>
    /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
    bool CanExecute(TParameter parameter);

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">The parameter to be passed to the command.</param>
    void Execute(TParameter parameter);
}
