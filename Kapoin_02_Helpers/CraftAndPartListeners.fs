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
  
  
  /// An interface that allows a parent part or vessel module to expose the
  /// filtered data and trace logger components of its listener object.
  type IFilteredListenerData =
    /// Unique logging identifier.
    abstract member Uid: Guid with get
    /// Access to filtered data.
    abstract member FilteredData: KeyedDataNode
    /// Log method for trace listeners and KSP log file.
    abstract member LogFn: (string -> unit)
  
  
  /// The listener object is a relative to the keyed data and logger node.
  /// Listener objects are owned by part modules (PartModule descendants) and
  /// vessel modules (VesselModule descendants).
  ///
  /// The parent may implement IFilteredListenerData by forwarding to the
  /// interface of the listener object.
  /// 
  /// In the PartModule or VesselModule descendant include a Listener and hook
  /// up the part or vessel module methods by forwarding them to the Listener.
  ///
  /// Remark: Only create a VesselModule descendant in the main assembly,
  ///  because as soon as KSP discovers a VesselModule descendant the class is
  ///  used for 'all' vessels (including unloaded asteroids).
  /// 
  /// Remark: The listener does the same thing for PartModule as for VesselModule.
  type Listener< 'T when 'T :> MonoBehaviour and 'T :> IFilteredListenerData > () =
    
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
    
    /// Initialize listener data and trace logger.
    /// The parent reference is passed so that it may later be registered
    /// in cache using the listener data uid as its cache key.
    /// The isRunning method should determine if Kapoin is running or not.
    /// You do not have to pass a set of persisted keys,
    /// but it may improve performance to create a (class) static set.
    /// Remark: Initialize the listener as early as possible; the embedded
    ///  trace and debug logger discards all messages until initialized.
    member public listener.Initialize (parentmodule: 'T, isRunning: unit -> bool, ?persistedkeys: Set<string>) =
      let persistedkeys = match persistedkeys with Some keys -> keys | None -> Listener<'T>.KSPFieldNames () // avoid defaultArg
      parent <- Some parentmodule
      runpredicate <- new Predicate<unit> (isRunning)
      data.Initialize (typeof<'T>, persistedkeys)
    
    /// Uninitialize will unregister the parent from cache (if previously
    /// registered) and let go of the explicit parent reference.
    /// The data and logger node is not affected and can still be used
    /// after the listener object has been uninitialized.
    member public listener.Uninitialize () =
      if cached then listener.Unregister ()
      parent <- None
      runpredicate <- null
    
    /// Register the parent module in the cache.
    /// Typically invoked when parent module is enabled.
    member public listener.Register () =
      if (not cached) && (parent.IsSome) && (KapoinCache.Ready) then
        let cacheinstance = KapoinCache.GetInstance ()
        let running = if assigned runpredicate
                       then runpredicate.Invoke ()
                       else false
        if running then
          cacheinstance.AddRef (parent.Value, listener.Uid.ToString ())
          cached <- true
    
    /// Unregister the parent module (i.e. remove it from cache).
    /// Typically invoked when parent module is disabled.
    member public listener.Unregister () =
      if (cached) && (KapoinCache.Ready) then
        let cacheinstance = KapoinCache.GetInstance ()
        cacheinstance.RemoveRef<'T> (listener.Uid.ToString ())
        cached <- false
    
    /// To be used in parent module's OnLoad .
    member public listener.OnLoad (node: ConfigNode) =
      data.OnLoad node
    
    /// To be used in parent module's OnSave .
    member public listener.OnSave (node: ConfigNode) =
      data.OnSave node
    
    /// Use reflection (on 'T) and return a set of the names
    /// of the fields marked with a 'KSPField' attribute.
    static member public KSPFieldNames () =
      let isKSPField (info: FieldInfo) =
        let attribs = info.GetCustomAttributes (typeof<KSPField>, false)
        attribs.Length > 0
      typeof<'T>.GetFields ()
      |> Array.filter isKSPField
      |> Array.map (fun info -> info.Name)
      |> Set.ofArray
    
    // Implemented here so that it is easy for the parent to get at
    // an IFilteredListenerData implementation.
    interface IFilteredListenerData with
      member listener.Uid = listener.Uid
      member listener.FilteredData = listener.FilteredData
      member listener.LogFn = listener.LogFn
  
