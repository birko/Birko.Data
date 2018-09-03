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

        public override IEnumerable<TViewModel> Read()
        {
            return Read(null);
        }

        public override TViewModel Read(Guid Id)
        {
            if (_store != null)
            {
                var list = Read(x => x.Guid == Id);
                if (list != null && list.Any())
                {
                    return list.First();
                }
            }
            return default(TViewModel);
        }

        public virtual IEnumerable<TViewModel> Read(Expression<Func<TModel, bool>> expr)
        {
            List<TViewModel> list = new List<TViewModel>();
            if(_store != null)
            {
                var connector = (_store as Store.DataBaseStore<TConnector, TModel>).Connector;
                connector.Select(typeof(TModel), (data) =>
                {
                    if (data != null)
                    {
                        TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                        result.LoadFrom((data as TModel));
                        list.Add(result);
                    }
                }, expr);
            }
            return list.ToArray();
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
