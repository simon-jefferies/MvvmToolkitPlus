# OliveStudio.Toolkit

A C# MVVM toolkit library that extends CommunityToolkit.Mvvm with additional strongly-typed abstractions for building robust MVVM applications.

## Installation

```bash
# Package manager
Install-Package MvvmToolkit

# .NET CLI
dotnet add package MvvmToolkit
```

## Dependencies

This library extends:
- **CommunityToolkit.Mvvm** - For base MVVM functionality
- **OliveStudio.Helpers** - For async event handler delegates

## Core Components

### `ObservableObject<TModel>`

A generic base class that combines CommunityToolkit.Mvvm's `ObservableObject` with a strongly-typed model, providing a clean separation between your view models and domain models.

```csharp
public abstract class ObservableObject<TModel> : ObservableObject
{
    public TModel Model { get; }
    
    protected ObservableObject(TModel model) { }
    protected ObservableObject() { }
}
```

**Benefits:**
- Strongly-typed access to your domain model
- Inherits all CommunityToolkit.Mvvm observable functionality
- Clear separation of concerns between UI and business logic

### `ICommand<T>`

A strongly-typed command interface that accepts a specific parameter type, providing better type safety than the standard `ICommand`.

```csharp
public interface ICommand<in T>
{
    event EventHandler CanExecuteChanged;
    bool CanExecute(T parameter);
    void Execute(T parameter);
}
```

### `ICommandAsync<T>`

An asynchronous command interface for handling async operations with strongly-typed parameters.

```csharp
public interface ICommandAsync<in T>
{
    event AsyncEventHandler CanExecuteChanged;
    bool CanExecute(T parameter);
    void Execute(T parameter);
}
```

**Note:** Uses `AsyncEventHandler` from OliveStudio.Helpers for async event handling.

## Usage Examples

### Observable Object with Model

```csharp
// Domain model
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// View model
public class UserViewModel : ObservableObject<User>
{
    public UserViewModel(User user) : base(user)
    {
    }
    
    // Expose model properties with change notification
    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (model, val) => model.Name = val);
    }
    
    public string Email
    {
        get => Model.Email;
        set => SetProperty(Model.Email, value, Model, (model, val) => model.Email = val);
    }
    
    // Computed properties
    public string DisplayName => $"{Model.Name} ({Model.Email})";
    
    // Can access the underlying model directly
    public DateTime CreatedAt => Model.CreatedAt;
}
```

### Command Implementation Example

While the library provides the interfaces, here's how you might implement them:

```csharp
public class RelayCommand<T> : ICommand<T>
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;
    
    public event EventHandler CanExecuteChanged;
    
    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(T parameter) => _canExecute?.Invoke(parameter) ?? true;
    
    public void Execute(T parameter) => _execute(parameter);
    
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

// Usage in view model
public class ProductViewModel : ObservableObject<Product>
{
    public ICommand<Product> SaveCommand { get; }
    public ICommand<int> DeleteCommand { get; }
    
    public ProductViewModel(Product product) : base(product)
    {
        SaveCommand = new RelayCommand<Product>(SaveProduct, CanSaveProduct);
        DeleteCommand = new RelayCommand<int>(DeleteProduct);
    }
    
    private bool CanSaveProduct(Product product) => !string.IsNullOrEmpty(product?.Name);
    private void SaveProduct(Product product) { /* Save logic */ }
    private void DeleteProduct(int productId) { /* Delete logic */ }
}
```

### Async Command Implementation Example

```csharp
public class AsyncRelayCommand<T> : ICommandAsync<T>
{
    private readonly Func<T, Task> _executeAsync;
    private readonly Func<T, bool> _canExecute;
    
    public event AsyncEventHandler CanExecuteChanged;
    
    public AsyncRelayCommand(Func<T, Task> executeAsync, Func<T, bool> canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(T parameter) => _canExecute?.Invoke(parameter) ?? true;
    
    public async void Execute(T parameter) => await _executeAsync(parameter);
    
    public async Task RaiseCanExecuteChangedAsync() 
    {
        if (CanExecuteChanged != null)
            await CanExecuteChanged(this, EventArgs.Empty);
    }
}

// Usage in view model
public class DataViewModel : ObservableObject<DataModel>
{
    public ICommandAsync<string> LoadDataCommand { get; }
    
    public DataViewModel(DataModel model) : base(model)
    {
        LoadDataCommand = new AsyncRelayCommand<string>(LoadDataAsync);
    }
    
    private async Task LoadDataAsync(string filter)
    {
        // Async data loading logic
        var data = await _dataService.LoadAsync(filter);
        Model.Items = data;
        OnPropertyChanged(nameof(Model));
    }
}
```

## Advanced Patterns

### Collection View Models

```csharp
public class UserListViewModel : ObservableObject<IList<User>>
{
    public ObservableCollection<UserViewModel> Users { get; }
    public ICommand<User> SelectUserCommand { get; }
    public ICommandAsync<string> SearchCommand { get; }
    
    public UserListViewModel(IList<User> users) : base(users)
    {
        Users = new ObservableCollection<UserViewModel>(
            users.Select(u => new UserViewModel(u)));
            
        SelectUserCommand = new RelayCommand<User>(SelectUser);
        SearchCommand = new AsyncRelayCommand<string>(SearchUsersAsync);
    }
    
    private void SelectUser(User user)
    {
        SelectedUser = Users.FirstOrDefault(vm => vm.Model == user);
    }
    
    private async Task SearchUsersAsync(string searchTerm)
    {
        var filteredUsers = await _userService.SearchAsync(searchTerm);
        Model.Clear();
        foreach (var user in filteredUsers)
        {
            Model.Add(user);
            Users.Add(new UserViewModel(user));
        }
    }
    
    [ObservableProperty]
    private UserViewModel _selectedUser;
}
```

### Nested Models

```csharp
public class OrderViewModel : ObservableObject<Order>
{
    public CustomerViewModel Customer { get; }
    public ObservableCollection<OrderItemViewModel> Items { get; }
    
    public OrderViewModel(Order order) : base(order)
    {
        Customer = new CustomerViewModel(order.Customer);
        Items = new ObservableCollection<OrderItemViewModel>(
            order.Items.Select(item => new OrderItemViewModel(item)));
    }
    
    public decimal TotalAmount => Items.Sum(item => item.Total);
    
    // Forward property changes from nested view models
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        if (e.PropertyName == nameof(Items))
        {
            OnPropertyChanged(nameof(TotalAmount));
        }
    }
}
```

## Integration with CommunityToolkit.Mvvm

This library works seamlessly with CommunityToolkit.Mvvm features:

```csharp
public partial class ProductViewModel : ObservableObject<Product>
{
    public ProductViewModel(Product product) : base(product)
    {
    }
    
    // Use CommunityToolkit.Mvvm source generators
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty] 
    private string _statusMessage;
    
    // Relay commands from CommunityToolkit.Mvvm
    [RelayCommand]
    private async Task SaveAsync()
    {
        IsLoading = true;
        try
        {
            await _productService.SaveAsync(Model);
            StatusMessage = "Product saved successfully";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    // Strongly-typed commands from this library
    public ICommand<Product> ValidateCommand { get; }
}
```

## Best Practices

### 1. Keep Models Pure
```csharp
// ❌ Don't put UI logic in models
public class User
{
    public string Name { get; set; }
    public bool IsVisible { get; set; } // UI concern
}

// ✅ Keep models focused on business logic
public class User
{
    public string Name { get; set; }
    public UserRole Role { get; set; }
}

public class UserViewModel : ObservableObject<User>
{
    public bool IsVisible => Model.Role != UserRole.Hidden;
}
```

### 2. Use Strongly-Typed Commands
```csharp
// ❌ Weak typing requires casting
public ICommand DeleteCommand { get; } // object parameter

// ✅ Strong typing prevents runtime errors
public ICommand<int> DeleteCommand { get; } // int parameter
```

### 3. Expose Model Properties Appropriately
```csharp
public class ProductViewModel : ObservableObject<Product>
{
    // ✅ Expose with change notification for bindable properties
    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
    }
    
    // ✅ Direct access for read-only properties
    public DateTime CreatedAt => Model.CreatedAt;
    
    // ✅ Computed properties based on model state
    public bool IsNew => Model.Id == 0;
}
```
