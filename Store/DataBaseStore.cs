using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Birko.Data.DataBase.Connector;

namespace Birko.Data.Store
{
    class DataBaseStore<DB, T> : IStore<T>
        where T : Model.AbstractModel
        where DB : AbstractConnector
    {
        private Dictionary<Guid, T> _insertList = null;
        private Dictionary<Guid, T> _updateList = null;
        private Dictionary<Guid, T> _deleteList = null;

        public AbstractConnector Connector { get; private set; }

        public DataBaseStore(PasswordSettings settings, InitConnector onInit = null)
        {
            Connector = DataBase.DataBase.GetConnector<DB>(settings);
            if (onInit != null)
            {
                Connector.OnInit += onInit;
            }
            _insertList = new Dictionary<Guid, T>();
            _updateList = new Dictionary<Guid, T>();
            _deleteList = new Dictionary<Guid, T>();
        }

        public void Init()
        {
            Connector?.DoInit();
        }

        public void Delete(T data)
        {
            if (data != null && data.Guid != null && !_deleteList.ContainsKey(data.Guid.Value))
            {
                _deleteList.Add(data.Guid.Value, data);
            }
        }

        public void List(Action<T> action)
        {
            List(null, action);
        }

        public void List(Expression<Func<T, bool>> filter, Action<T> action)
        {
            if (Connector != null && action != null)
            {
                Connector.Select(typeof(T), (data) =>
                {
                    if (data != null)
                    {
                        action?.Invoke((T)data);
                    }
                }, filter);
            }
        }

        public long Count()
        {
            if (Connector != null)
            {
                return Connector.SelectCount(typeof(T));
            }
            return  0;
        }

        public long Count(Expression<Func<T, bool>> filter)
        {
            if (Connector != null)
            {
                return Connector.SelectCount(typeof(T), filter);
            }
            return 0;
        }

        public void Save(T data, StoreDataDelegate storeDelegate = null)
        {
            if (data != null)
            {
                if (data.Guid == null) // new
                {
                    data.Guid = Guid.NewGuid();
                    if (!_insertList.ContainsKey(data.Guid.Value))
                    {
                        _insertList.Add(data.Guid.Value, data);
                    }
                    else
                    {
                        _insertList[data.Guid.Value] = data;
                    }
                }
                else //update
                {
                    if (data is Model.AbstractLogModel)
                    {
                        (data as Model.AbstractLogModel).UpdatedAt = DateTime.UtcNow;
                    }
                    if (!_updateList.ContainsKey(data.Guid.Value))
                    {
                        _updateList.Add(data.Guid.Value, data);
                    }
                    else
                    {
                        _updateList[data.Guid.Value] = data;
                    }
                }
                storeDelegate?.Invoke(data);
            }
        }

        public void StoreChanges()
        {
            if (Connector != null)
            {
                IEnumerable<DataBase.Field.AbstractField> primaryFields = new DataBase.Field.AbstractField[0];
                if (_deleteList.Count > 0 || _updateList.Count > 0)
                {
                    primaryFields = DataBase.DataBase.GetPrimaryFields(typeof(T));
                    if (primaryFields == null || !primaryFields.Any())
                    {
                        throw new Exceptions.StoreException("No primary fields in stored model");
                    }
                }
                //delete
                while (_deleteList.Count > 0)
                {
                    var kvp = _deleteList.First();
                    List<DataBase.Condition.Condition> conditions = new List<DataBase.Condition.Condition>();
                    foreach (var field in primaryFields)
                    {
                        conditions.Add(DataBase.DataBase.CreateCondition(field, kvp.Value));
                    }
                    Connector.Delete(typeof(T), conditions);
                    _deleteList.Remove(kvp.Key);
                }
                //update
                while (_updateList.Count > 0)
                {
                    var kvp = _updateList.First();

                    List<DataBase.Condition.Condition> conditions = new List<DataBase.Condition.Condition>();
                    foreach (var field in primaryFields)
                    {
                        conditions.Add(DataBase.DataBase.CreateCondition(field, kvp.Value));
                    }
                    Connector.Update(kvp.Value, conditions);
                    _updateList.Remove(kvp.Key);
                }
                //insert
                while (_insertList.Count > 0)
                {
                    var kvp = _insertList.First();
                    Connector.Insert(kvp.Value);
                    _insertList.Remove(kvp.Key);
                }
            }
            else
            {
                throw new Exceptions.StoreException("No database connector provided");
            }
        }
    }
}
