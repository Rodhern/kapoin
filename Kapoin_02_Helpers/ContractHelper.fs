// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.Contracts
  
  open System
  open System.Collections.Generic
  open UnityEngine
  open Contracts
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  open Rodhern.Kapoin.Helpers.ScenarioData
  
  
  type FilteredContractData () =
    inherit FilteringDataNode ()
  
  
  type ContractState = Contract.State
  
  
  type KapoinContractParameter () as kcpobj =
    inherit System.Object () // placeholder
  
  
  type KapoinContract () as kcobj =
    inherit System.Object () // placeholder
  
