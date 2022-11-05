using DnetIndexedDb;
using Microsoft.JSInterop;

namespace Circuits.Services.Database;

public class DataIndexedDb : IndexedDbInterop
{
    public DataIndexedDb(IJSRuntime jsRuntime, IndexedDbOptions<DataIndexedDb> indexedDbDatabaseOptions) : base(jsRuntime, indexedDbDatabaseOptions)
    {
    }
}