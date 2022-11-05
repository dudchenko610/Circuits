using Circuits.Services.Database;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Elements;

namespace Circuits.Services.Services;

public class StorageService : IStorageService
{
    private readonly DataIndexedDb _dataIndexedDb;
    private readonly ISchemeService _schemeService;

    public StorageService(
        DataIndexedDb dataIndexedDb, 
        ISchemeService schemeService)
    {
        _dataIndexedDb = dataIndexedDb;
        _schemeService = schemeService;
    }
    
    public async Task SaveAsync()
    {
        await _dataIndexedDb.OpenIndexedDb();
        await _dataIndexedDb.DeleteAll<Wire>();
        await _dataIndexedDb.DeleteAll<Transistor>();
        await _dataIndexedDb.DeleteAll<Resistor>();
        await _dataIndexedDb.DeleteAll<Inductor>();
        await _dataIndexedDb.DeleteAll<DCSource>();
        await _dataIndexedDb.DeleteAll<Capacitor>();

        var wires = _schemeService.Elements.OfType<Wire>().ToList();
        var transistors = _schemeService.Elements.OfType<Transistor>().ToList();;
        var resistors = _schemeService.Elements.OfType<Resistor>().ToList();;
        var inductors = _schemeService.Elements.OfType<Inductor>().ToList();;
        var dcSources = _schemeService.Elements.OfType<DCSource>().ToList();;
        var capacitors = _schemeService.Elements.OfType<Capacitor>().ToList();;

        await _dataIndexedDb.AddItems(wires);
        await _dataIndexedDb.AddItems(transistors);
        await _dataIndexedDb.AddItems(resistors);
        await _dataIndexedDb.AddItems(inductors);
        await _dataIndexedDb.AddItems(dcSources);
        await _dataIndexedDb.AddItems(capacitors);
    }

    public async Task RestoreAsync()
    {
        await _dataIndexedDb.OpenIndexedDb();
        
        var wires = await _dataIndexedDb.GetAll<Wire>();
        var transistors = await _dataIndexedDb.GetAll<Transistor>();
        var resistors = await _dataIndexedDb.GetAll<Resistor>();
        var inductors = await _dataIndexedDb.GetAll<Inductor>();
        var dcSources = await _dataIndexedDb.GetAll<DCSource>();
        var capacitors = await _dataIndexedDb.GetAll<Capacitor>();

        _schemeService.Clear();
        
        var elements = (List<Element>) _schemeService.Elements;
        
        elements.AddRange(wires);
        elements.AddRange(transistors);
        elements.AddRange(resistors);
        elements.AddRange(inductors);
        elements.AddRange(dcSources);
        elements.AddRange(capacitors);
        
        _schemeService.Reindex();
        _schemeService.Update();
    }
}