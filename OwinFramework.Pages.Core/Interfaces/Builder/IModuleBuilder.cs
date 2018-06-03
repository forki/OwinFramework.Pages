﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the module builder to construct modules using a fluent syntax
    /// </summary>
    public interface IModuleBuilder
    {
        /// <summary>
        /// Starts building a new module
        /// </summary>
        IModuleDefinition Module();
    }
}
