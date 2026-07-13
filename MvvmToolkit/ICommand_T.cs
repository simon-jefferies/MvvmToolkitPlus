namespace MvvmToolkit;

/// <summary>
/// Defines a command with a single generic parameter.
/// </summary>
/// <typeparam name="T">The type of parameter used by the command.</typeparam>
public interface ICommand<in T>
{
    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    event EventHandler CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can execute in its current state with the given parameter.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    bool CanExecute(T parameter);

    /// <summary>
    /// Executes the command with the given parameter.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    void Execute(T parameter);
}
