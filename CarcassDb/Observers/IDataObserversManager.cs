//using System;
//using System.Collections.Generic;

//namespace CarcassDb.Observers;

//public interface IDataObserversManager
//{
//    void NotifyAll();

//    //void Attach(string dataTableName, IDataObserver obs);
//    //void Detach(string dataTableName, IDataObserver obs);
//    //void Notify(string dataTableName);
//    void Attach(Type dataTableType, IDataObserver obs);
//    void Detach(Type dataTableType, IDataObserver obs);
//    void Notify(Type? dataTableType);
//    void NotifyList(List<Type> mayChangeTables);
//}

