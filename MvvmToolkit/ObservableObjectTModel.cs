using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmToolkit;

/// <summary>
/// Represents an observable object that has a model of type <typeparamref name="TModel"/>.
/// </summary>
/// <typeparam name="TModel">The type of model this observable object holds.</typeparam>
public abstract class ObservableObject<TModel> : ObservableObject
{
    /// <summary>
    /// Gets the model of this observable object.
    /// </summary>
    public TModel Model { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableObject{TModel}"/> class with the specified model.
    /// </summary>
    /// <param name="model">The model to associate with this observable object.</param>
    protected ObservableObject(TModel model)
    {
        Model = model;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableObject{TModel}"/> class.
    /// </summary>
    protected ObservableObject()
    {
    }
}
