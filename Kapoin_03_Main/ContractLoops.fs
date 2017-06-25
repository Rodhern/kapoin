// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Contracts

  open System
  open System.Collections.Generic
  open System.Reflection
  open UnityEngine
  open Rodhern.Kapoin.Helpers
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.Contracts
  open Rodhern.Kapoin.MainModule.Events
  
  
  type StaticRequirementCheckAttribute () =
    inherit Attribute ()
  
  
  [< AbstractClass >]
  type SRCAction =
    inherit System.Object // placeholder
  
  
  type StaticRequirementCheckScheduler () =
    inherit SceneAddonBehaviour "StaticRequirementCheckScheduler"
  
  
  type KapoinContractLoopBehaviour (name: string, checktype: ContractCheck) =
    inherit SceneAddonBehaviour (name)
  
  
  type KapoinInFlightContractLoop () =
    inherit KapoinContractLoopBehaviour (typeof<KapoinInFlightContractLoop>.Name, ContractCheck.FlightCheck)
  
  
  type KapoinOnGroundContractLoop () =
    inherit KapoinContractLoopBehaviour (typeof<KapoinOnGroundContractLoop>.Name, ContractCheck.GroundCheck)
  
