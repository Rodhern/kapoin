﻿// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Data
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  open Rodhern.Kapoin.Helpers.GameSettings
  open Rodhern.Kapoin.Helpers.ScenarioData
  open Rodhern.Kapoin.MainModule.Cache
  
  
  module Constants =
    
    [< Literal >]
    let public KSCTrackDataModuleName = "KapoinSpaceCenterTrackingData" // i.e. typeof<KapoinSpaceCenterTrackingData>.Name
    
    [< Literal >]
    let public KapoinMainNode = "KAPOIN_CAREER_DATA" // name of the node type itself
    
    [< Literal >]
    let public DataverKey = "dataver"
    
    [< Literal >]
    let public DataverDisabled = "off" // a curiosity of the F# language requires a variable name beginning with a capital letter in order to use the literal in pattern matching
    
    let public CurrentDataver: VersionTuple = 0,0,2,1
  
  
  open Constants
  
  /// A simple interface that denotes
  /// that the class contains a KeyedData property.
  type IKeyedData =
    abstract member KeyedData: KeyedDataNode
  
  
  [< KSPScenario (ScenarioCreationOptions.AddToNewCareerGames, [| GameScenes.SPACECENTER |]) >]
  /// DESC_MISS
  type KapoinSpaceCenterTrackingData () =
    inherit ScenarioDataModule ()
    
    /// DESC_MISS / method to be rewritten
    member public data.UpdateDataVer () = // currently verbose
      if data.KeyedData.values.ContainsKey DataverKey then
        match data.KeyedData.values.[DataverKey] with
        | []
          -> data.LogFn "Error: Empty data format version list (internal error - should never happen)." // TODO
        | _::_::_
          -> data.LogFn <| sprintf "Error: Multiple '%s' entries." DataverKey // TODO
        | [DataverDisabled]
          -> data.LogFn "Data format stamp marked disabled."
        | [verstr]
          -> match str2ver verstr with
             | None
               -> data.LogFn <| sprintf "Error: Unable to parse '%s'." verstr
             | Some ver
               -> if ver = CurrentDataver then
                    data.LogFn <| sprintf "Data format stamp present and up to date."
                   else
                    data.LogFn <| sprintf "Update data format from ver. %s to %s." (ver2str ver) (ver2str CurrentDataver)
                    data.LogFn "TODO: Data format update not yet implemented."
       else
        let dataver = ver2str CurrentDataver
        data.LogFn <| sprintf "Create new key (%s = %s)." DataverKey dataver
        data.KeyedData.values.Add (DataverKey, [ dataver ])
    
    /// DESC_MISS / method to be rewritten
    member public data.EnableDisableDataVer () = // currently verbose
      let verstr = data.KeyedData.values.[DataverKey].Head
      let track = (HighLogic.CurrentGame.Parameters.CustomParams<KapoinParameterNode> ()).TrackProgress
      match verstr, track with
      | DataverDisabled, true
        -> data.LogFn "Enable tracking data (tracking is turned on in game settings)."
           data.KeyedData.values.[DataverKey] <- [ver2str CurrentDataver]
      | verstr, false when (str2ver verstr) = Some CurrentDataver
        -> data.LogFn "Disable tracking data (tracking is turned off in game settings)."
           // todo: what do we do with the 'old' KeyedData other than the DataverKey ?
           data.KeyedData.values.[DataverKey] <- [DataverDisabled]
      | _ -> () // nothing to be done
    
    /// DESC_MISS
    member public data.Start () =
      data.UpdateDataVer ()
      data.EnableDisableDataVer ()
    
    /// DESC_MISS
    member internal data.OnEnable () =
      base.OnEnable ()
      IndexBoard.Instance.AddRef data
    
    /// DESC_MISS
    member internal data.OnDisable () =
      IndexBoard.Instance.RemoveRef<KapoinSpaceCenterTrackingData> ()
      base.OnDisable ()
    
    interface IKeyedData with
      member data.KeyedData = data.KeyedData
  
  
  /// DESC_MISS
  type KapoinMainNode () =
    // We could inherit from 'ScenarioDataModule', but instead we have chosen
    // the "Common DataAndLoggerNode container implementation" route.
    
    static let [< Literal >] Hard = true
    static let [< Literal >] Soft = false
    
    /// Return first level subnodes that are Kapoin main nodes.
    static let filteredsubnodes (topnode: ConfigNode) =
      [ for node in topnode.nodes
         do if node.name = Constants.KapoinMainNode
             then yield node ]
    
    // #region DataAndLoggerNode container implementation
    // ---- ---- ---- ---- ---- ----
    
    /// The data and logger pair.
    let data = new DataAndLoggerNode ()
    
    // Initialize trace logger.
    // Warning: A hardcoded type argument is only sensible for classes
    //  that will not be used as base class for descendants.
    do data.InitTraceLogger (typeof<KapoinMainNode>)
    
    /// Write message to trace listeners and KSP log file.
    member public mainnode.LogFn = data.LogFn
    
    /// Unique logging identifier.
    member public mainnode.Uid = data.Uid
    
    /// Access to keyed data.
    member public mainnode.KeyedData = data.KeyedData
    
    // ---- ---- ---- ---- ---- ----
    // #endregion
    
    /// DESC_MISS
    static member public ResetNode (hard: bool) =
      if IndexBoard.Ready then // otherwise there is no work to be done
        let cache = IndexBoard.Instance
        match hard, cache.ContainsRef<KapoinMainNode> () with
        | Hard, true -> cache.RemoveRef<KapoinMainNode> () // Hard Reset
        | Hard, false -> () // no work to be done
        | Soft, true -> (cache.GetRef<KapoinMainNode> ()).Clear () // Soft Reset
        | Soft, false -> cache.AddRef<KapoinMainNode> (new KapoinMainNode ()) // Soft initialize
    
    /// DESC_MISS
    static member public LoadNode (topnode: ConfigNode) =
      if not IndexBoard.Ready then
        LogWarn <| sprintf "Cannot load '%s' node; Kapoin cache not yet ready." Constants.KapoinMainNode
       else
        KapoinMainNode.ResetNode Soft // First, soft reset the node.
        match filteredsubnodes topnode with // Load node if exactly one Kapoin main node is found.
        | [] -> () // nothing to do, soft reset was already done
        | [datanode] -> (IndexBoard.Instance.GetRef<KapoinMainNode> ()).OnLoad datanode
        | _ -> LogError <| sprintf "Multiple '%s' nodes encountered during load; cache node reset." Constants.KapoinMainNode
    
    /// DESC_MISS
    static member public SaveNode (topnode: ConfigNode) =
      if (not IndexBoard.Ready) || (not (IndexBoard.Instance.ContainsRef<KapoinMainNode> ())) then
        LogError <| sprintf "Cannot save '%s' node; cache is uninitialized or empty." Constants.KapoinMainNode
       else
        let datanode = topnode.AddNode Constants.KapoinMainNode // always assume we are served a pristine topnode
        do (IndexBoard.Instance.GetRef<KapoinMainNode> ()).OnSave datanode
    
    /// Clear the keyed data. All existing keyed data is discarded.
    member public mainnode.Clear () =
      mainnode.LogFn "Clear keyed data."
      data.KeyedData <- KeyedDataNode.EmptyNode
    
    /// This is a near verbatim copy of ScenarioDataModule.OnLoad .
    member public mainnode.OnLoad (cnode: ConfigNode) =
      mainnode.LogFn <| sprintf "OnLoad (source node named '%s')." cnode.name
      if mainnode.KeyedData.IsEmpty
       then data.KeyedData <- KeyedDataNode.CopyConfigNode cnode
       else "Data already present in data module."
            |> KapoinPersistenceError.Raise
    
    /// This is a near verbatim copy of ScenarioDataModule.OnSave .
    member public mainnode.OnSave (cnode: ConfigNode) =
      mainnode.LogFn <| sprintf "OnSave (destination node named '%s')." cnode.name
      if cnode.HasData
       then LogWarn "Data already present in destination node."
      if mainnode.KeyedData.IsEmpty
       then // Even though it is a bad omen when the keyed data is empty at
            // save time, it seems to happen regularly when the flight scene
            // is loading. So let us skip 'LogWarn' in favour of 'LogFn'.
            mainnode.LogFn "No data present in data module."
       else mainnode.KeyedData.SaveKeyedData cnode
    
    interface IKeyedData with
      member data.KeyedData = data.KeyedData
  
  
  /// DESC_MISS
  module KeyedDataNodeExtensions =
    
    type KeyedDataNode with
      
      /// Function to create a named distinct subnode,
      /// or if it already exists simply look it up.
      /// Note: An exception is raised if more than one subnode
      ///  match the name given.
      member public node.GetOrCreateSubNode name =
        if not (node.nodes.ContainsKey name) then
          node.nodes.Add (name, [])
        if node.nodes.[name].IsEmpty then
          node.nodes.[name] <- [ KeyedDataNode.EmptyNode ]
        match node.nodes.[name] with
        | [ node ] -> node
        | _ -> sprintf "Failed to locate distinct subnode named '%s'." name
               |> KapoinPersistenceError.Raise
      
      /// Function to create a named distinct value property,
      /// or if it already exists simply look it up.
      /// Note: An exception is raised if more than one value property
      ///  match the name given.
      member public node.GetOrCreateValue name =
        if not (node.values.ContainsKey name) then
          node.values.Add (name, [])
        if node.values.[name].IsEmpty then
          node.values.[name] <- [ "" ]
        match node.values.[name] with
        | [ value ] -> value
        | _ -> sprintf "Failed to locate distinct value property named '%s'." name
               |> KapoinPersistenceError.Raise
      
      /// Function to look up a specific KeyedData parent
      /// cached in the IndexBoard cache.
      /// The KeyedData parent must implement IKeyedData.
      /// If the IndexBoard cache is ready and contains the given parent
      /// then the IKeyedData.KeyedData property of that parent is returned;
      /// otherwise the function returns None.
      static member public TryGetTopnode<'T when 'T:> IKeyedData> (?name: string) =
        if not IndexBoard.Ready then None
        elif not (IndexBoard.Instance.ContainsRef<'T> (?name = name)) then None
        else // fsharp syntax notice: the below indentation may be omitted
          let parent = IndexBoard.Instance.GetRef<'T> (?name = name)
          Some parent.KeyedData
      
      /// Function to look up a named distinct subnode, if it exists.
      /// If the subnode does not exist the function returns None.
      /// An exception is raised if more than one subnode match the name.
      static member public TryGetSubnode (name: string) (head: KeyedDataNode option) =
        if head.IsNone then None else // notice: fsharp if-guard with indentation-less else-part
        let nodes = head.Value.nodes
        if not (nodes.ContainsKey name) then None else // - ditto -
        match nodes.[name] with
        | [] -> None
        | [ subnode ] -> Some subnode
        | _ -> sprintf "Failed to locate distinct subnode named '%s'." name
               |> KapoinPersistenceError.Raise
      
      /// Function to look up a named distinct value property, if one exists.
      /// If a matching value does not exist the function returns None.
      /// An exception is raised if there is more than one matching value.
      static member public TryGetValue (name: string) (head: KeyedDataNode option) =
        if head.IsNone then None else
        let values = head.Value.values
        if not (values.ContainsKey name) then None else
        match values.[name] with
        | [] -> None
        | [ strvalue ] -> Some strvalue
        | _ -> sprintf "Failed to locate distinct value property named '%s'." name
               |> KapoinPersistenceError.Raise
      
      /// Look up distinct value property of a keyed data node and parse it
      /// with a 'tryParse' function.
      member public node.LookUpAndParse (name: string) (tryParse: string -> 'a option) =
        match KeyedDataNode.TryGetValue name (Some node) with
        | None -> sprintf "Field '%s' missing." name
                  |> KapoinPersistenceError.Raise
        | Some value -> value
        |> tryParse
        |> function
           | None -> sprintf "Failed parsing field '%s'." name
                     |> KapoinPersistenceError.Raise
           | Some value -> value
  
