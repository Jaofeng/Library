﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 組件的一般資訊是由下列的屬性集控制。
// 變更這些屬性的值即可修改組件的相關
// 資訊。
[assembly: AssemblyTitle("Common Library - Utility")]
[assembly: AssemblyDescription("Common Library[Utility]")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jaofeng Chen")]
[assembly: AssemblyProduct("Common Library - Utility")]
[assembly: AssemblyCopyright("Copyright © 2018 Jaofeng Chen All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 將 ComVisible 設定為 false 會使得這個組件中的型別
// 對 COM 元件而言為不可見。如果您需要從 COM 存取這個組件中
// 的型別，請在該型別上將 ComVisible 屬性設定為 true。
[assembly: ComVisible(false)]

// 下列 GUID 為專案公開 (Expose) 至 COM 時所要使用的 typelib ID
[assembly: Guid("53fc732b-537b-49fa-a712-eb9cd332fb42")]

// 組件的版本資訊是由下列四項值構成:
//
//      主要版本
//      次要版本 
//      組建編號
//      修訂編號
//
// 您可以指定所有的值，也可以依照以下的方式，使用 '*' 將修訂和組建編號
// 指定為預設值:
[assembly: AssemblyVersion("1.37.0810.633")]
[assembly: AssemblyFileVersion("1.37.0810.633")]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]