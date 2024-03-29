﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 組件的一般資訊是由下列的屬性集控制。
// 變更這些屬性的值即可修改組件的相關
// 資訊。
[assembly: AssemblyTitle("Common Library - Utility Logger")]
[assembly: AssemblyDescription("Common Library[Logger]")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jaofeng Chen")]
[assembly: AssemblyProduct("Common Library - Utility Logger")]
[assembly: AssemblyCopyright("Copyright © 2018 Jaofeng Chen All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 將 ComVisible 設為 false 可對 COM 元件隱藏
// 組件中的類型。若必須從 COM 存取此組件中的類型，
// 的類型，請在該類型上將 ComVisible 屬性設定為 true。
[assembly: ComVisible(false)]

// 下列 GUID 為專案公開 (Expose) 至 COM 時所要使用的 typelib ID
[assembly: Guid("b314a72d-1a1c-4ef0-bf82-c25b9488b277")]

// 組件的版本資訊由下列四個值所組成: 
//
//      主要版本
//      次要版本
//      組建編號
//      修訂編號
//
// 您可以指定所有的值，或將組建編號或修訂編號設為預設值
//方法是使用 '*'，如下所示:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.01.0324.10")]
[assembly: AssemblyFileVersion("1.01.0324.10")]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]