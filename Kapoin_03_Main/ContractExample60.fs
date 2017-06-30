// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Contracts
  
  open Contracts
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.Contracts
  open Rodhern.Kapoin.MainModule.Contracts.SRCData
  
  
  // North of Sixty example
  
  /// Contract parameter (1/1) for contract 'C60'.
  type C60Param1 () =
    inherit KapoinContractParameter ()
    
    override cp.GetTitle () = "Land in The North"
    override cp.GetNotes () = " Land north of 60°N."
    override cp.FlightCheck () =
      match FlightGlobals.ActiveVessel with
      | v when assigned v
        -> let incomplete = 
             if not v.Landed then Some "Not landed."
              elif v.isEVA then Some "Is EVA (not a craft)."
              elif v.crewedParts < 1 then Some "Craft is uncrewed."
              elif v.mainBody.name <> "Kerbin" then Some "Not on Kerbin."
              elif v.latitude < 60. then Some "South of sixtieth parallel."
              else None
           match cp.State, incomplete with
           | ParameterState.Complete, Some _
             -> cp.SetState ParameterState.Incomplete
           | ParameterState.Incomplete, None
             -> cp.SetState ParameterState.Complete
           | _, _ -> ()
      | _ -> ()
  
  
  /// Example mission 'North of Sixty'.
  /// While we will likely keep the mission in future Kapoin releases, the true
  /// purpose of contract classes 'C60' and 'C70' is to test the custom Kapoin
  /// contract elements.
  type C60 () =
    inherit KapoinContract ()
    
    override c.Generate () =
      c.AddParameter (new C60Param1 (), "C60Param1Id") |> ignore
      let kerbin = c.GetNamedBody "Kerbin"
      base.SetContractAcceptParams (true, true, false)
      base.SetExpiry (8.f,12.f) // the default is floating deadlines
      base.SetDeadlineDays (60.f, kerbin)
      base.SetFunds (1800.f, 7850.f, -2450.f, kerbin) 
      base.SetScience (0.f, kerbin)
      base.SetReputation (3.f, -5.f, kerbin)
      base.SetPrestige Contract.ContractPrestige.Trivial
      base.SetAgency "Kapoin Poc Ltd." // hardcoded
      true
    
    override c.GetTitle () = "North of Sixty."
    override c.GetNotes () = "Mission Notes.\nSummary of objectives:\n"
    override c.GetSynopsys () = "Fly (or drive) to the northern part of Kerbin."
    override c.GetDescription () =
      "Now this again. Voices from the desolate parts of Kerbin claim we never visit."
      + " Prove that we go there, by going there. It does not matter how you get there;"
      + " you can use a rocket, for all I care."
    override c.MessageCompleted () =
      "Good job.\nNow we can say that we do go, the next time someone asks if we ever visit."
    
    override c.MeetRequirements () = ContractOfferWindowNode.AreRequirementsMet c
    
    [< StaticRequirementCheck >]
    /// DESC_MISS
    static member public C60SRC () = // todo rename and/or remove this SRC method
      LogLine "C60SRC invoked."
      let delta = { delta= 0.5 }
      let schedule = { initialoffset= 12.; maxoffered= 1;
                       lowwaitopen= 12.0; cwaitopen= 0.1; highwaitopen= 30.0;
                       lowwaitclose= 6.0; cwaitclose= 0.00; highwaitclose= 0.0 }
      let action (node, day) =
        let reqOk = // just some example rule to determine if we want to offer (more) C60 contracts
          let cs = KapoinContract.ListKapoinContracts [| ContractState.Completed |]
          let c60count = cs |> List.filter (fun c -> (c.GetType ()).Name = "C60") |> List.length
          let c70count = cs |> List.filter (fun c -> (c.GetType ()).Name = "C70") |> List.length
          if c60count + c70count < cs.Length
           then LogWarn <| sprintf "Count warning; C60 count: %d, C70 count: %d, total: %d." c60count c70count cs.Length
          c60count < 3 && c70count > 0
        match node, reqOk with
        // technically, in 'release' software, we would also want some branch that simply stop offering C60 altogether (i.e. 'Remove_Todo')
        | NoNode, true
          -> LogLine "C60SRC schedule contract offer."
             Schedule_Todo (schedule, delta)
        | Hibernate _, true
        | Schedule _, true
          -> LogLine "C60SRC contract schedule continued (hibernating or not)."
             Schedule_Todo (schedule, delta)
        | NoNode, false
        | Hibernate _, false
          -> LogLine "C60SRC contract offers waiting."
             Wait_Todo delta
        | Schedule _, false
          -> LogLine "C60SRC unschedule contract offers."
             Wait_Todo delta
      { new SRCAction () with override a.Execute () = SRCheckResult.WriteResult typeof<C60> action }
  
