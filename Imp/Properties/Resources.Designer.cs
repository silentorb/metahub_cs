﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace imperative.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("imperative.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to start = trim @(namespace_class, newlines, 1, 0) final_trim
        ///
        ///namespace_class = namespace | class
        ///snippet_entry = trim @(snippet_function, semicolon_or_newline, 1, 0) final_trim
        ///
        ///none = /&amp;*/
        ///ws = /\s+/
        ///trim = /\s*/
        ///final_trim = /\s*$/
        ///newlines = /(\s*\n)+\s*/
        ///one_or_no_newline = /[ \t]*(\r\n)?[ \t]*/
        ///comma = trim &quot;,&quot; trim
        ///spaces = /[ \t]+/
        ///dot = &quot;.&quot;
        ///path_separator = &quot;.&quot;
        ///id = /[\$a-zA-Z0-9_]+/
        ///comma_or_newline = /\s*((\s*\n)+|,)\s*/
        ///semicolon_or_newline = /\s*((\s*\n)+|;)\s*/
        ///
        ///string = (&apos;&quot;&apos;  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string imp_grammar {
            get {
                return ResourceManager.GetString("imp_grammar", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to none = /&amp;*/
        ///ws = /\s+/
        ///trim = /\s*/
        ///final_trim = /\s*$/
        ///newlines = /(\s*\n)+\s*/
        ///one_or_no_newline = /[ \t]*(\r\n)?[ \t]*/
        ///comma = &quot;,&quot;
        ///spaces = /[ \t]+/
        ///path_separator = &quot;.&quot;
        ///id = /[\$a-zA-Z0-9_]+/
        ///comma_or_newline = /\s*((\s*\n)+|,)\s*/
        ///semicolon_or_newline = /\s*((\s*\n)+|;)\s*/
        ///line_comment = &quot;//[^\r\n]*&quot;
        ///string = (&apos;&quot;&apos; /[^&quot;]*/ &apos;&quot;&apos;) | (&quot;&apos;&quot; /[^&apos;]*/ &quot;&apos;&quot;)
        ///bool = &quot;true&quot; | &quot;false&quot;
        ///int = /-?[0-9]+/
        ///float = /-?([0-9]*\.)?[0-9]+f?/
        ///operator = &apos;+&apos; | &apos;-&apos; | &apos;/&apos; | &apos;*&apos; | &apos;&lt;=&apos; | &apos;&gt;=&apos; | &apos;&lt;&apos; | &apos;&gt;&apos; | &apos;==&apos;  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string imp_lexer {
            get {
                return ResourceManager.GetString("imp_lexer", resourceCulture);
            }
        }
    }
}
