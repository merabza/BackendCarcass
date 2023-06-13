//using System.Collections.Generic;
//using Microsoft.Extensions.Logging;

//namespace CarcassDb.Observers;

//public sealed class DataTableChanges
//{
//    private readonly ILogger<DataObserversManager> _logger;
//    private readonly List<IDataObserver> _observers = new();

//    public DataTableChanges(ILogger<DataObserversManager> logger)
//    {
//        _logger = logger;
//    }

//    public void Add(IDataObserver obs)
//    {
//        if (!_observers.Contains(obs))
//            _observers.Add(obs);
//    }

//    public void Remove(IDataObserver obs)
//    {
//        if (_observers.Contains(obs))
//            _observers.Remove(obs);
//    }

//    public void Notify()
//    {
//        foreach (var obs in _observers)
//            //_logger.LogInformation($"---=== Notify ===--- {obs}");
//            obs.Notify();
//    }
//}

