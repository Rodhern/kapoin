// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers
  
  open System
  open System.Collections.Generic
  open System.Reflection
  open UnityEngine
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  type KapoinCache () =
    inherit SceneAddonBehaviour ("KapoinCache", true)
  
