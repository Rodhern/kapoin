// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Contracts.MilestoneData
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.ScenarioData
  open Rodhern.Kapoin.MainModule.Data
  open Rodhern.Kapoin.MainModule.Data.KeyedDataNodeExtensions
  
  
  module Constants =
    
    /// Node for persisted space center milestone data.
    let [< Literal >] public MilestoneNodeName = "MILESTONE_DATA"
  
  
  open Constants
  
  [< AbstractClass >]
  /// A static class with a few functions.
  type MilestoneUtilClass = // todo - rename class ?
    class end
  
