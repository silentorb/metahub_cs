﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace metahub.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("metahub.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to contains(list, item) {
        ///	if (item != null &amp;&amp; list.contains(item) == false) {
        ///		list += item
        ///	}
        ///}
        ///
        ///equals(condition, first, second) {
        ///	if (condition) {
        ///		first = second
        ///	}
        ///}
        ///
        ///map_on_add(ref, $add) {
        ///	if (ref != null &amp;&amp; origin != ref) {
        ///		$add
        ///	}
        ///}
        ///
        ///map_add_to_list(T, list_add, hub, origin, main_item, $link) {
        ///	var item:T = new T()
        ///	item.initialize(hub)
        ///	item.$link = main_item
        ///	list.setter(item, origin)
        ///}
        ///
        ///add_to_list(T, list_add, hub, origin) {
        ///	var item:T = new T()
        ///	item.initialize [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string jackolantern_snippets {
            get {
                return ResourceManager.GetString("jackolantern_snippets", resourceCulture);
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
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///namespace metahub {
        ///
        ///  class Tick_Target {
        ///	hub:Hub
        ///    abstract tick()
        ///  }
        ///
        ///  class Hub {
        ///	tick_targets:Tick_Target[]
        ///
        ///    tick() {
        ///		for (var target in tick_targets) {
        ///			target.tick()
        ///		}
        ///    }
        ///  }
        ///
        ///}.
        /// </summary>
        internal static string metahub_imp {
            get {
                return ResourceManager.GetString("metahub_imp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;trellises&quot;: {
        ///    &quot;Hub&quot;: {
        ///      &quot;properties&quot;: {
        ///        &quot;tick_targets&quot;: {
        ///          &quot;type&quot;: &quot;list&quot;,
        ///          &quot;trellis&quot;: &quot;Tick_Target&quot;
        ///        }
        ///      }
        ///    },
        ///    &quot;Tick_Target&quot;: {
        ///      &quot;is_interface&quot;: true,
        ///      &quot;properties&quot;: {
        ///        &quot;hub&quot;: {
        ///          &quot;type&quot;: &quot;reference&quot;,
        ///          &quot;trellis&quot;: &quot;Hub&quot;
        ///        }
        ///      }
        ///    }
        ///  }
        ///}.
        /// </summary>
        internal static string metahub_json {
            get {
                return ResourceManager.GetString("metahub_json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///namespace piecemaker {
        ///
        ///	class Conflict {
        ///		abstract is_resolved():bool
        ///		abstract resolve()
        ///	}
        ///
        ///	class Conflict_Group {
        ///
        ///	}
        ///
        ///	class Piece_Maker {
        ///
        ///		tick() {
        ///			if (is_active)
        ///				return
        ///
        ///			if (conflicts.count() &gt; 0) {
        ///				var conflict:Conflict = conflicts.last()
        ///				conflicts.pop()
        ///				if (conflict.is_resolved)
        ///					return
        ///
        ///				conflict.resolve
        ///			}
        ///		}
        ///
        ///	}
        ///}.
        /// </summary>
        internal static string piecemaker_imp {
            get {
                return ResourceManager.GetString("piecemaker_imp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;targets&quot;: {
        ///    &quot;cpp&quot;: {
        ///      &quot;class_export&quot;: &quot;CORE_API&quot;
        ///    }
        ///  },
        ///  &quot;trellises&quot;: {
        ///    &quot;Piece_Maker&quot;: {
        ///      &quot;parent&quot;: &quot;metahub.Tick_Target&quot;,
        ///      &quot;properties&quot;: {
        ///        &quot;hub&quot;: {
        ///          &quot;type&quot;: &quot;reference&quot;,
        ///          &quot;trellis&quot;: &quot;metahub.Hub&quot;,
        ///          &quot;other_property&quot;: &quot;piece_maker&quot;
        ///        },
        ///        &quot;conflicts&quot;: {
        ///          &quot;type&quot;: &quot;list&quot;,
        ///          &quot;trellis&quot;: &quot;Conflict&quot;
        ///        },
        ///        &quot;is_active&quot;: {
        ///          &quot;type&quot;: &quot;bool&quot;,
        ///          &quot;default&quot;: true
        ///        }
        ///   [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string piecemaker_json {
            get {
                return ResourceManager.GetString("piecemaker_json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Distance_Conflict(Class_Name, Node_Type) {
        ///	class Class_Name : Conflict {
        ///	
        ///		nodes: Node_Type[]
        ///
        ///		is_resolved():bool {
        ///		
        ///		}
        ///
        ///		resolve() {
        ///			var offset:Vector3 = (nodes[0].position - nodes[1].position) / 2
        ///			nodes[0].position += offset
        ///			nodes[1].position -= offset
        ///		}
        ///	}
        ///}
        ///.
        /// </summary>
        internal static string piecemaker_snippets {
            get {
                return ResourceManager.GetString("piecemaker_snippets", resourceCulture);
            }
        }
    }
}
