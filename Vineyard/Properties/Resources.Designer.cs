﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace vineyard.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("vineyard.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to start = trim @(statement, newlines, 0, 0) final_trim
        ///
        ///none = /&amp;*/
        ///ws = /\s+/
        ///trim = /\s*/
        ///final_trim = /\s*$/
        ///newlines = /(\s*\n)+\s*/
        ///comma_or_newline = /\s*((\s*\n)+|,)\s*/
        ///colon_divider = trim &quot;:&quot; trim
        ///dot = &quot;.&quot;
        ///path_function_separator = &quot;.&quot; | &quot;|&quot;
        ///trim_same_line = /[ \t]*/
        ///
        ///id = /[a-zA-Z0-9_]+/
        ///
        ///path = @(id, dot, 2, 0)
        ///
        ///path_or_id = @(id, dot, 1, 0)
        ///
        ///id_or_array = id | array
        ///
        ///complex_token = id_or_array @(arguments, none, 0, 1)
        ///
        ///reference = @(complex_token, path_function_separator, [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string metahub_grammar {
            get {
                return ResourceManager.GetString("metahub_grammar", resourceCulture);
            }
        }
    }
}
