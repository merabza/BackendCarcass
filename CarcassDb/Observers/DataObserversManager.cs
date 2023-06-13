//using System;
//using System.Collections.Generic;
//using Microsoft.Extensions.Logging;

//namespace CarcassDb.Observers;

//public sealed class DataObserversManager : IDataObserversManager, IDisposable
//{
//    private readonly ILogger<DataObserversManager> _logger;
//    private readonly Dictionary<Type, DataTableChanges> _observers = new();

//    public DataObserversManager(ILogger<DataObserversManager> logger)
//    {
//        _logger = logger;
//    }

//    public void Attach(Type dataTableType, IDataObserver obs)
//    {
//        if (!_observers.ContainsKey(dataTableType))
//            _observers.Add(dataTableType, new DataTableChanges(_logger));
//        _observers[dataTableType].Add(obs);
//    }

//    public void Detach(Type dataTableType, IDataObserver obs)
//    {
//        if (_observers.ContainsKey(dataTableType))
//            _observers[dataTableType].Remove(obs);
//    }

//    public void NotifyAll()
//    {
//        foreach (var kvp in _observers)
//            kvp.Value.Notify();
//    }


//    public void NotifyList(List<Type> mayChangeTables)
//    {
//        foreach (var mayChangeTable in mayChangeTables)
//            Notify(mayChangeTable);
//    }


//    public void Notify(Type? dataTableType)
//    {
//        if (dataTableType is not null && _observers.ContainsKey(dataTableType)) _observers[dataTableType].Notify();
//    }

//    #region IDisposable

//    public void Dispose()
//    {
//        Dispose(true);
//        GC.SuppressFinalize(this);
//        GC.Collect();
//    }

//    private void Dispose(bool disposing)
//    {
//        if (disposing) _observers.Clear();
//        // Free your own state (unmanaged objects).
//        // Set large fields to null.
//    }

//    ~DataObserversManager()
//    {
//        // Simply call Dispose(False).
//        Dispose(false);
//    }

//    #endregion
//}

