using ControlLibrería.Data;
using Microsoft.EntityFrameworkCore;

namespace ControlLibrería.Services
{
    public interface IConfigurationService
    {
        Task<string> GetConfigurationAsync(string key, string defaultValue = "");
        Task SetConfigurationAsync(string key, string value, string description = "");
        Task<string> GetCurrencySymbolAsync();
        
        // Compatibility with previous interface if needed
        Task<string> GetConfigurationValueAsync(string key, string defaultValue = "");
        Task SetConfigurationValueAsync(string key, string value, string description = "");
        Task<List<ControlLibrería.Models.Entities.SystemConfiguration>> GetAllConfigurationsAsync();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly ApplicationDbContext _context;

        public ConfigurationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetConfigurationAsync(string key, string defaultValue = "")
        {
            var config = await _context.SystemConfigurations.FirstOrDefaultAsync(c => c.Key == key);
            return config?.Value ?? defaultValue;
        }

        public async Task SetConfigurationAsync(string key, string value, string description = "")
        {
            var config = await _context.SystemConfigurations.FirstOrDefaultAsync(c => c.Key == key);
            if (config == null)
            {
                _context.SystemConfigurations.Add(new Models.Entities.SystemConfiguration
                {
                    Key = key,
                    Value = value,
                    Description = description
                });
            }
            else
            {
                config.Value = value;
                if (!string.IsNullOrEmpty(description))
                    config.Description = description;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetCurrencySymbolAsync()
        {
            return await GetConfigurationAsync("CurrencySymbol", "Q");
        }

        public Task<string> GetConfigurationValueAsync(string key, string defaultValue = "") => GetConfigurationAsync(key, defaultValue);
        public Task SetConfigurationValueAsync(string key, string value, string description = "") => SetConfigurationAsync(key, value, description);
        public async Task<List<Models.Entities.SystemConfiguration>> GetAllConfigurationsAsync() => await _context.SystemConfigurations.ToListAsync();
    }
}
