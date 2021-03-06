﻿using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute classes that define a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsPackageAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that identifies a class as a package
        /// </summary>
        /// <param name="name">The name of the package. Must be unique accross the 
        /// whole website, and must be valid as part of a css class name or
        /// JavaScript function name</param>
        public IsPackageAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Constructs an attribute that identifies a class as a package
        /// </summary>
        /// <param name="name">The name of the package. Must be unique 
        /// accross the whole website</param>
        /// <param name="namespaceName">The prefix to add to all names in
        /// this package to ensure that they do not conflict.
        /// Must be valid as part of a css class name or
        /// JavaScript function name. If you do not provide a namespace
        /// one will be auto generated, which works but can be
        /// difficult to debug</param>
        public IsPackageAttribute(string name, string namespaceName)
        {
            Name = name;
            NamespaceName = namespaceName;
        }

        /// <summary>
        /// Defines an optional name for this package so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines the namespace prefix to use for all dynamically generated
        /// names within this package
        /// </summary>
        public string NamespaceName { get; set; }
    }
}
