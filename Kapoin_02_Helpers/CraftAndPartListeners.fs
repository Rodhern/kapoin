// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.FlightModules
  
  open System
  open System.Collections.Generic
  open System.Reflection
  open UnityEngine
  open Rodhern.Kapoin.Helpers
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  open Rodhern.Kapoin.Helpers.ScenarioData
  
  
  /// Specialized filtering variation of DataAndLoggerNode,
  /// used to persist data for craft and part listeners.
  type FilteredListenerData () =
    inherit FilteringDataNode ()
    
    /// The set of fields (keys) that are marked with the KSPField attribute.
    /// Those fields are not supposed to reside in 'Data'; they are maintained
    /// by Module Manager.
    let mutable kspkeyfieldset: Set<string> = Set.empty
    
    /// Filter that weeds out first level values that are marked with the
    /// KSPField attribute.
    override data.DataFilter { values= values; nodes= nodes } =
      if kspkeyfieldset.IsEmpty
       then None
       else let fvals = new SortedList<string, string list> ()
            let mutable modified = false
            for key in values.Keys
             do if kspkeyfieldset.Contains key
                 then modified <- true
                 else fvals.Add (key, values.[key])
            if modified
             then Some { values= fvals; nodes= nodes }
             else None
    
    /// Initialize trace logger and data filter.
    /// Notice: In the FilteredListenerData case the 'persistedkeys' are those
    ///  keys that the data node should ignore. I.e. roughly the opposite of
    ///  the FilteredContractData case.
    member public data.Initialize (parenttype: Type, persistedkeys: Set<string>) =
      data.InitTraceLogger parenttype
      kspkeyfieldset <- persistedkeys
    
    /// To be used in parent module's OnLoad .
    member public data.OnLoad (cnode: ConfigNode) =
      if not data.KeyedData.IsEmpty
       then let msg = "Clear keyed data node; data present in node at load."
            data.LogFn msg // sending (duplicate) message to specific log will ...
            LogWarn msg // ... help to find the source of the problem
      let unfiltereddata = KeyedDataNode.CopyConfigNode cnode
      let filtereddata =
        match data.DataFilter unfiltereddata with
        | None -> unfiltereddata // turns out nothing was removed
        | Some fdata
          -> sprintf "Filtered load; %d nodes and %d distinct keys loaded (a further %d keys removed by filter)."
                     fdata.nodes.Count fdata.values.Count (unfiltereddata.values.Count - fdata.values.Count)
             |> data.LogFn
             fdata
      data.KeyedData <- filtereddata
    
    /// To be used in parent module's OnSave .
    member public data.OnSave (cnode: ConfigNode) =
      match data.DataFilter data.KeyedData with
      | Some _ -> KapoinPersistenceError.Raise "Save failed; persisted fields are duplicated in keyed data node."
      | None -> data.KeyedData.TrimAndSaveKeyedData cnode // note that the data is effectively filtered a second time at this point
  
  
  /// DESC_MISS
  type Listener< 'T when 'T :> MonoBehaviour > () =
    
    /// Data and trace logger for listener's parent module.
    let data = new FilteredListenerData ()
    
    /// A back-reference to the parent module.
    /// The back reference is initialized in .Initialize.
    let mutable parent: 'T option = None
    
    /// A function that will check if Kapoin is enabled and running.
    /// The predicate is initialized in .Initialize.
    let mutable runpredicate: Predicate<unit> = null
    
    /// Keeps track of the register / unregister state of the parent module.
    let mutable cached = false
    
    /// Write message to trace listeners and KSP log file.
    member public listener.LogFn = data.LogFn
    
    /// Unique logging identifier.
    member public listener.Uid = data.Uid
    
    /// Access to keyed data.
    member public listener.FilteredData = data.KeyedData
    
    // TODO - implement .Initialize et cetera
  
