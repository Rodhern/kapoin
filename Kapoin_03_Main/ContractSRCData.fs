// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Contracts.SRCData
  
  open System
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  open Rodhern.Kapoin.Helpers.ScenarioData
  open Rodhern.Kapoin.Helpers.Contracts
  open Rodhern.Kapoin.MainModule.Cache
  open Rodhern.Kapoin.MainModule.Data
  open Rodhern.Kapoin.MainModule.Data.KeyedDataNodeExtensions
  open Rodhern.Kapoin.MainModule.Events
  
  
  /// A type abbreviation for System.Double.
  /// An UTDay value is a double precision floating point value representing a
  /// time value, either an absolute value or a relative one, expressed in
  /// Kerbin days.
  /// Notice: UTDay is not annotated with unit of measure.
  type UTDay = float
  
  
  module Constants =
    
    /// Node for contract related persisted space center tracking data.
    let [< Literal >] public TrackNodeName = "CONTRACT_SRC_DATA"
    
    let [< Literal >] public TimeStampNodeName = "CHECK_SCHEDULE"
    let [< Literal >] public TimeStampStatus = "state"
    let [< Literal >] public TimeStampPrevCheck = "daychecked"
    let [< Literal >] public TimeStampNextCheck = "nextcheckdue"
    let [< Literal >] public TimeStampDefaultState = "N/A" // todo consider an enumeration value
    
    /// Node for static requirement check results
    /// in the form of earliest (open) contract offer windows.
    let [< Literal >] public StaticContractDataNodeName = "CONTRACT_REQUIREMENTS"
    
    let [< Literal >] public COWNodeName = "OFFER_SCHEDULE"
    let [< Literal >] public COWStateHibernate = "hibernate"
    let [< Literal >] public COWStateSchedule = "schedule"
    let [< Literal >] public COWStatus = "state"
    let [< Literal >] public COWFirstDay = "firstday"
    let [< Literal >] public COWLastDay = "lastday"
    let [< Literal >] public COWMaxOffered = "maxcount"
    let [< Literal >] public COWCachedCount = "cachedcount"
  
  
  [< AbstractClass >]
  /// A static class with a few functions
  /// used in the COW and SRC classes below.
  type SRCUtilClass =
    
    /// A temporary function to look up the current game time (UT)
    /// expressed in days.
    /// One (Kerbin) day is hardcoded as 6*60*60 = 21600 seconds.
    static member public Day
      with get (): UTDay =
        if not (assigned HighLogic.CurrentGame)
         then KapoinContractError.Raise "Unable to access 'CurrentGame'."
         else HighLogic.CurrentGame.UniversalTime / 21600.  + 1. // the game starts "day 1 time 0:00"
    
    /// The same as float2str, but with the result rounded to 6 decimals.
    static member Float2Str6 (v: float) =
      Math.Round (v, 6) |> float2str
    
    /// Write two integer pair value as a string.
    static member Pair2Str ((a,b): int * int) =
      sprintf "%0d / %0d" a b
    
    /// Read two integer values separated by a slash.
    static member Str2Pair (value: string): (int * int) option =
      if String.IsNullOrEmpty value
       then None else
      match value.Split [|'/'|]
            |> Array.map (fun s -> s.Trim ())
            |> Array.map str2int
       with
       | [| Some a; Some b |] -> Some (a, b)
       | _ -> None
    
    /// Check if ctype is a KapoinContract type, and return the name.
    static member public CTypeName (ctype: Type) =
      if ctype.IsSubclassOf typeof<KapoinContract>
       then ctype.Name
       else sprintf "Contract class '%s' was expected to be subclass of '%s', but was not."
                    ctype.Name typeof<KapoinContract>.Name
            |> KapoinContractError.Raise
    
    /// Count the number of contracts of the given type in the given states.
    /// Remark: Beacause we occasionally (awkwardly often actually) need to
    ///  get the contract count when the KSP contract system is not ready the
    ///  result takes the form of an option value.
    static member public ContractCount (ctype: Type, states: ContractState array) =
      if not (assigned Contracts.ContractSystem.Instance)
       then None
       else KapoinContract.ListKapoinContracts states
            |> List.filter (fun c -> c.GetType () = ctype)
            |> List.length
            |> Option.Some
    
//    /// Quote a list of items, invoking ToString on each.
//    static member public QuotedList (items: 'a list) =
//      let format = function
//        | null -> "null" // special rule for null values
//        | o -> o.ToString ()
//      let rec tostring = function
//      | [], txt -> txt
//      | s::ss, "" -> tostring (ss, sprintf "'%s'" (format s))
//      | s::ss, txt -> tostring (ss, sprintf "%s, '%s'" txt (format s))
//      tostring (items, "")
  
  
  open Constants
  
  /// Record part for the Hibernate option of a ContractOfferWindowNode.
  type COWHibernateRec =
    { /// DESC_MISS
      LastDay: UTDay } with
    
    /// TODO
    static member public Parse (node: KeyedDataNode) =
      let lastday = node.LookUpAndParse COWLastDay str2float
      if (lastday < 0.)
       then sprintf "Invalid hibernate parameter, lastday= %f." lastday
            |> KapoinContractError.Raise
      { LastDay= lastday }
    
    /// TODO
    member public record.UpdateNode (node: KeyedDataNode) =
      node.GetOrCreateValue COWStatus |> ignore
      node.values.[COWStatus] <- [ COWStateHibernate ]
      node.GetOrCreateValue COWLastDay |> ignore
      node.values.[COWLastDay] <- [ record.LastDay |> SRCUtilClass.Float2Str6 ]
  
  
  /// Record part for the Schedule option of a ContractOfferWindowNode.
  type COWScheduleRec =
    { /// DESC_MISS
      FirstDay: UTDay
      /// DESC_MISS
      LastDay: UTDay
      /// DESC_MISS
      MaxOffered: int
      /// Cache the offered contract count.
      /// It turns out that the contract requirements are often checked
      /// even at times when the KSP contract system is not ready
      /// (i.e. when Contracts.ContractSystem.Instance is null).
      /// The first int of the pair is the last known number of offered (and
      /// active) contracts of its type, the second int of the pair is the
      /// number of times the cached value has been 'queried'. The assumption
      /// is that as long as the sum of the two ints is less than 'MaxOffered'
      /// the contract count limit is not yet reached.
      CachedCount: (int * int) option } with
    
    /// TODO
    static member public Parse (node: KeyedDataNode) =
      let firstday = node.LookUpAndParse COWFirstDay str2float
      let lastday = node.LookUpAndParse COWLastDay str2float
      let maxoffered = node.LookUpAndParse COWMaxOffered str2int
      let cachedcount = KeyedDataNode.TryGetValue COWCachedCount (Some node)
                        |> Option.bind SRCUtilClass.Str2Pair
      if (firstday < 0.) || not (firstday < lastday) || (maxoffered < 1)
       then sprintf "Invalid schedule parameters, firstday= %f,lastday= %f, maxoffered= %d." firstday lastday maxoffered
            |> KapoinContractError.Raise
      match cachedcount
       with | Some (a, b) when (a < 0 || b < 0)
              -> sprintf "Invalid schedule parameters, cachedcount= %d / %d." a b
                 |> KapoinContractError.Raise
            | _ -> ()
      { MaxOffered= maxoffered; FirstDay= firstday; LastDay= lastday; CachedCount= cachedcount }
    
    /// TODO
    member public record.UpdateNode (node: KeyedDataNode) =
      // verify that none of the attributes are duplicates
      node.GetOrCreateValue COWStatus |> ignore
      node.GetOrCreateValue COWFirstDay |> ignore
      node.GetOrCreateValue COWLastDay |> ignore
      node.GetOrCreateValue COWMaxOffered |> ignore
      if record.CachedCount.IsSome
       then node.GetOrCreateValue COWCachedCount |> ignore
       else node.values.Remove COWCachedCount |> ignore
      // then fill in each of the fields
      node.values.[COWStatus] <- [ COWStateSchedule ]
      node.values.[COWFirstDay] <- [ record.FirstDay |> SRCUtilClass.Float2Str6 ]
      node.values.[COWLastDay] <- [ record.LastDay |> SRCUtilClass.Float2Str6  ]
      node.values.[COWMaxOffered] <- [ int2str record.MaxOffered ]
      if record.CachedCount.IsSome
       then node.values.[COWCachedCount] <- [ record.CachedCount.Value |> SRCUtilClass.Pair2Str ]
  
  
  /// Static requirement check result node with information about
  /// the time window of an offered contract.
  /// Remark: The 'SRCTimeStampRec' and 'ContractOfferWindowNode' records are
  ///  not all that helpful; they are here to get data into the sfs file, so
  ///  that we may better discover how well the ideas work.
  type ContractOfferWindowNode =
    /// Serves the same purpose as None, but with a different name.
    | NoNode
    /// DESC_MISS
    | Hibernate of COWHibernateRec
    /// DESC_MISS
    | Schedule of COWScheduleRec
    with
    
    /// Try to fetch the value of each of the record fields from
    /// a keyed data node. If the values, or the node, do not exist
    /// the result is interpreted as NoNode.
    static member public Parse (nodeopt: KeyedDataNode option) =
      match KeyedDataNode.TryGetValue COWStatus nodeopt with
      | None -> NoNode
      | Some COWStateHibernate -> Hibernate (COWHibernateRec.Parse nodeopt.Value)
      | Some COWStateSchedule -> Schedule (COWScheduleRec.Parse nodeopt.Value)
      | Some other -> sprintf "Unable to parse ContractOfferWindowNode; unknown status value '%s'." other
                      |> KapoinContractError.Raise
    
    /// Update a keyed data (sub)node with the values of the
    /// ContractOfferWindowNode fields.
    /// Notice: This will not work in the NoNode case.
    member public cownode.UpdateNode (node: KeyedDataNode) =
      match cownode with
      | NoNode -> "Unable to update node in the 'NoNode' case;"
                  + " such an update requires the node to be deleted."
                  |> KapoinContractError.Raise
      | Hibernate record -> record.UpdateNode node
      | Schedule record -> record.UpdateNode node
    
    /// Try to fetch the value for the record fields
    /// from the default cached data module ('KapoinMainNode').
    /// The contract type parameter is used to find the proper subnode.
    static member public GetContractData (ctype: Type) =
      let csubnodename = SRCUtilClass.CTypeName ctype
      KeyedDataNode.TryGetTopnode<KapoinMainNode> ()
      |> KeyedDataNode.TryGetSubnode StaticContractDataNodeName
      |> KeyedDataNode.TryGetSubnode csubnodename
      |> KeyedDataNode.TryGetSubnode COWNodeName
      |> ContractOfferWindowNode.Parse
    
    /// Update, or create, a node with the record values 
    /// in the default cached data module ('KapoinMainNode').
    /// The contract type parameter is used to name the subnode.
    /// In the NoNode case the mentioned node is deleted rather than created.
    member public cownode.SaveContractData (ctype: Type) =
      let topnodeopt = KeyedDataNode.TryGetTopnode<KapoinMainNode> ()
      if topnodeopt.IsNone then false else
      let srcnode = topnodeopt.Value.GetOrCreateSubNode StaticContractDataNodeName
      let cnodename = SRCUtilClass.CTypeName ctype
      match cownode with
      | Hibernate _
      | Schedule _
        -> let csubnode = srcnode.GetOrCreateSubNode cnodename
           let recnode = csubnode.GetOrCreateSubNode COWNodeName
           do cownode.UpdateNode recnode
           true
      | NoNode ->
        match Some srcnode |> KeyedDataNode.TryGetSubnode cnodename with
        | None -> false
        | Some csubnode ->
          do csubnode.nodes.Remove COWNodeName |> ignore // delete COW record(s)
          if csubnode.IsEmpty
           then srcnode.nodes.Remove cnodename |> ignore // if csubnode is empty then delete that as well
          true
    
    /// Method to downgrade COW node from Schedule to Hibernate
    /// if the contract offer count is already reached.
    /// Returns true if the offer count is strictly less than the limit.
    static member public CheckOfferCount (ctype: Type, ?schedulerec: COWScheduleRec) =
      let ctypename = SRCUtilClass.CTypeName ctype
      let srecopt =
        match schedulerec with
        | Some srec -> Some srec // if a record was already given use that
        | None -> // otherwise load the one from the main node if one is there
          match ContractOfferWindowNode.GetContractData ctype with
          | Schedule srec -> Some srec
          | _ -> None
      if srecopt.IsNone then false else // no schedule record, so nothing to do
      let { LastDay= lastday; MaxOffered= maxoffered; CachedCount= cachedcountopt } as srec = srecopt.Value
      let offercountopt = SRCUtilClass.ContractCount (ctype, [| ContractState.Offered; ContractState.Active |])
      let checkresult, updatednode =
        match offercountopt, cachedcountopt with
        | Some offercount, _ when offercount < maxoffered
          -> true, // limit not yet reached (write actual offer count to cache)
              Some <| Schedule { srec with CachedCount= Some (offercount, 1) }
        | Some offercount, _
          -> false, // downgrade COW node to Hibernate
              Some <| Hibernate { LastDay= lastday }
        | None, Some (a, b) when a + b < maxoffered
           -> true, // we deduce from cached values that the limit is not yet reached
               Some <| Schedule { srec with CachedCount= Some (a, b + 1) }
        | None, Some _
           -> false, None // possibly we are already at the limit
        | None, None
           -> "Contract system instance not available"
              + sprintf " and contract offer count for '%s' is not cached." ctypename
              |> LogWarn
              false, None // we are out of options, we choose to return 'false' as the result
      if updatednode.IsSome then
        let savedOk = updatednode.Value.SaveContractData ctype
        if not savedOk then
          sprintf "Unable to update COW node for contracts of type '%s'." ctypename
          |> LogWarn
      checkresult
    
    /// Implementation example:
    /// Let contracts check the SRC result node.
    static member public IsOfferWindowOpen (c: KapoinContract) =
      let closed (msg: string) = false
      let now = SRCUtilClass.Day
      match ContractOfferWindowNode.GetContractData (c.GetType ()) with
      | NoNode ->
        closed "No contract offer window record for contract."
      | Hibernate { LastDay= lastday } when lastday < now ->
        closed <| sprintf "Contract offer hibernation elapsed; lastday= %.4f, now= %.4f." lastday now
      | Hibernate _ ->
        closed "Contract offer in hibernation."
      | Schedule ({ FirstDay= firstday; LastDay= lastday; MaxOffered= maxoffered } as srec) ->
        if now < firstday
         then closed "Contract offer window not yet reached."
        elif now > lastday
         then closed <| sprintf "Contract offer window expired; lastday= %.4f, now= %.4f." lastday now
        elif ContractOfferWindowNode.CheckOfferCount (c.GetType (), srec) |> not
         then closed "Maximum number of offered contracts of this type reached."
         else true // the offer window is open; go ahead and offer the contract.
    
    /// Helper function that can be used for standard implementation of
    /// "KapoinContract.MeetRequirements ()" in custom contract classes.
    /// This function looks up the SRC result in the Kapoin main data node
    /// and returns if the relevant contract offer window is currently open.
    static member public AreRequirementsMet (c: KapoinContract) =
      if    c.ContractState = ContractState.Offered
         || c.ContractState = ContractState.Active then
        true // requirements are checked even for already offered and active contracts !
      elif  not IndexBoard.Ready
         || not MainKapoinLoop.LoopStateIsRunning
         || not (IndexBoard.Instance.ContainsRef<KapoinMainNode> ()) then
        false // if Kapoin is not running do not offer contracts not already in play
      else
        ContractOfferWindowNode.IsOfferWindowOpen c
  
  
  /// TODO - Not the most intuitive of parameter sets; but it will do for now.
  type ContractWindowScheduleParams =
    { // todo rename all of these constants.
      initialoffset: UTDay
      lowwaitopen: UTDay
      cwaitopen: float
      highwaitopen: UTDay
      lowwaitclose: UTDay
      cwaitclose: float
      highwaitclose: UTDay
      maxoffered: int } with
    
    /// TODO
    /// Temporary implementation, which is basically just a loop.
    member public p.GetSchedule (ctype: Type, now: UTDay): COWScheduleRec =
      let forwardopen (t: UTDay) =
        t + p.lowwaitopen + (min (p.cwaitopen * t) p.highwaitopen)
      let forwardclose (t: UTDay) =
        t + p.lowwaitclose + (min (p.cwaitclose * t) p.highwaitclose)
      let mutable tfirst: UTDay = p.initialoffset
      let mutable tlast: UTDay = forwardclose tfirst
      while now > tlast do
        tfirst <- forwardopen tlast
        tlast <- forwardclose tfirst
      let cachedcount =
        SRCUtilClass.ContractCount (ctype, [| ContractState.Offered; ContractState.Active |])
        |> Option.map (fun k -> k, 0)
      { FirstDay= tfirst; LastDay= tlast; MaxOffered= p.maxoffered; CachedCount= cachedcount }
  
  
  /// DESC_MISS
  type SRCIntervalParams =
    { /// DESC_MISS - Delta in days.
      delta: UTDay }
  
  
  /// DESC_MISS
  type SRCheckResult =
    /// DESC_MISS
    | Remove_Todo
    /// DESC_MISS
    | Wait_Todo of SRCIntervalParams
    /// DESC_MISS
    | Schedule_Todo of ContractWindowScheduleParams * SRCIntervalParams
  
  
  /// The SRC action is given two parameters when it is invoked. Often the
  /// parameters are ignore, but some times it may be convenient to know the
  /// present COW state and UT time (in days) before updating the COW state.
  /// The return value is an SRCheckResult that describes what the SRC action
  /// wants to do with the time stamp and COW result nodes.
  type SRCheckAction = ContractOfferWindowNode * UTDay -> SRCheckResult
  
  
  /// Static requirement check time stamp for SRCs triggered in the main loop.
  /// Remark: The time stamp records are, at least at the moment, direct
  ///  subnodes of the scenario CONTRACT_SRC_DATA node. Either we find a
  ///  different spot for 'local variable' contract data, or possibly we could
  ///  do better if we expand the space center tracking data time stamp record
  ///  functionality with more generic data access support.
  ///  However, also remember that some 'local variable' contract data may just
  ///  have to reside in the Kapoin main data node, if, for instance, the data
  ///  is needed in other game scenes.
  type SRCTimeStampRec =
    { /// DESC_MISS
      Status: string; // not used per se
      /// DESC_MISS
      PrevCheck: UTDay;
      /// DESC_MISS
      NextCheck: UTDay } with
    
    /// Create a SRCTimeStampRec using the current time (UT) to mark
    /// PrevCheck and set NextCheck in the future, by the amount specified
    /// in intervalparam.
    static member public New (intervalparam: SRCIntervalParams) =
      if intervalparam.delta > 0.
       then ()
       else invalidArg "intervalparam" "SRC interval parameter must be positive."
      let now = SRCUtilClass.Day
      { Status= TimeStampDefaultState; PrevCheck= now; NextCheck= now + intervalparam.delta }
    
    /// Try to fetch the value of each of the record fields from a keyed data
    /// node. If the values, or the node, do not exist the result is None.
    static member public TryParse (nodeopt: KeyedDataNode option) =
      let lookup = fun s -> KeyedDataNode.TryGetValue s nodeopt
      let statusopt = lookup TimeStampStatus
      let prevcheckopt = lookup TimeStampPrevCheck
                         |> Option.bind str2float
      let nextcheckopt = lookup TimeStampNextCheck
                         |> Option.bind str2float
      match statusopt, prevcheckopt, nextcheckopt with
      | None, None, None ->
        None
      | Some status, Some prevcheck, Some nextcheck ->
        Some { Status= status; PrevCheck= prevcheck; NextCheck= nextcheck }
      | _ ->
        let optX (opt: 'a option) = if opt.IsSome then "X" else "-"
        "Some but not all SRCTimeStampRec fields are present in keyed data node;"
         + sprintf " (%s %s; %s %s; %s %s)."
                   TimeStampStatus (optX statusopt)
                   TimeStampPrevCheck (optX prevcheckopt)
                   TimeStampNextCheck (optX nextcheckopt)
        |> LogError
        None
    
    /// Write the record field values to a keyed data node.
    member public record.UpdateNode (node: KeyedDataNode) =
      // verify that none of the attributes are duplicates
      node.GetOrCreateValue TimeStampStatus |> ignore
      node.GetOrCreateValue TimeStampPrevCheck |> ignore
      node.GetOrCreateValue TimeStampNextCheck|> ignore
      // then fill in each of the three fields
      node.values.[TimeStampStatus] <- [ record.Status ]
      node.values.[TimeStampPrevCheck] <- [ record.PrevCheck |> SRCUtilClass.Float2Str6 ]
      node.values.[TimeStampNextCheck] <- [ record.NextCheck |> SRCUtilClass.Float2Str6 ]
    
    /// Access the cached tracking data (KapoinSpaceCenterTrackingData)
    /// and try to fetch the record field values.
    /// The contract type parameter is used to find the proper subnode.
    static member public TryGetContractData (ctype: Type) =
      let csubnodename = SRCUtilClass.CTypeName ctype
      KeyedDataNode.TryGetTopnode<KapoinSpaceCenterTrackingData> ()
      |> KeyedDataNode.TryGetSubnode TrackNodeName
      |> KeyedDataNode.TryGetSubnode csubnodename
      |> KeyedDataNode.TryGetSubnode TimeStampNodeName
      |> SRCTimeStampRec.TryParse
    
    /// Access the cached tracking data (KapoinSpaceCenterTrackingData)
    /// and create or update the time stamp record field values.
    /// The contract type parameter is used to find the proper subnode.
    member public record.SaveContractData (ctype: Type) =
      match KeyedDataNode.TryGetTopnode<KapoinSpaceCenterTrackingData> () with
      | None -> false
      | Some topnode
        -> let csubnodename = SRCUtilClass.CTypeName ctype
           let tracknode = topnode.GetOrCreateSubNode TrackNodeName
           let cnode = tracknode.GetOrCreateSubNode csubnodename
           let timestampnode = cnode.GetOrCreateSubNode TimeStampNodeName
           do record.UpdateNode timestampnode
           true
    
    /// Create or update the space center tracking data time stamp for the
    /// given custom contract class, e.g. to create an initial time stamp.
    /// Note: Unless 'overwrite' is true (the default is false) this function
    ///  will not update an existing time stamp.
    /// Note: This function will only work while the Kapoin Space Center
    ///  tracking data (KapoinSpaceCenterTrackingData) is registered in cache.
    member public timestamp.UpdateTimeStampNode (ctype: Type, ?overwrite: bool) =
      if (defaultArg overwrite false) ||
         (SRCTimeStampRec.TryGetContractData ctype).IsNone
       then timestamp.SaveContractData ctype
       else false
    
    /// Create the space center tracking data time stamp.
    /// If the time stamp node already exists then do nothing.
    /// The default values used are to set the previous check time to 'now'
    /// and the next check time 0.001 days (approx. 22 secs.) in the future.
    static member public CreateTimeStampNode (ctype: Type) =
      let timestamp = SRCTimeStampRec.New { delta= 0.001 }
      timestamp.UpdateTimeStampNode ctype
    
    /// Access the cached tracking data (KapoinSpaceCenterTrackingData)
    /// and delete existing time stamp node(s) for the given contract type.
    /// The contract type parameter is used to find the proper subnode.
    static member public DeleteTimeStampNode (ctype: Type) =
      let topnodeopt = KeyedDataNode.TryGetTopnode<KapoinSpaceCenterTrackingData> ()
      if topnodeopt.IsNone then false else
      let tracknode = topnodeopt.Value.GetOrCreateSubNode TrackNodeName
      let cnodename = SRCUtilClass.CTypeName ctype
      if not (tracknode.nodes.ContainsKey cnodename) then false else
      let cnode = tracknode.GetOrCreateSubNode cnodename
      if not (cnode.nodes.ContainsKey TimeStampNodeName) then false else
      do cnode.nodes.Remove TimeStampNodeName |> ignore // delete timestamp subnode(s)
      if cnode.IsEmpty
       then tracknode.nodes.Remove cnodename |> ignore // if cnode is empty then delete that as well
      true
  
  
  type SRCheckResult with // extend SRCheckResult with static WriteResult method
    
    /// Implementation example:
    /// Update SRC result and time stamp if an SRC is due.
    ///
    /// Each contract class will typically have its own SRC method found via
    /// the StaticRequirementCheckAttribute. Once an SRC method has determined
    /// wheter the static requirements for that particular contract class is
    /// met or not, the SRC method must disseminate the information so that it
    /// is available to all potential contract instances.
    ///
    /// Standard practice is to write the SRC result to the Kapoin main data
    /// node (KapoinMainNode), and to use the Kapoin Space Center tracking
    /// data (KapoinSpaceCenterTrackingData) for 'local variable' values.
    /// If the static requirements are not (yet) met, it is customary to leave
    /// that SRC result out of the main data node though.
    /// 
    /// This function is a helper function that can be used by SRC methods to
    /// write SRC results to the Kapoin main data node. The SRC method provide
    /// an SRC action to this function, the action is then triggered, but only
    /// if the SRC is due according to the space center tracking data time
    /// stamp for the given contract class.
    /// Note that the first space center tracking data time stamp cannot be
    /// created by the SRC action itself.
    static member public WriteResult (ctype: Type) (checkaction: SRCheckAction) =
      let ctypename = SRCUtilClass.CTypeName ctype
      let timestamp = SRCTimeStampRec.TryGetContractData ctype
      let cownode = ContractOfferWindowNode.GetContractData ctype
      let now = SRCUtilClass.Day
      
      /// TODO
      let performcheck () =
        let result = checkaction (cownode, now) // ask the SRC action what should be done
        match result with
        | Remove_Todo ->
          SRCTimeStampRec.DeleteTimeStampNode (ctype) |> ignore // cancel periodic SRCs
          NoNode.SaveContractData (ctype) |> ignore // implicitly assume NoNode
        | Wait_Todo intervalparams ->
          let newTimeStamp = SRCTimeStampRec.New intervalparams
          newTimeStamp.SaveContractData (ctype) |> ignore
          NoNode.SaveContractData (ctype) |> ignore // NoNode is used for NOT scheduling an open contract offer window
        | Schedule_Todo (scheduleparams, intervalparams) ->
          let mutable newTimeStamp = SRCTimeStampRec.New intervalparams
          let newSchedule = scheduleparams.GetSchedule (ctype, now)
          if newSchedule.LastDay + 0.0001 < newTimeStamp.NextCheck // don't let the next check wait for too long
           then newTimeStamp <- { newTimeStamp with NextCheck= newSchedule.LastDay }
          let newCOWNode =
            let offeredcount =
              match SRCUtilClass.ContractCount (ctype, [| ContractState.Offered; ContractState.Active |]), cownode with
              | Some k, _ -> k // this is the regular case
              | None, Schedule { CachedCount= Some (a, b) }
                -> LogWarn <|
                    "Contract system instance not available;"
                    + sprintf " using cached values '%d / %d' in lieu of an actual count." a b
                   a + b
              | None, _
                -> LogError <|
                    "Contract system instance not available;"
                    + sprintf " arbitrarily choosing 'Schedule' over 'Hibernate' for '%s'." ctypename
                   0
            if offeredcount >= newSchedule.MaxOffered
             then Hibernate { LastDay= newSchedule.LastDay }
             else Schedule newSchedule
          newTimeStamp.SaveContractData (ctype) |> ignore
          newCOWNode.SaveContractData (ctype) |> ignore
      
      match timestamp, cownode with
      | Some { NextCheck= due }, NoNode when now > due ->
        performcheck ()
      | Some { NextCheck= due }, Hibernate { LastDay= lastday } when now > due || now > lastday ->
        performcheck ()
      | Some { NextCheck= due }, Schedule { LastDay= lastday } when now > due || now > lastday ->
        performcheck ()
      | None, Hibernate _
      | None, Schedule _ ->
        sprintf "A contract offer window node for '%s' exists," ctypename
        + " but the corresponding space center tracking record is missing."
        + " This is not a good situation for a static requirement check."
        + sprintf " Loaded scene is '%A' and the time (UT in days) is %.4f." HighLogic.LoadedScene now
        |> LogWarn
      | _ -> () // SRC is not yet due
  
