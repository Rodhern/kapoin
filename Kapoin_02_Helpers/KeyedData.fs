// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.ScenarioData
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  /// A helper object that will help build lists arranged by key.
  type KeyedList<'K,'T when 'K: equality> () =
    
    /// The buffer is the (temporary) memory backing of the KeyedList.
    let buffer = new Dictionary<'K, 'T ResizeArray> ()
    
    /// Add a key and value pair to the collection.
    /// If the key already exists the value is appended to
    /// the end of the values list for that key.
    member public kl.Add (key: 'K, value: 'T) =
      if not <| buffer.ContainsKey key then buffer.Add (key, new ResizeArray<'T> ())
      do buffer.[key].Add value
    
    /// Return the collection in the form of a SortedList.
    /// This is the default use-case of a KeyedList.
    member public kl.SortedList () =
      let slist = new SortedList<'K, 'T list> (buffer.Count)
      for key in buffer.Keys
       do slist.Add (key, List.ofSeq buffer.[key])
      slist
    
    /// If you are sure every key has exactly one value,
    /// then the result can be simplified
    /// (from sortedList<'K, 'T list> to SortedList<'K,'T>).
    member public kl.SimplePairs () =
      let klist = new SortedList<'K,'T> (buffer.Count)
      for key in buffer.Keys
       do klist.Add (key, Seq.exactlyOne buffer.[key])
      klist
  
  
  /// A tree structure of string values.
  /// The contents is the same as inside a ConfigNode,
  /// but sorted by key names.
  type KeyedDataNode =
       { values: SortedList<string, string list>;
         nodes: SortedList<string, KeyedDataNode list> }
    with
      
      /// Copy the value list of a ConfigNode into a KeyedDataNode compatible
      /// ingredient.
      static member public ValuesList (cnode: ConfigNode) =
        let klist = new KeyedList<string, string> ()
        for kvp in cnode.values do klist.Add (kvp.name, kvp.value)
        klist.SortedList ()
      
      /// Copy the nodes list of a ConfigNode into a KeyedDataNode compatible
      /// ingredient.
      static member public NodesList (cnode: ConfigNode) =
        let klist = new KeyedList<string, KeyedDataNode> ()
        for node in cnode.nodes
         do let values = KeyedDataNode.ValuesList node
            let nodes = KeyedDataNode.NodesList node
            do klist.Add (node.name, { values= values; nodes= nodes })
        klist.SortedList ()
      
      /// Copy a ConfigNode.
      /// Abbreviated notation for calling ValuesList and NodesList.
      static member public CopyConfigNode (cnode: ConfigNode) =
        { values= KeyedDataNode.ValuesList cnode;
          nodes= KeyedDataNode.NodesList cnode }
      
      /// Create an empty KeyedDataNode.
      static member public EmptyNode
       with get () =
        { values= new SortedList<_,_> ();
          nodes= new SortedList<_,_> () }
      
      /// Check if the node is empty.
      member public node.IsEmpty
       with get () =
        (node.values.Count = 0) && (node.nodes.Count = 0)
      
      /// Save the data to the target ConfigNode, by adding
      /// key-value pairs and ConfigNode subnodes using
      /// '.AddValue (key, value)' and 'ConfigNode.AddNode'.
      member public srcnode.SaveKeyedData (tgtnode: ConfigNode) =
        for key in srcnode.values.Keys
         do for value in srcnode.values.[key]
             do tgtnode.AddValue (key, value)
        for key in srcnode.nodes.Keys
         do for subnode in srcnode.nodes.[key]
             do tgtnode.AddNode key |> subnode.SaveKeyedData
      
      /// Remove first level nodes and values already found in the target node,
      /// then call SaveKeyedData to save the remaining data.
      /// Notice: The source node is left unaltered.
      member public srcnode.TrimAndSaveKeyedData (tgtnode: ConfigNode) =
        let trimmedsubnodes = KeyedList<_,_> ()
        for key in srcnode.nodes.Keys
         do if tgtnode.nodes.Contains key
             then () // these nodes are not cloned; e.g. PartModule maintains a set of own subnodes
             else trimmedsubnodes.Add (key, srcnode.nodes.[key])
        let trimmedvalues = KeyedList<_,_> ()
        for key in srcnode.values.Keys
         do if tgtnode.values.Contains key
             then () // this key is not cloned; the usual examples are "name" and "scene"
             else trimmedvalues.Add (key, srcnode.values.[key])
        let trimmednode = { nodes= trimmedsubnodes.SimplePairs (); values= trimmedvalues.SimplePairs () }
        do trimmednode.SaveKeyedData tgtnode
      
      /// Print the contents of the node to the log file.
      /// If TRACE is enabled also send output to trace listeners.
      /// Notice: An empty node will not print anything in the log file.
      member public node.DebugPrint () =
        for key in node.values.Keys
         do match node.values.[key] with
            | [ value ] -> LogLine <| sprintf "   %s = %s" key value
            | xs -> LogLine <| sprintf "   %s = ..." key
                    for x in xs do LogLine <| sprintf "      %s" x
        for key in node.nodes.Keys
         do for subnode in node.nodes.[key]
             do LogLine <| sprintf "  %s {" key
                subnode.DebugPrint ()
                LogLine "  }"
      
      /// Helper function to copy selected scenario data from a persistence
      /// config node. The data is returned as keyed data in a SortedList with
      /// the scenario names as keys.
      /// Usually the given persistence config node is the game node itself.
      /// If, instead, the given persistence config node is a top level sfs
      /// file node, then the sfs file node's sole "GAME" subnode is used as
      /// the source config node. Currently this method's only use is as a
      /// helper method for the static method ScenarioDataModule.LoadSFS, used
      /// to load the save game persistence file on demand.
      /// Notice: Only "SCENARIO" nodes are considered for matching. Scenario
      ///  names are case sensitive. Scenario names without matching scenario
      ///  nodes are ignored. The scenario name "GAME" has special meaning.
      static member LoadSelected (scenarios: string array) (topnode: ConfigNode) =
        let gamekey, paramkey, scenariokey = "GAME", "PARAMETERS", "SCENARIO"
        let gnode =
          match topnode.CountNodes with
          // this is the standard case
          | n when n > 1
            -> topnode
          // this may be the case if topnode is from an sfs file load
          | 1 when topnode.nodes.[0].name = gamekey
            -> topnode.nodes.[0] 
          // other cases with 0 or 1 subnodes probably will not work
          | _ -> KapoinPersistenceError.Raise <| sprintf
                   "Node named '%s' with %d subnodes cannot be used as %s node."
                   topnode.name topnode.CountNodes gamekey
        let allnodes = [ for node in gnode.nodes do yield node ]
        let results = new KeyedList<_,_> ()
        if Array.contains gamekey scenarios
         then let paramnode = allnodes |> List.filter (fun node -> node.name = paramkey) |> List.exactlyOne
              let rootvalues = KeyedDataNode.ValuesList gnode
              let paramvalues = KeyedDataNode.ValuesList paramnode
              let paramsubnodes = KeyedDataNode.NodesList paramnode
              let onenodelist = new SortedList<_,_> ()
              do onenodelist.Add (paramkey, [ { values= paramvalues; nodes= paramsubnodes} ])
              do results.Add (gamekey, { values= rootvalues; nodes= onenodelist })
        let getNodeName (values: SortedList<string, string list>) =
          if values.ContainsKey "name"
           then match values.["name"] with
                | [name] -> name
                | _ -> ""
           else ""
        do allnodes
           |> List.filter (fun node -> node.name = scenariokey)
           |> List.map (fun node -> KeyedDataNode.ValuesList node, node)
           |> List.map (fun (vals, node) -> getNodeName vals, vals, node)
           |> List.filter (fun (name, _, _) -> Array.contains name scenarios)
           |> List.map (fun (name, vals, node) -> name, vals, KeyedDataNode.NodesList node)
           |> List.iter (fun (name, vals, nodes) -> results.Add (name, { values= vals; nodes= nodes }))
        results.SimplePairs ()
  
  
  /// A DataAndLoggerNode object is a container of a two ingredients.
  /// One ingredient is the keyed data, in a KeyedDataNode.
  /// The other is a trace logger unique to each DateNodeAndLogger instance.
  type DataAndLoggerNode =
    val mutable private keyeddata: KeyedDataNode // the keyed data
    val mutable private tracelogger: string -> unit // the logger stub
    val private id: Guid // immutable life-time identification
    
    /// The default constructor does not initialize the trace logger.
    /// Until the trace logger is initialized it ignores all invocations.
    new () = { keyeddata= KeyedDataNode.EmptyNode; tracelogger= ignore; id= Guid.NewGuid () }
    
    /// Unique logging identifier.
    member public data.Uid with get () = data.id
    
    /// Public access to the keyed data.
    member public data.KeyedData
     with get () = data.keyeddata
      and set kdnode = data.keyeddata <- kdnode
    
    /// Public access to trace logger,
    /// to write messages to trace listeners and KSP log file.
    member public data.LogFn txt =
      data.tracelogger txt
    
    /// Initialize the trace logger.
    /// It is possible to never initialize the trace logger. It is also
    /// possible to reinitialize an already initialized trace logger.
    /// It is recommended to initialize the trace logger exactly once,
    /// preferably when it is first created.
    member public data.InitTraceLogger (parenttype: Type, ?delayedlogger: bool) =
      let delay = defaultArg delayedlogger false
      do data.tracelogger <- LogTraceDelayed (parenttype.Name, Some data.id, delay)
  
  
  [< AbstractClass >]
  /// Filtering variation of DataAndLoggerNode used for persisted keyed data,
  /// equipped with a virtual filter method, 'DataFilter', and implementations,
  /// 'OnLoad' and 'OnSave', for reading and writing data from and to a
  /// ConfigNode. The filter is applied on load so that only data that makes it
  /// past the filter is loaded as keyed data.
  type FilteringDataNode () =
    inherit DataAndLoggerNode ()
    
    /// Data is filtered on load. The applied filter is 'DataFilter'.
    /// The filtered data is returned as an option value,
    /// with None meaning that the filtered data is unchanged.
    /// The preferred technique is for 'DataFilter' to create a copy
    /// of the input node, instead of directly manipulating the input.
    /// Note: Technically the data is first loaded, and then filtered.
    /// Note: The filter is applied on save as well.
    abstract member DataFilter: KeyedDataNode -> KeyedDataNode option
    
    /// Clear the keyed data. All existing keyed data is discarded.
    member public data.Clear () =
      data.LogFn "Clear keyed data." // temporary debug message; this code line to be removed later
      data.KeyedData <- KeyedDataNode.EmptyNode
    
    /// To be used in container's OnLoad .
    member public data.OnLoad (cnode: ConfigNode) =
      data.LogFn <| sprintf "OnLoad (source node named '%s')." cnode.name
      if data.KeyedData.IsEmpty
       then let unfiltereddata = KeyedDataNode.CopyConfigNode cnode
            let filtereddata = defaultArg (data.DataFilter unfiltereddata) unfiltereddata
            data.KeyedData <- filtereddata
       else let msg = "Cannot load data; data already present in node."
            data.LogFn msg // sending (duplicate) message to specific log will ...
            LogError msg // ... help to find the source of the problem
    
    /// To be used in container's OnSave .
    member public data.OnSave (cnode: ConfigNode) =
      data.LogFn <| sprintf "OnSave (destination node named '%s')." cnode.name
      let filtereddata =
        match data.DataFilter data.KeyedData with
        | None -> data.KeyedData // this is the normal case
        | Some fdata
          -> let msg = "Data was actively filtered during save."
             data.LogFn msg // sending (duplicate) message to specific log will ...
             LogWarn msg // ... help to find the source of the problem 
             fdata
      filtereddata.SaveKeyedData cnode
  
