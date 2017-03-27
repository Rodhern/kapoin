// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.GUI
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Wrappers.Toolbar
  open Rodhern.Kapoin.Helpers
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type BlizzyMouseButton =
    inherit System.Object // placeholder
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type BlizzyButtonData =
    inherit System.Object // placeholder
  
  
  and  BlizzyButtonBehaviour (buttondata: BlizzyButtonData) =
    inherit System.Object () // placeholder
  
  
  open UnityEngine
  open KSP.UI.Screens
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type AppBarMouseClick =
    inherit System.Object // placeholder
  
  
  type AppScene = ApplicationLauncher.AppScenes
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type AppBarButtonData =
    inherit System.Object // placeholder
  
  
  and  AppBarButtonBehaviour (buttondata: AppBarButtonData) =
    inherit System.Object () // placeholder
  
  
  type AppBarButtonClass (buttondata: AppBarButtonData, ?staticref: AppBarButtonClass option ref) as instance =
    inherit AppBarButtonBehaviour (buttondata)
  
