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
  
  
  /// Specialized filtering variation of DataAndLoggerNode,
  /// used to persist custom contract and contract parameter data.
  type FilteredContractData () =
    inherit FilteringDataNode ()
    
    /// DESC_MISS
    let mutable persistedkeys: string array = [| |]
    
    /// DESC_MISS
    let isPersisted (key: string) =
      Array.contains key persistedkeys
    
    /// A filter where only keys from 'persistedkeys' are kept.
    let rec filter (kdata: KeyedDataNode) =
      let mutable reduced = false
      let vs = new SortedList<string, string list> ()
      let ns = new SortedList<string, KeyedDataNode list> ()
      for vk in kdata.values.Keys
       do if isPersisted vk
           then vs.Add (vk, kdata.values.[vk])
           else reduced <- true
      for nk in kdata.nodes.Keys
       do if isPersisted nk
           then let fnodes = List.map filter kdata.nodes.[nk]
                ns.Add (nk, List.map fst fnodes)
                reduced <- fnodes // update mutable variable
                           |> List.map snd
                           |> List.fold (||) reduced
           else reduced <- true
      { values= vs; nodes= ns }, reduced
    
    /// In this implementation only keys from 'persistedkeys' are kept.
    override data.DataFilter kdata =
      let fdata, reduced = filter kdata
      if reduced then Some fdata else None
    
    /// Initialize trace logger and data filter.
    /// The given filter keys are the 'only' keys that will survive filtering
    /// during load and save of custom data. It is recommended to pass the
    /// array of persisted keys ('filterkeys') by reference (i.e. uncloned).
    /// Note: The trace logger is initialized with its 'delay' flag enabled.
    member public data.Initialize (parenttype: Type, filterkeys: string array) =
      data.InitTraceLogger (parenttype, true)
      persistedkeys <- filterkeys
  
  
  /// Type abbreviation,
  /// such that ContractState and ParameterState enjoy similar names.
  type ContractState = Contract.State
  
  
  /// Indicates the type of contract check to perform on contract parameters.
  /// Most Kapoin contracts are either designed to be checked in flight (i.e.
  /// the flight scene) or on the ground (meaning the space center and
  /// tracking station scenes).
  type ContractCheck =
    /// DESC_MISS
    | FlightCheck
    /// DESC_MISS
    | GroundCheck
    with
      member public cc.IsInFlightCheck with get () = cc = FlightCheck
      member public cc.IsOnGroundCheck with get () = cc = GroundCheck
      override cc.ToString () =
        match cc with
        | FlightCheck -> "In-Flight Contract Check"
        | GroundCheck -> "On-Ground Contract Check"
  
  
  /// DESC_MISS
  type KapoinContractParameter =
    inherit ContractParameter
    
    /// The filtered data and logger pair.
    val data: FilteredContractData
    
    /// The constructor will initialize trace logger and data filter.
    new () as kcpobj =
      { inherit ContractParameter ()
        data= new FilteredContractData () } then
      do kcpobj.data.Initialize (kcpobj.GetType (), kcpobj.PersistedKeys ())
    
    /// Write message to trace listeners and KSP log file.
    member public cp.LogFn = cp.data.LogFn
    
    /// Unique logging identifier.
    member public cp.Uid = cp.data.Uid
    
    /// Access to keyed filtered custom data.
    member public cp.FilteredData = cp.data.KeyedData
    
    /// An array with all keys used to hold custom keyed data.
    /// Make sure to override this in actual contract parameter classes.
    abstract member PersistedKeys: unit -> string array
    default cp.PersistedKeys () = Array.empty
    
    /// Public access to private methods 'SetIncomplete', 'SetComplete', and 'SetFailed'.
    member public cp.SetState (state: ParameterState) =
      match state with
      | ParameterState.Incomplete -> cp.SetIncomplete ()
      | ParameterState.Complete -> cp.SetComplete ()
      | ParameterState.Failed -> cp.SetFailed ()
      | _ -> sprintf "Invalid value %d; valid options are Incomplete (0), Complete (1) and Failed (2)." (int state)
             |> invalidArg "state"
    
    /// DESC_MISS
    override cp.OnLoad (node) =
      cp.data.OnLoad node
      base.OnLoad node
    
    /// DESC_MISS
    override cp.OnSave (node) =
      base.OnSave node
      cp.data.OnSave node
    
    /// The hash string is equal to the Uid.
    override cp.GetHashString () = cp.Uid.ToString ()
    
    /// A contract parameter check may be triggered in the 'FLIGHT' scene.
    /// In that case the parent contract invokes 'FlightCheck'.
    /// By default nothing happens, but if overridden a contract parameter
    /// can choose to check the parameter conditions and update the parameter
    /// state, e.g. from 'Incomplete' to 'Complete'.
    abstract member FlightCheck: unit -> unit
    default cp.FlightCheck () = ()
    
    /// A contract parameter check similar to 'FlightCheck', but intended to
    /// be triggered in the 'SPACECENTER' and 'TRACKSTATION' scenes instead of
    /// the 'FLIGHT' scene.
    /// For most contracts you override either 'FlightCheck' or 'GroundCheck',
    /// but you may choose to override neither or both.
    abstract member GroundCheck: unit -> unit
    default cp.GroundCheck () = ()
  
  
  /// Helper record type to list Kapoin contract parameters (KCPs)
  /// sorted by parameter state.
  type KapoinContractParametersRecord =
    { IncompleteKCPs: KapoinContractParameter list;
      CompleteKCPs: KapoinContractParameter list;
      FailedKCPs: KapoinContractParameter list }
  
  
  /// DESC_MISS
  /// Notice: The KapoinContract class itself is not to be used as a contract,
  ///  but the class cannot be declared abstract or the contract system will
  ///  get upset.
  type KapoinContract =
    inherit Contract
    
    /// The filtered data and logger pair.
    val data: FilteredContractData
    
    /// The constructor will initialize trace logger and data filter.
    new () as kcobj =
      { inherit Contract ()
        data= new FilteredContractData ()
        canbedeclined= true
        canbecancelled= true } then
      do kcobj.data.Initialize (kcobj.GetType (), kcobj.PersistedKeys ())
    
    /// Default value of CanBeDeclined is 'true'.
    val mutable canbedeclined: bool
    
    /// Default value of CanBeCancelled is 'true'.
    val mutable canbecancelled: bool
    
    /// Write message to trace listeners and KSP log file.
    member public c.LogFn = c.data.LogFn
    
    /// Unique logging identifier.
    member public c.Uid = c.data.Uid
    
    /// Access to keyed filtered custom data.
    member public c.FilteredData = c.data.KeyedData
    
    /// An array with all keys used to hold custom keyed data.
    /// Make sure to override this in actual contract classes.
    abstract member PersistedKeys: unit -> string array
    default c.PersistedKeys () = Array.empty
    
    /// Override CanBeDeclined to use the local mutable variable.
    override c.CanBeDeclined () =
      c.canbedeclined
    
    /// Override CanBeCancelled to use the local mutable variable.
    override c.CanBeCancelled () =
      c.canbecancelled
    
    /// Look up agency by name in the 'AgentList' and if agency is found
    /// then assign that agency to contract's 'agent' variable.
    member public c.SetAgency (name: string) =
      match Agents.AgentList.Instance with
      | null
        -> "KapoinContract: AgentList object was null."
           |> Rodhern.Kapoin.Helpers.UtilityModule.LogError
      | agentlist
        -> match agentlist.Agencies
                 |> List.ofSeq
                 |> List.filter (fun (a: Agents.Agent) -> a.Name = name) with
           | [ agency ] -> c.agent <- agency
           | _ -> sprintf "KapoinContract: Agency '%s' not found." name
                  |> Rodhern.Kapoin.Helpers.UtilityModule.LogWarn
    
    /// DESC_MISS
    member public c.SetContractAcceptParams (canDecline: bool, canCancel: bool, ?autoaccept: bool) =
      c.canbedeclined <- canDecline
      c.canbecancelled <- canCancel
      match autoaccept with
      | None -> () // no need to access field in this case
      | Some b -> c.AutoAccept <- b
    
    /// DESC_MISS (set expiry and deadline types)
    member public c.SetDateTypes (expirytype: Contract.DeadlineType, deadlinetype: Contract.DeadlineType) =
      c.expiryType <- expirytype
      c.deadlineType <- deadlinetype
    
    /// DESC_MISS
    member public c.SetPrestige (prestige: Contract.ContractPrestige) =
      c.prestige <- prestige
    
    /// Look up celesital body by name.
    member public c.GetNamedBody (name: string) =
      let matches =
        Planetarium.FindObjectsOfType<CelestialBody> ()
        |> Array.filter (fun cb -> cb.name.ToUpper () = name.ToUpper ())
      match matches with
      | [| cbody |] -> cbody // equivalent to Array.exactlyOne
      | _ -> sprintf "Cannot find (unique) celestial body named '%s'." name
             |> KapoinContractError.Raise
    
    /// DESC_MISS
    override c.OnLoad (node) =
      c.data.OnLoad node
      base.OnLoad node
    
    /// DESC_MISS
    override c.OnSave (node) =
      base.OnSave node
      c.data.OnSave node
    
    /// The hash string is equal to the Uid.
    override c.GetHashString () = c.Uid.ToString ()
    
    // #region Contract state events
    // ---- ---- ---- ---- ---- ----
    
    // These methods log changes to the contract state.
    
    override c.OnOffered () = base.OnOffered (); c.LogFn "Contract offered."
    override c.OnOfferExpired () = base.OnOfferExpired (); c.LogFn "Contract offer expired."
    override c.OnWithdrawn () = base.OnWithdrawn (); c.LogFn "Contract offer withdrawn."
    override c.OnAccepted () = base.OnAccepted (); c.LogFn "Contract accepted."
    override c.OnDeclined () = base.OnDeclined (); c.LogFn "Contract declined."
    override c.OnDeadlineExpired () = base.OnDeadlineExpired (); c.LogFn "Contract deadline overdue."
    override c.OnCancelled () = base.OnCancelled (); c.LogFn "Contract cancelled."
    override c.OnCompleted () = base.OnCompleted (); c.LogFn "Contract completed."
    override c.OnFailed () = base.OnFailed (); c.LogFn "Contract failed."
    
    // ---- ---- ---- ---- ---- ----
    // #endregion
    
    /// Get a list of all KapoinContract contract instances currently found in
    /// the contract system. This is not a list of the contract types, but of
    /// the instances found in the lists 'Contracts' and 'ContractsFinished'.
    static member public ListKapoinContracts (?states: ContractState array) =
      let statefilter =
        match states with
        | None -> fun _ -> true
        | Some ss
          -> let stateset = Set.ofArray ss
             fun (contract: KapoinContract)
              -> stateset.Contains contract.ContractState
      match Contracts.ContractSystem.Instance with
      | csys when assigned csys
          -> [ for contract in csys.Contracts
                do match contract with
                   | :? KapoinContract as kc -> yield kc
                   | _ -> ()
               for contract in csys.ContractsFinished
                do match contract with
                   | :? KapoinContract as kc -> yield kc
                   | _ -> () ]
             |> List.filter statefilter
      | _ -> LogError "Contract system instance not available."; []
    
    /// Check if the generated contract have siblings already in play
    /// whether offered, active, completed, failed or cancelled.
    member public c.ListSiblingContracts (?states: ContractState array) =
      let isSibling (kc: KapoinContract) =
        (kc.GetType () = c.GetType ()) && (kc.Uid <> c.Uid)
      match states with
      | None -> KapoinContract.ListKapoinContracts ()
      | Some states -> KapoinContract.ListKapoinContracts states
      |> List.filter isSibling
    
    /// List contract parameters of type KapoinContractParameter.
    /// The parameters are sorted by parameter state.
    member public c.ListKapoinContractParameters () =
      let mutable incomplete: KapoinContractParameter list = []
      let mutable complete: KapoinContractParameter list = []
      let mutable failed: KapoinContractParameter list = []
      for cparam in c.AllParameters
       do match cparam with
          | :? KapoinContractParameter as cp when cp.State = ParameterState.Incomplete
            -> incomplete <- cp::incomplete
          | :? KapoinContractParameter as cp when cp.State = ParameterState.Complete
            -> complete <- cp::complete
          | :? KapoinContractParameter as cp when cp.State = ParameterState.Failed
            -> failed <- cp::failed
          | :? KapoinContractParameter as cp
            -> sprintf "Contract of type '%s' with KapoinContractParameter '%s' in undefined state (%d) encountered."
                 (c.GetType ()).Name (cp.GetType ()).Name (int cp.State)
               |> LogWarn
          | _ -> ()
      { IncompleteKCPs= incomplete; CompleteKCPs= complete; FailedKCPs= failed }
    
    /// A contract parameter check may be triggered in the 'FLIGHT' scene.
    /// The default implementation of 'FlightCheck' will in turn invoke each
    /// of the contract parameters' 'FlightCheck' methods.
    /// However, contract parameters with parameter state 'Complete' are
    /// checked before contract parameters with parameter state 'Incomplete',
    /// and contract parameters with parameter state 'Failed' and parameters
    /// not of type KapoinContractParameter are skipped.
    abstract member FlightCheck: unit -> unit
    default c.FlightCheck () =
      // first run through the already completed params (they might revert to incomplete),
      // then check the incomplete ones (to see if they have been completed);
      // if all params were taken in sequence the contract might prematurely complete.
      let kcps = c.ListKapoinContractParameters ()
      kcps.CompleteKCPs |> List.iter (fun cp -> cp.FlightCheck ())
      kcps.IncompleteKCPs |> List.iter (fun cp -> cp.FlightCheck ())
    
    /// A contract parameter check similar to 'FlightCheck', but triggered in
    /// the 'SPACECENTER' and 'TRACKSTATION' scenes instead of the 'FLIGHT'
    /// scene. The default implementation of 'GroundCheck' is similar to the
    /// default implementation of 'FlightCheck', but will invoke the contract
    /// parameter 'GroundCheck' methods, instead of the 'FlightCheck' methods.
    abstract member GroundCheck: unit -> unit
    default c.GroundCheck () =
      let kcps = c.ListKapoinContractParameters ()
      kcps.CompleteKCPs |> List.iter (fun cp -> cp.GroundCheck ())
      kcps.IncompleteKCPs |> List.iter (fun cp -> cp.GroundCheck ())
    
    /// DESC_MISS
    static member public CheckActiveContract (cctype: ContractCheck) (idx: int ref option) =
      let kclist = KapoinContract.ListKapoinContracts [| ContractState.Active |]
      let check (kc: KapoinContract) =
        if cctype.IsOnGroundCheck then kc.GroundCheck ()
        if cctype.IsInFlightCheck then kc.FlightCheck ()
      match idx with
      | None // this is a special case that might never be used
        -> List.iter check kclist
      | Some idxref
        -> if !idxref >= kclist.Length || !idxref < 0 then idxref:= 0 // adjust whenever the index passes beyond the end of the present list
           if not kclist.IsEmpty then
             check kclist.[!idxref]
             incr idxref
    
    // neither 'c.MeetRequirements ()' nor 'c.Generate ()' is overridden, so the base class contract is not generated
  
