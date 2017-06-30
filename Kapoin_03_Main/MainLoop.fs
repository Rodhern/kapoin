// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Events
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.GameSettings
  open Rodhern.Kapoin.Helpers.Events
  open Rodhern.Kapoin.MainModule.Cache
  open Rodhern.Kapoin.MainModule.Data
  
  
  /// DESC_MISS
  type LoopState =
    /// The running game is not a Kapoin game.
    /// Only events that may change that situation are relevant
    /// (currently: exit to main menu, and change of game settings).
    | Hibernating
    /// No game is running yet.
    /// Once a game is started determine wheter it is a Kapoin game or not.
    | Waiting
    /// The loaded scene is a Kapoin game
    /// (active tracking loop at the Space Center scene).
    | Running
  
  
  [<KSPAddon (KSPAddon.Startup.MainMenu, true)>]
  /// DESC_MISS
  type MainKapoinLoop () =
    inherit LoopMonitor ("MainKapoinLoop")
    
    /// DESC_MISS
    let mutable loopstate = LoopState.Waiting
    
    /// DESC_MISS
    let mainlooptiming = { LoopId= MainLoop; InitialDelay= 1.f; RepeatDelay= 2.f; MinimumDeltaUT= 59. }
    
    /// DESC_MISS
    let maintickevent = DelegateEvent<EventHandler<EventArgs>> ()
    
    /// Check whether Kapoin career tracking should be enabled for the given game.
    /// The three criteria checked are:
    ///  The game is a career game.
    ///  The game mods include the Kapoin Space Center data tracking module.
    ///  The Kapoin game settings are set to 'progress tracking enabled'.
    /// The return value is a result bool and a debug string.
    let isTrackingEnabledGame (game: Game) =
      let isTrackingDataModulePresent () =
        let trackmodname = Constants.KSCTrackDataModuleName
        game.scenarios |> Seq.exists (fun scenario -> scenario.moduleName = trackmodname)
      try
        if game.Mode <> Game.Modes.CAREER then
          false, "Game mode is not career mode."
         elif not (isTrackingDataModulePresent ()) then
          false, "Kapoin Space Center tracking data module not included in game."
         elif not (game.Parameters.CustomParams<KapoinParameterNode> ()).TrackProgress then
          false, "Kapoin career progress tracking turned off in game settings."
         else
          true, "Game is capable of Kapoin career progress tracking."
       with exn ->
        do sprintf "Exception of type '%s' encountered when trying to determine if game is Kapoin career progress tracking capable." (exn.GetType ()).Name
           |> LogError
        false, "Failed to determine if game is Kapoin career progress tracking capable."
    
    /// Return whether the given scene is considered a game scene.
    /// The scenes that are considered game scenes are scenes 5, 6, 7 and 8.
    /// While all of the scenes technically are game scenes, in the context of
    /// this function scenes like the main menu and the credits screen are not
    /// considered game scenes.
    let isGameScene (scene: GameScenes) =
      [| GameScenes.SPACECENTER; GameScenes.EDITOR;
         GameScenes.FLIGHT; GameScenes.TRACKSTATION |]
      |> Set.ofArray
      |> Set.contains scene
    
    /// Set the mutable state and update the reference in cache.
    /// The cache reference uses the hardcoded key 'LoopState'.
    member private monitor.SetState (newstate: LoopState, ?logmsg: string) =
      let cache = IndexBoard.Instance
      let update () =
        if cache.ContainsRef<LoopState> "LoopState"
         then cache.RemoveRef<LoopState> ("LoopState")
         else monitor.LogFn <| sprintf "Add loop state reference '%A' to cache." newstate
        cache.AddRef<LoopState> (newstate, "LoopState")
        loopstate <- newstate
      if loopstate = newstate
       then if cache.ContainsRef<LoopState> "LoopState"
             then let cacheval = cache.GetRef<LoopState> "LoopState"
                  if cacheval = loopstate
                   then () // cache reference is already up to date
                   else sprintf "Wrong 'LoopState' cache value; found '%A', expected '%A'." cacheval loopstate
                        |> LogError
                        update ()
             else update ()
       else
        if logmsg.IsNone
         then sprintf "Loop state changed to %A." newstate
         else sprintf "%s (%A -> %A)." logmsg.Value loopstate newstate
         |> monitor.LogFn
        update ()
    
    /// The loop state value is stored in cache.
    /// LoopStateIsRunning is true when the cached value is 'Running'.
    /// If the cached value is 'Waiting' or 'Hibernating', or if the 
    /// loop state value is not cached, LoopStateIsRunning is false.
    static member public LoopStateIsRunning
     with get () = IndexBoard.TryGet<LoopState> "LoopState" = Some LoopState.Running
    
    /// Look up the KapoinSpaceCenterTrackingData object in cache and invoke
    /// its 'EnableDisableDataVer' method in response to game setting changes.
    member private monitor.PingTrackingDataMod () =
      let datamod = IndexBoard.Instance.GetRef<KapoinSpaceCenterTrackingData> ()
      in datamod.EnableDisableDataVer ()
    
    [< CLIEvent >]
    /// DESC_MISS
    member public monitor.MainTickEvent = maintickevent.Publish
    
    override monitor.Callback msg = // todo clean up this override
      // debug (pre)
      match msg with
      | Scene (SceneSwitchRequest (fromscene, toscene))
        -> monitor.LogFn <| sprintf "Debug (pre): Scene change from %A to %A." fromscene toscene
      | _ -> ()
      // --
      monitor.UpdateState msg
      monitor.RefreshMainDataNode msg
      // debug (post)
      match msg with
      | AppState msgtype
        -> ()//do monitor.LogFn <| sprintf "Callback, AppState, %s." (msgtype.GetType().ToString())
      | GameState msgtype
        -> ()//do monitor.LogFn <| sprintf "Callback, GameState, %s." (msgtype.GetType().ToString())
      | Level msgtype
        -> ()//do monitor.LogFn <| sprintf "Callback, Level, %s." (msgtype.GetType().ToString())
      | Scene msgtype
        -> ()//do monitor.LogFn <| sprintf "Callback, Scene, %s." (msgtype.GetType().ToString())
      | Tick MainLoop
        -> ()//do monitor.LogFn <| sprintf "Callback, Tick (UT= %.0f)." (Planetarium.GetUniversalTime ())
           maintickevent.Trigger [| monitor; new EventArgs () |] // TODO - note this line of code here in the middle of the 'debug (post)' section
      | Tick msgtype
        -> ()//do monitor.LogFn <| sprintf "Callback, Tick (UT= %.0f)." (Planetarium.GetUniversalTime ())
      // --
    
    /// Helper method for .UpdateState method below.
    member private monitor.HandleSettingsApplied () =
      do monitor.LogFn <| sprintf "Invoke 'HandleSettingsApplied' (current state: %A; scene: %A)." loopstate HighLogic.LoadedScene // Debug.
      let currentscene = HighLogic.LoadedScene
      if not (isGameScene currentscene) then
        // if we are not currently in a game simply set the state to Waiting
        monitor.SetState (Waiting, "Game settings possibly applied away from game scenes.")
       else
        let trackgame, dbgmsg = isTrackingEnabledGame HighLogic.CurrentGame
        match loopstate, trackgame with
        | Hibernating, true
          -> // change directly to Running
             monitor.SetState (Running, "Kapoin career progress tracking enabled in game settings.")
             if currentscene = GameScenes.SPACECENTER
              then monitor.PingTrackingDataMod ()
                   monitor.InvokeLoop mainlooptiming
        | Running, false
          -> // change directly to Hibernating
             monitor.SetState (Hibernating, "Kapoin career progress tracking disabled in game settings.")
             if currentscene = GameScenes.SPACECENTER
              then monitor.CancelLoop MainLoop
                   monitor.PingTrackingDataMod ()
        | _ -> ()
      do monitor.LogFn <| sprintf "Executed 'HandleSettingsApplied' (state: %A)." loopstate // Debug.
    
    /// Given a loop message, possibly update the loop state.
    member monitor.UpdateState (msg: LoopMessageType) =
      match loopstate, msg with
      
      // When entering the Space Center change status to either Running or Hibernating (if not already)
      | Waiting, GameState (Created game) when HighLogic.LoadedScene = GameScenes.SPACECENTER
        -> let enableTracking, dbgmsg = isTrackingEnabledGame game
           do monitor.LogFn <| sprintf "UpdateState: %s" dbgmsg
           monitor.SetState (if enableTracking then Running else Hibernating)
      
      // When returning to the Main Menu set loop status back to Waiting
      | Hibernating, Level (LevelWasLoaded GameScenes.MAINMENU)
      | Running, Level (LevelWasLoaded GameScenes.MAINMENU)
        -> monitor.SetState Waiting
      
      // The main loop is invoked at the Space Center scene (when loop state is Running)
      | Running, Level (LevelWasLoaded GameScenes.SPACECENTER)
        -> monitor.InvokeLoop mainlooptiming
      
      // The main loop is halted when exiting the Space Center scene (when loop state is Running)
      | Running, Scene (SceneSwitchRequest (GameScenes.SPACECENTER, _))
        -> monitor.CancelLoop MainLoop
      
      // When leaving the Main Menu, update the state reference in cache.
      // note: not really needed, but should help initialize the cache reference.
      | Waiting, Scene (SceneSwitchRequest (GameScenes.MAINMENU, _))
        -> monitor.SetState Waiting // i.e. state is unchanged
      
      // If game settings are changed while Hibernating then possibly 'wake up'.
      // If game settings are changed while Running then possibly stop 'running'.
      // note: The game settings applied event won't actually detect changes to
      //  difficulty settings (issue 13591).
      | Hibernating, Scene (GameSettingsApplied ())
      | Running, Scene (GameSettingsApplied ())
        -> monitor.HandleSettingsApplied ()
      
      // The assumption at this point is that if difficulty settings are
      // changed in-scene we will at least receive a 'GamePause false' message.
      | Hibernating, AppState (GamePause false)
      | Running, AppState (GamePause false)
        -> do monitor.LogFn "Debug: Enqueue check difficulty settings."
           let oldstate = loopstate
           let delayedAction () =
             do monitor.LogFn <| sprintf "Debug: Execute check difficulty settings. (ref state= %A; loop state= %A; loaded scene= %A)." oldstate loopstate HighLogic.LoadedScene
             if loopstate = oldstate then monitor.HandleSettingsApplied ()
           monitor.QueueAction delayedAction
      
      // all other cases are explicitly ignored
      | _ -> ()
    
    /// Given a loop message, check if the main data node (KAPOIN_CAREER_DATA)
    /// should be refreshed, and if so signal KapoinMainNode.
    member monitor.RefreshMainDataNode (msg: LoopMessageType) =
      match msg with
      
      // DESC_MISS
      | Level (LevelWasLoaded scene) when not (isGameScene scene)
        -> monitor.LogFn "Reset (hard)"
           KapoinMainNode.ResetNode true
      
      // DESC_MISS
      | GameState (Created game) when isGameScene HighLogic.LoadedScene
        -> monitor.LogFn "Reset (soft)"
           KapoinMainNode.ResetNode false
      
      // DESC_MISS
      | GameState (Load node)
      | GameState (LoadRevert node) when isGameScene HighLogic.LoadedScene
        -> monitor.LogFn "Load (Kapoin or otherwise)"
           KapoinMainNode.LoadNode node
      
      // DESC_MISS
      | GameState (Save node) when loopstate = Running
        -> monitor.LogFn "Save (loop running)"
           KapoinMainNode.SaveNode node
      
      // DESC_MISS
      | GameState (Save node)
        -> monitor.LogFn <| sprintf "Debug, ignore SAVE (loop state = %A)" loopstate // todo
      
      // DESC_MISS
      | _ -> ()
  
