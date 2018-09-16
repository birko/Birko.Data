﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Repository
{
    public interface IConfigurator
    {
        /// <summary>
        /// Provides the version number of this set of repository changes
        /// </summary>
        int Version { get; }
        /// <summary>
        /// Perform any actions on the repository before changing the schema. This might
        /// involve copying data to temp tables etc to avoid data loss
        /// </summary>
        void PreMigrate<T, TModel>(IRepository<T, TModel> repository) where TModel: Model.AbstractModel;
        /// <summary>
        /// Update the repository schema
        /// The final task must be to update the version number in Sqlite
        /// </summary>
        void Migrate<T, TModel>(IRepository<T,TModel> repository) where TModel : Model.AbstractModel;
        /// <summary>
        /// Perform any actions on the repository after changing the schema. This might
        /// include copying data back from temporary tables and then cleaning up.
        /// </summary>
        void PostMigrate<T, TModel>(IRepository<T, TModel> repository) where TModel : Model.AbstractModel;
        /// <summary>
        /// Perform any calculation needed by the repository. This might include setting
        /// new column values to a default as well as genuine data seeding
        /// </summary>
        void Seed<T, TModel>(IRepository<T, TModel> repository) where TModel: Model.AbstractModel;
    }
}
