using ScreenTemperature.Entities;

namespace ScreenTemperature.Services;

public interface IKeyBindingService
{
    /// <summary>
    /// Get configurations
    /// </summary>
    IQueryable<KeyBinding> GetKeyBindings();

    /// <summary>
    ///  Create a <see cref="Profile"/>
    /// </summary>
    Task<KeyBinding> AddKeyBindingAsync(KeyBinding keyBinding);
}

public class KeyBindingService : IKeyBindingService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly DatabaseContext _databaseContext;

    public KeyBindingService(ILogger<ProfileService> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    /// <summary>
    /// Create a new <see cref="KeyBinding"/>.
    /// </summary>
    /// <param name="keyBinding">The <see cref="KeyBinding"/> to create.</param>
    public async Task<KeyBinding> AddKeyBindingAsync(KeyBinding keyBinding)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a list of <see cref="KeyBinding"/>.
    /// </summary>
    public IQueryable<KeyBinding> GetKeyBindings()
    {
        return _databaseContext.KeyBindings.AsQueryable();
    }
}