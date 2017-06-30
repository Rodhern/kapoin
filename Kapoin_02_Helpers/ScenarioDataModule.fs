// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.ScenarioData
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  [< AbstractClass >]
  /// A scenario data module is used to load partial data from the persistence
  /// file when the designated scene is loaded, and save partial data when
  /// leaving the scene.
  type ScenarioDataModule =
    inherit ScenarioModule
    
    // #region Common DataAndLoggerNode container implementation
    // ---- ---- ---- ---- ---- ----
    
    val node: DataAndLoggerNode // the data and logger pair
    
    /// The ScenarioDataModule and its descendants are all born with a
    /// DataAndLoggerNode with a freshly initialized trace logger.
    new () as obj =
      { inherit ScenarioModule ()
        node= new DataAndLoggerNode () } then
      do obj.node.InitTraceLogger (obj.GetType ())
    
    /// Write message to trace listeners and KSP log file.
    member public datamodule.LogFn = datamodule.node.LogFn
    
    /// Unique logging identifier available to descendant classes.
    member public datamodule.Uid = datamodule.node.Uid
    
    /// Access to the partial data.
    /// Add new keys, remove keys and modify the keyed data
    /// to govern which information is saved to persistence file.
    /// Notice: Access to the node reference itself is read-only.
    member public datamodule.KeyedData = datamodule.node.KeyedData
    
    // ---- ---- ---- ---- ---- ----
    // #endregion
    
    /// When the data module is loaded the values (and subnodes) of the
    /// persistence file config node is copied to KeyedData.
    /// The data module is only loaded once; Unity will create a new fresh
    /// data module object before reloading data or loading new data.
    override datamodule.OnLoad (node: ConfigNode) =
      base.OnLoad node
      datamodule.LogFn <| sprintf "OnLoad (source node named '%s')." node.name
      if datamodule.KeyedData.IsEmpty
       then datamodule.node.KeyedData <- KeyedDataNode.CopyConfigNode node
       else "Data already present in data module."
            |> KapoinPersistenceError.Raise
    
    /// When the data module is to be saved, the keyed data is copied to the
    /// module's persistence file config node.
    /// Notice: The copied data is filtered, meaning that first level nodes
    ///  and values already found in the config node are ignored; those values
    ///  and nodes are not updated, even if the data is outdated or incomplete.
    override datamodule.OnSave (node: ConfigNode) =
      base.OnSave node
      datamodule.LogFn <| sprintf "OnSave (destination node named '%s')." node.name
      if datamodule.KeyedData.IsEmpty
       then // it is a bad omen if keyed data is empty at save time
            LogWarn "No data present in scenario data module."
       else datamodule.KeyedData.TrimAndSaveKeyedData node
    
    /// No particular actions are taken to enable or disable a scenario data
    /// module.
    /// A warning is logged if keyed data is already present at 'OnEnable'.
    member (*internal*) public datamodule.OnEnable () =
      datamodule.LogFn "Enable/load data module."
      if not datamodule.KeyedData.IsEmpty
       then LogWarn "Keyed data already present at module load/enable."
    
    /// No particular actions are taken to enable or disable a scenario data
    /// module.
    /// For good measure keyed data is cleared at 'OnDisable'.
    member (*internal*) public datamodule.OnDisable () =
      datamodule.LogFn "Disable/discard data module."
      datamodule.node.KeyedData <- KeyedDataNode.EmptyNode // should make no difference
    
    /// The persistence file, 'persistent.sfs', may be loaded on demand.
    /// Be aware that the persistence file may not be up to date mid-scene.
    /// The return value is a 'SortedList' of scenario nodes.
    /// The keys in the returned 'SortedList' are the scenario names.
    /// The (optional) input argument is an array of scenario names;
    /// only scenarios that match the names in the array are returned.
    /// It is perfectly possible for fewer keys to be returned than the
    /// number of scenario names given as input argument.
    /// It is safe to search for scenarios that are not present;
    /// if a given scenario is not found, it is not returned in the output.
    /// One key, named "GAME", is special; it is the key for a filtered
    /// version of the root game node. The filtered root game node is included
    /// when "GAME" is among the input parameter scenario names.
    /// If LoadSFS is called without an input parameter
    /// only the filtered root game node is returned.
    /// If the load fails an empty SortedList is returned.
    static member public LoadSFS (?scenarios: string array) =
      try
        GamePersistence.LoadSFSFile ("persistent", HighLogic.SaveFolder) // hardcoded sfs file name
        |> KeyedDataNode.LoadSelected (defaultArg scenarios [| "GAME" |])
       with
       | (e: exn)
         -> LogError <| sprintf
              "Exception of type '%s' encountered in %s; with message \"%s\"."
              (e.GetType ()).Name "ScenarioDataModule.LoadSFS" e.Message
            new SortedList<_,_> ()
  
