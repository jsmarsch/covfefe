﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Covfefe.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Covfefe.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Your Monitors will remain powered on, and your computer will not sleep.  To exit this mode, select &quot;Normal Operation&quot; in the context menu, or exit Covfefe..
        /// </summary>
        internal static string BalloonText_KeepMonitorsOn {
            get {
                return ResourceManager.GetString("BalloonText_KeepMonitorsOn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your computer will sleep and wake normally, as configured in your power management settings.  Use the context menu to enable &quot;Stay Awake&quot; or &quot;Monitors On&quot; Mode..
        /// </summary>
        internal static string BalloonText_Normal {
            get {
                return ResourceManager.GetString("BalloonText_Normal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to It&apos;s been a while since you have disabled sleep.  Did you forget?  You can click to here to return to Normal Mode..
        /// </summary>
        internal static string BalloonText_SleepReminder {
            get {
                return ResourceManager.GetString("BalloonText_SleepReminder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your computer will not sleep.  To exit this mode, select &quot;Normal Operation&quot; in the context menu, or exit Covfefe..
        /// </summary>
        internal static string BalloonText_StayAwake {
            get {
                return ResourceManager.GetString("BalloonText_StayAwake", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Keep Monitors On.
        /// </summary>
        internal static string BalloonTitle_KeepMonitorsOn {
            get {
                return ResourceManager.GetString("BalloonTitle_KeepMonitorsOn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Normal Operation.
        /// </summary>
        internal static string BalloonTitle_Normal {
            get {
                return ResourceManager.GetString("BalloonTitle_Normal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Re-enable Sleep?.
        /// </summary>
        internal static string BalloonTitle_SleepReminder {
            get {
                return ResourceManager.GetString("BalloonTitle_SleepReminder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stay Awake!.
        /// </summary>
        internal static string BalloonTitle_StayAwake {
            get {
                return ResourceManager.GetString("BalloonTitle_StayAwake", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        internal static System.Drawing.Icon coffee_cup {
            get {
                object obj = ResourceManager.GetObject("coffee_cup", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        internal static System.Drawing.Icon coffee_cup_cold {
            get {
                object obj = ResourceManager.GetObject("coffee_cup_cold", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
    }
}
