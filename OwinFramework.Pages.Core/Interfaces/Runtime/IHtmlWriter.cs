﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A TextWriter that writes HTML to the response. Has special features for
    /// writing HTML elements and also for allowing multiple threads to simultaneously
    /// write into different parts of the output buffer.
    /// </summary>
    public interface IHtmlWriter
    {
        /// <summary>
        /// Returns a TextWriter implementation that allows you to write html
        /// using standard methods that work with TextWriter
        /// </summary>
        TextWriter GetTextWriter();

        /// <summary>
        /// Writes the buffered text to the response. Make sure all threads
        /// have finished writing before calling this method
        /// </summary>
        /// <param name="context">The owin context of the response to write</param>
        void ToResponse(IOwinContext context);

        /// <summary>
        /// Returns a task that will asynchronously write the response. Make sure all threads
        /// have finished writing before calling this method
        /// </summary>
        /// <param name="context">The owin context of the response to write</param>
        Task ToResponseAsync(IOwinContext context);

        /// <summary>
        /// Constructs and returns an HtmlWriter that will insert into the
        /// output buffer at the current spot. You can use this to start
        /// an async process that will insert text into the output when it
        /// completes.
        /// </summary>
        IHtmlWriter CreateInsertionPoint();

        /// <summary>
        /// Specifies how much to indent new lines. In pretty mode
        /// writes a number of spaces for each level of indent at the
        /// start of every line.
        /// </summary>
        int IndentLevel { get; set; }

        /// <summary>
        /// Writes a single character to the response buffer
        /// </summary>
        IHtmlWriter Write(char c);

        /// <summary>
        /// Writes a string to the response buffer
        /// </summary>
        IHtmlWriter Write(string s);

        /// <summary>
        /// Writes an object to the response buffer by calling its ToString method
        /// </summary>
        IHtmlWriter Write<T>(T s);

        /// <summary>
        /// Writes a string to the response buffer
        /// </summary>
        IHtmlWriter WriteLine();

        /// <summary>
        /// Writes a string to the response buffer
        /// </summary>
        IHtmlWriter WriteLine(string s);

        /// <summary>
        /// Writes an object to the response buffer by calling its ToString method
        /// </summary>
        IHtmlWriter WriteLine<T>(T s);

        /// <summary>
        /// Writes the opening tag of an html element
        /// </summary>
        /// <param name="tag">The html tag to write</param>
        /// <param name="selfClosing">Pass true to self close the element</param>
        /// <param name="attributePairs">Name value pairs of the element attributes</param>
        IHtmlWriter WriteOpenTag(string tag, bool selfClosing, params string[] attributePairs);

        /// <summary>
        /// Writes the opening tag of an html element that contains other elements
        /// You must close this element after writing the contents
        /// </summary>
        /// <param name="tag">The html tag to write</param>
        /// <param name="attributePairs">Name value pairs of the element attributes</param>
        IHtmlWriter WriteOpenTag(string tag, params string[] attributePairs);

        /// <summary>
        /// Writes the closing tag of an element
        /// </summary>
        /// <param name="tag">The element tag to close</param>
        IHtmlWriter WriteCloseTag(string tag);

        /// <summary>
        /// Writes a simple html element with an opening and closing tag and some
        /// content in between
        /// </summary>
        /// <param name="tag">The tag to write</param>
        /// <param name="content">The content inside the element</param>
        /// <param name="attributePairs">Attributes to apply to the opening tag</param>
        IHtmlWriter WriteElement(string tag, string content, params string[] attributePairs);

    }
}
