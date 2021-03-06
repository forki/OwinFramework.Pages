﻿using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a component that you want to have discovered and 
    /// registered automitically at startup. If your component implements IComponent
    /// it works out better if you build and register it using a Package instead
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsComponentAttribute : IsElementAttributeBase
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a component
        /// </summary>
        /// <param name="name">The name of the component. Must be unique within a package</param>
        public IsComponentAttribute(string name)
            : base(name)
        {
        }
    }
}
