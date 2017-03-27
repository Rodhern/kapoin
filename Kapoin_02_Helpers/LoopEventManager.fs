﻿// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.Events
  
  open System
  open System.Collections.Generic
  open UnityEngine
  open Rodhern.Kapoin.Helpers
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type LoopMessageType =
    inherit System.Object // placeholder
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type LoopTimingInformation =
    inherit System.Object // placeholder
  
  
  [< AbstractClass >]
  type LoopMonitor (?name: string) =
    inherit SceneAddonBehaviour (defaultArg name "LoopMonitor")
  
