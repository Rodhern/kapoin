// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.ScenarioData
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  type KeyedList<'K,'T when 'K: equality> () =
    inherit System.Object () // placeholder
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type KeyedDataNode =
    inherit System.Object // placeholder
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type DataAndLoggerNode =
    inherit System.Object // placeholder
  
  
  [< AbstractClass >]
  type FilteringDataNode () =
    inherit System.Object () // placeholder
  
