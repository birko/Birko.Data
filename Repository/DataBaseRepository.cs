using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Repository
{
    public abstract class DataBaseRepository<TConnector, TViewModel, TModel> : AbstractRepository<TViewModel, TModel>
        where TConnector : DataBase.Connector.AbstractConnector
        where TModel : Model.AbstractModel, Model.ILoadable<TViewModel>
        where TViewModel : Model.ILoadable<TModel>
    {
        public DataBaseRepository(string path, string filename, DataBase.Connector.InitConnector onInit = null)
        {
            _store = new Store.DataBaseStore<TConnector, TModel>(new Store.PasswordSettings()
            {
                Location = path,
                Name = filename
            }, onInit);
        }

        public override void Read(Action<TViewModel> readAction)
        {
            Read(null, readAction);
        }

        public override TViewModel Read(Guid Id)
        {
            if (_store != null)
            {
                TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                Read(x => x.Guid == Id, (item) =>
                {
                    result = item;
                });
                return result;
            }
            return default(TViewModel);
        }

        public virtual void Read(Expression<Func<TModel, bool>> expr, Action<TViewModel> readAction)
        {
            if(_store != null && readAction != null)
            {
                var connector = (_store as Store.DataBaseStore<TConnector, TModel>).Connector;
                connector.Select(typeof(TModel), (data) =>
                {
                    if (data != null)
                    {
                        TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                        result.LoadFrom((data as TModel));
                        readAction?.Invoke(result);
                    }
                }, expr);
            }
        }

        public virtual void ReadView<TView>(Action<TView> readAction)
        {
            ReadView(null, readAction);
        }

        public virtual void ReadView<TView>(Expression<Func<TView, bool>> expr, Action<TView> readAction)
        {
            if (_store != null && readAction != null)
            {
                var connector = (_store as Store.DataBaseStore<TConnector, TModel>).Connector;
                connector.SelectView(typeof(TView), (data) =>
                {
                    readAction((TView)data);
                }, expr);
            }
        }

        public virtual void Update(IDictionary<Expression<Func<TModel, bool>>, object> expresions, Expression<Func<TModel, bool>> expr)
        {
            if (_store != null)
            {
                var connector = (_store as Store.DataBaseStore<TConnector, TModel>).Connector;
                connector.Update(typeof(TModel), expresions, expr);
            }
        }
    }
}
