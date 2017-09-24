// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Contracts
  
  open Contracts
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  open Rodhern.Kapoin.Helpers.ScenarioData
  open Rodhern.Kapoin.Helpers.ReflectedWrappers
  open Rodhern.Kapoin.Helpers.Contracts
  open Rodhern.Kapoin.MainModule.Cache
  open Rodhern.Kapoin.MainModule.Data
  open Rodhern.Kapoin.MainModule.Data.KeyedDataNodeExtensions
  open Rodhern.Kapoin.MainModule.Contracts.SRCData
  
  
  // Supersonic simulation example
  
  /// TODO
  type C350Result =
    inherit SimulationResultNode
    
    static member public CacheKey = "C350" // used as a key in cache
    
    /// Constructor for simulation result node.
    new () as self =
      { inherit SimulationResultNode (typeof<C350Result>) }
      then self.UT <- 0. // initialize with some data
    
    /// A data field.
    member public self.UT
     with get () = self.KeyedData.LookUpAndParse "UT" str2float
     and set (t: float) = KeyedList.Set self.KeyedData.values "UT" [ float2str t ]
    
    /// Return a list of the C350 simulation result nodes from the cached
    /// main Kapoin node.
    static member public SimResNodes () =
      let envelopes =
        match KeyedDataNode.TryGetTopnode<KapoinMainNode> ()
           |> KeyedDataNode.TryGetSubnode Constants.SimResCollectionNode with
        | None -> []
        | Some cnode -> KeyedList.Get cnode.nodes Constants.SimResEnvelopeNode
        |> C350Result.FilterByNameAndType (C350Result.CacheKey, typeof<C350Result>)
      let datalist =
        [ for envelope in envelopes
           do yield! envelope.nodes.[Constants.SimResDataNode] ]
      [ for kdn in datalist
         do let simresnode = new C350Result ()
            do simresnode.KeyedData <- kdn
            yield simresnode ]
    
    /// Delete all C350Result nodes from the cached marshalled envelope
    /// collection. Also remove the collection node if it ends up empty.
    static member public RemoveNodes () =
      let mainnodeopt = KeyedDataNode.TryGetTopnode<KapoinMainNode> ()
      match KeyedDataNode.TryGetSubnode Constants.SimResCollectionNode mainnodeopt with
      | None -> ()
      | Some cnode ->
        let filteredenvelopes =
          KeyedList.Get cnode.nodes Constants.SimResEnvelopeNode
          |> C350Result.FilterAwayMatches [ C350Result.CacheKey, typeof<C350Result> ]
        cnode.nodes.[Constants.SimResEnvelopeNode] <- filteredenvelopes
        if filteredenvelopes.IsEmpty
         then mainnodeopt.Value.nodes.Remove Constants.SimResCollectionNode |> ignore
  
  
  /// Contract parameter (1/2) for contract 'C350'.
  type C350Param1 () =
    inherit KapoinContractParameter ()
    
    override cp.GetTitle () = "Low level supersonic"
    override cp.GetNotes () = " Maintain high speed (350 - 450 m/s)"
                              + " at low level (500 - 1000 m)"
                              + " for some time (approx 12 secs)."
    // override cp.PersistedKeys () = [| |] // do not persist any of the custom values // do not add this code line - it will throw a runtime exception with message "InvalidOperationException: The initialization of an object or value resulted in an object or value being accessed recursively before it was fully initialized." !!
    override cp.SimulatorCheck () =
      if (cp.State = ParameterState.Incomplete) && (cp.IsUTBandSuccess ())
       then cp.SetState ParameterState.Complete
    override cp.GroundCheck () =
      if (cp.State = ParameterState.Incomplete)
       then let simresults = C350Result.SimResNodes ()
            if not simresults.IsEmpty
             then cp.SetState ParameterState.Complete
    
    /// Get a named floating point value from the filtered data.
    member private cp.GetValue (key: string) =
      match KeyedList.Get cp.FilteredData.values key with
      | [] -> 0.
      | [ sval ]
        -> match str2float sval with
           | Some v -> v
           | None -> sprintf "Unable to parse floating point value \"%s\" for parameter '%s'." sval key
                     |> KapoinPersistenceError.Raise
      | xs -> sprintf "Unable to parse value for parameter '%s'; multiple (%d) values found." key xs.Length
             |> KapoinPersistenceError.Raise
    
    /// Set a named floating point value in filtered data.
    member private cp.SetValue (key: string) (value: float) =
      let sval = float2str value
      KeyedList.Set cp.FilteredData.values key [ sval ]
    
    /// Floating point value property.
    member private cp.UTFirst
     with get () = cp.GetValue "first"
     and set v = cp.SetValue "first" v
    
    /// Floating point value property.
    member private cp.UTLast
     with get () = cp.GetValue "last"
     and set v = cp.SetValue "last" v
    
    /// Abbreviation for GetUniversalTime from the planetarium.
    member private cp.UTNow
     with get () = Planetarium.GetUniversalTime ()
    
    /// Determine if the craft is in the required parameter band.
    member private cp.InBand () =
      let v = FlightGlobals.ActiveVessel
      if not (assigned v) then false
       elif v.situation <> Vessel.Situations.FLYING then false
       elif v.isEVA then false
       elif v.crewedParts < 1 then false
       elif (v.srfSpeed < 350. || v.srfSpeed > 450.) then false
       elif (v.altitude < 500. || v.altitude > 999.) then false
       else true
    
    /// Update the recorded time interval the plane is in the parameter band.
    member private cp.UpdateUT () =
      let first, last, now = cp.UTFirst, cp.UTLast, cp.UTNow
      if first > now || last > now || last + 60. < now
       then if cp.InBand ()
             then cp.UTFirst <- now; cp.UTLast <- now; // plane entered band
             else ()
       else if cp.InBand ()
             then cp.UTLast <- now; // plane remained in band
             else cp.UTFirst <- 0.; cp.UTLast <- 0.; // plane left band
    
    /// Check if a simulation result is already cached, and if not update the
    /// recorded band time, and finally return if the simulation is a success.
    member private cp.IsUTBandSuccess () =
      if not IndexBoard.Ready then false
       elif IndexBoard.Instance.ContainsRef<C350Result> C350Result.CacheKey then true
       else cp.UpdateUT ()
            if cp.UTFirst + 8. < cp.UTLast
             then let simres = new C350Result ()
                  simres.UT <- cp.UTLast
                  IndexBoard.Instance.AddRef (simres, C350Result.CacheKey)
                  true
             else false
  
  
  /// Contract parameter (2/2) for contract 'C350'.
  type C350Param2 () =
    inherit KapoinContractParameter () // download simulator data
    
    override cp.GetTitle () = "Download simulator data"
    override cp.GetNotes () = " Wait for simulator data to download once the simulation is shut down."
    override cp.GroundCheck () =
      if (cp.State = ParameterState.Incomplete)
       then let simresults = C350Result.SimResNodes ()
            let now = Planetarium.GetUniversalTime ()
            if simresults |> List.exists (fun res -> res.UT + 1200. < now)
             then cp.SetState ParameterState.Complete
                  C350Result.RemoveNodes ()
  
  
  /// Example mission 'Supersonic simulation'.
  /// While we will likely keep the mission in future Kapoin releases, the true
  /// purpose of the contract is to test progress information interoperability
  /// with the 'KRASH' simulation mod.
  type C350 () =
    inherit KapoinContract ()
    
    override c.Generate () =
      c.AddParameter (new C350Param1 (), "C350Param1Id") |> ignore
      c.AddParameter (new C350Param2 (), "C350Param2Id") |> ignore
      let kerbin = c.GetNamedBody "Kerbin"
      base.SetContractAcceptParams (true, true, false)
      base.SetExpiry (9.f,21.f) // the default is floating deadlines
      base.SetDeadlineDays (3.f, kerbin) // you should carry out the simulation very soon
      base.SetFunds (0.f, 2800.f, 0.f, kerbin) 
      base.SetScience (1.f, kerbin)
      base.SetReputation (0.f, 0.f, kerbin)
      base.SetPrestige Contract.ContractPrestige.Trivial
      base.SetAgency "Kapoin Poc Ltd." // hardcoded
      true
    
    override c.GetTitle () = "Supersonic simulation."
    override c.GetNotes () = "Remarks:\nNotice that the objective states shown"
                             + " are not updated while in the editor (SPH and VAB)."
                             + "\nSummary of objectives:\n"
    override c.GetSynopsys () = "Simulate low level supersonic flight."
    override c.GetDescription () =
      "We need to get more experience with analyzing simulator data."
      + " Also we would like to compile performance data for low level"
      + " supersonic flight. Quite possibly crewed low level supersonic flight"
      + " is too risky at this stage anyway, so we feel simulation is the way"
      + " to go."
      + "\nLaunch the simulator with a crewed plane capable of supersonic"
      + " flight. Do not worry about the crew; it is a simulation — everyone"
      + " is going to be fine."
      + "\nDuring simulation establish and maintain between 350 and 450 m/s,"
      + " at an altitude of 500 to 1000 m, for roughly 10 or 12 seconds."
      + " Once the simulation is done the engineers will download the"
      + " simulated flight data. When back at the space center expect it to"
      + " take about half an hour to get the download done."
    override c.MessageCompleted () =
      "Contract complete. Performance data downloaded from simulator memory."
    
    override c.MeetRequirements () = ContractOfferWindowNode.AreRequirementsMet c
    
    [< StaticRequirementCheck >]
    static member public C350SRC () = // todo remove the verbose LogLine debugging
      LogLine "C350SRC invoked."
      let delta = { delta= 1.0 }
      let schedule = { initialoffset= 10.; maxoffered= 1; // todo just some params - we should wait to offer contracts until ??
                       lowwaitopen= 6.0; cwaitopen= 0.2; highwaitopen= 15.0;
                       lowwaitclose= 1.0; cwaitclose= 0.03; highwaitclose= 3.0 }
      if KRASHHelper.ModPresent
       then if SRCTimeStampRec.CreateTimeStampNode typeof<C350>
             then LogLine "C350SRC created (first) SRC time stamp."
             else LogLine "C350SRC skipped SRC time stamping."
       else if SRCTimeStampRec.DeleteTimeStampNode typeof<C350>
             then LogLine "C350SRC removed SRC time stamp; KRASH mod not present."
             else LogLine "C350SRC skipped SRC time stamping; KRASH mod not present."
      let action (node, day) =
        LogLine "SRCheckAction for 'C350' invoked (debug, always return 'Schedule_Todo')."
        SRCheckResult.Schedule_Todo (schedule, delta)
      { new SRCAction () with
          override a.Execute () = SRCheckResult.WriteResult typeof<C350> action }
  
