﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CirculationSCC.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.3.63;Initial Catalog=BRIT_SOVET;Persist Security Info=True;Us" +
            "er ID=sasha;Password=Corpse536")]
        public string BRIT_SOVETConnectionString {
            get {
                return ((string)(this["BRIT_SOVETConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.4.7;Initial Catalog=BRIT_SOVET;Persist Security Info=True;Use" +
            "r ID=tmpbrit;Password=brittmp")]
        public string BRIT_SOVET {
            get {
                return ((string)(this["BRIT_SOVET"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.3.241;Initial Catalog=BRIT_SOVET;Persist Security Info=True;U" +
            "ser ID=BJBritS_RW;Password=BJ_onlyReadWriteBS")]
        public string BRIT_SOVET_Crystal {
            get {
                return ((string)(this["BRIT_SOVET_Crystal"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.4.25,1443;Initial Catalog=Reservation_R;Persist Security Info" +
            "=True;User ID=ReaderOrd;Password=Reader_Order")]
        public string Reservation_RConnectionString {
            get {
                return ((string)(this["Reservation_RConnectionString"]));
            }
        }
    }
}
