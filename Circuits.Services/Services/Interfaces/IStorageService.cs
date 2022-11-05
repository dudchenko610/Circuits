namespace Circuits.Services.Services.Interfaces;

public interface IStorageService
{
    Task SaveAsync();
    Task RestoreAsync();
}