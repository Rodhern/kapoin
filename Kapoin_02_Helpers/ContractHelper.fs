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
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type KapoinContractParameter =
    inherit System.Object // placeholder
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type KapoinContract =
    inherit System.Object // placeholder
  
