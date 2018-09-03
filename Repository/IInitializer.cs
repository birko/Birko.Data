using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Repository
{
    public interface IInitializer
    {
        /// <summary>
        /// Perform repository initialisation.
        /// This should move the repository from it's current
        /// version up to the latest version.
        /// The IDbInitialiser will resolve how to locate and sort the
        /// required IConfigurators
        /// </summary>
        void InitialiseRepository();
        /// <summary>
        /// Perform repository initialisation.
        /// This should move the repository from it's current
        /// version up to the latest version.
        /// The correct sequence of the IConfigurators is the
        /// responsibility of the caller.
        /// </summary>
        void InitialiseRepository(IConfigurator[] configurators);
        /// <summary>
        /// After completion this should show the initial version of the repository
        /// </summary>
        long InitialVersion { get; }
        /// <summary>
        /// After completion this should show the final version of the repository
        /// </summary>
        long FinalVersion { get; }
        /// <summary>
        /// After completion this should show
        /// the number of IConfigurators which were executed
        /// </summary>
        long ConfiguratorsRun { get; }
    }
}
