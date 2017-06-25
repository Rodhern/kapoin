// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Events
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.GameSettings
  open Rodhern.Kapoin.Helpers.Events
  open Rodhern.Kapoin.MainModule.Cache
  open Rodhern.Kapoin.MainModule.Data
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type LoopState =
    inherit System.Object // placeholder
  
  
  type MainKapoinLoop () =
    inherit LoopMonitor ("MainKapoinLoop")
    
    override monitor.Callback msg = // todo implement this override
      ()
  
