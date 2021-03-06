﻿using System;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Supplies a very specific type of data when executed
    /// </summary>
    public interface IDataSupply
    {
        /// <summary>
        /// The runtime will call this for each page request that needs the
        /// type of data suplied by this instance.
        /// </summary>
        /// <param name="renderContext">The request that is being handled</param>
        /// <param name="dataContext">The data context to use to get dependant data and where
        /// new data should be added</param>
        void Supply(IRenderContext renderContext, IDataContext dataContext);

        /// <summary>
        /// When this property is true it means that the supplied data can
        /// be preloaded into the data context before the rendering operation begins
        /// and the data remains static for the duration of the rendering operation.
        /// When this property is false it means that the data changes during
        /// the rendering operation and any dependents of this supply should attach
        /// to the OnDataSupplied event to be notified when the data in the
        /// data context changes
        /// </summary>
        bool IsStatic { get; set; }

        /// <summary>
        /// There are a few unusual situations where other objects need to
        /// know when data has been updated in the data context. For the most
        /// part this is not required because the data context is established
        /// then used for rendering operations.
        /// One case where this event is needed is when a region repeats its
        /// contents based on a list of objects, and one of the descendants
        /// of the region contents is bound to data that is derrived from the
        /// repeating type. Like I said this is a very rare and unusual situation.
        /// </summary>
        void AddOnSupplyAction(Action<IRenderContext> onSupplyAction);
    }
}
