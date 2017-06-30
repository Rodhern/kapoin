// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Contracts
  
  open Contracts
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.Contracts
  open Rodhern.Kapoin.MainModule.Contracts.SRCData
  
  
  // Take a swim example
  
  /// Contract parameter (1/2) for contract 'C70'.
  type C70Param1 () =
    inherit KapoinContractParameter ()
    
    override cp.GetTitle () = "Go swimming"
    override cp.GetNotes () = " Go EVA in the ocean."
    override cp.FlightCheck () =
      match FlightGlobals.ActiveVessel with
      | v when assigned v
        -> let incomplete = 
             if not v.isEVA then Some "Is not on EVA."
              elif not v.Splashed then Some "Not in the water."
              elif v.mainBody.name <> "Kerbin" then Some "Not on Kerbin."
              else None
           match cp.State, incomplete with
           | ParameterState.Complete, Some _
             -> cp.SetState ParameterState.Incomplete
           | ParameterState.Incomplete, None
             -> cp.SetState ParameterState.Complete
           | _, _ -> ()
      | _ -> ()
  
  
  /// Contract parameter (2/2) for contract 'C70'.
  type C70Param2 () =
    inherit KapoinContractParameter ()
    
    override cp.GetTitle () = "Head out to sea"
    override cp.GetNotes () = " Be near the equator and east of 70°W."
    override cp.FlightCheck () =
      match FlightGlobals.ActiveVessel with
      | v when assigned v
        -> let incomplete = 
             if not v.LandedOrSplashed then Some "Not touched down."
              elif v.mainBody.name <> "Kerbin" then Some "Not on Kerbin."
              elif v.latitude < -10. || v.latitude > 10. then Some "Not at the equator."
              elif v.longitude < -70. || v.longitude > 540. then Some "Out of longtitude interval."
              elif v.longitude > 180. && v.longitude < 287. then Some "Too near the KSC."
              else None
           match cp.State, incomplete with
           | ParameterState.Complete, Some _
             -> cp.SetState ParameterState.Incomplete
           | ParameterState.Incomplete, None
             -> cp.SetState ParameterState.Complete
           | _, _ -> ()
      | _ -> ()
  
  
  /// Example mission 'Take a swim'.
  /// While we will likely keep the mission in future Kapoin releases, the true
  /// purpose of contract classes 'C60' and 'C70' is to test the custom Kapoin
  /// contract elements.
  type C70 () =
    inherit KapoinContract ()
    
    override c.Generate () =
      c.AddParameter (new C70Param1 (), "C70Param1Id") |> ignore
      c.AddParameter (new C70Param2 (), "C70Param2Id") |> ignore
      let kerbin = c.GetNamedBody "Kerbin"
      base.SetContractAcceptParams (true, true, false)
      base.SetExpiry (2.f,9.f) // the default is floating deadlines
      base.SetDeadlineDays (35.f, kerbin)
      base.SetFunds (1250.f, 5500.f, -3500.f, kerbin) 
      base.SetScience (0.f, kerbin)
      base.SetReputation (2.f, -3.f, kerbin)
      base.SetPrestige Contract.ContractPrestige.Trivial
      base.SetAgency "Kapoin Poc Ltd." // hardcoded
      true
    
    override c.GetTitle () = "Take a swim."
    override c.GetNotes () = "Mission Notes.\nSummary of objectives:\n"
    override c.GetSynopsys () = "Ditch in the ocean and go swimming."
    override c.GetDescription () =
      "Lately there have been a lot of rumors that the eastern parts of Kerbin's oceans"
      + " have become polluted by crashing, I mean returning, space vehicles."
      + " Next time you are there, go for a swim and show the critics that the water is"
      + " still safe to swim in."
      + "\nWe have taken water samples from the coast and several deep sea samples too."
      + " The critics continue to claim that we deliberately avoid the narrow band"
      + " around the equator further out to sea where most space vehicles end up when"
      + " comming back from space. I guess we just have to go there now!"
    override c.MessageCompleted () =
      "Mission completed.\nSwim recorded for television."
    
    override c.MeetRequirements () = ContractOfferWindowNode.AreRequirementsMet c
    
    [< StaticRequirementCheck >]
    static member public C70SRC () = // todo remove the verbose LogLine debugging
      LogLine "C70SRC invoked."
      let delta = { delta= 1.0 }
      let schedule = { initialoffset= 10.; maxoffered= 1;
                       lowwaitopen= 6.0; cwaitopen= 0.2; highwaitopen= 15.0;
                       lowwaitclose= 3.0; cwaitclose= 0.03; highwaitclose= 7.0 }
      let c60type = StaticRequirementCheckScheduler.FindKapoinContractClass "C60" // hardcoded to avoid compile time dependency
      if SRCTimeStampRec.CreateTimeStampNode typeof<C70>
       then LogLine "C70SRC created (first) SRC time stamp."
       else LogLine "C70SRC skipped SRC time stamping."
      let action (node, day) =
        LogLine "SRCheckAction for 'C70' invoked."
        if not ProgressTracking.Instance.reachSpace.IsComplete 
         then
          SRCheckResult.Wait_Todo delta
         else
          if SRCTimeStampRec.CreateTimeStampNode c60type
           then LogLine "C70SRC created SRC time stamp for C60."
          Schedule_Todo (schedule, delta)
      { new SRCAction () with // the return value is an SRCAction object expression
          override a.GetPriority with get () = 15
          override a.OnDispose () = () // just to check that the member comment shows up (debug)
          override a.Execute () = SRCheckResult.WriteResult typeof<C70> action }
  
