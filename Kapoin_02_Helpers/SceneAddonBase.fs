// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers
  
  open System
  open System.Collections.Generic
  open UnityEngine
  
  
  [< AbstractClass >]
  /// The SceneAddonBehaviour is almost the same as the MonoBehaviour class.
  /// The difference is that every SceneAddonBehaviour object is assigned a
  /// unique identification value (Uid). The Uid is useful for logging and
  /// debugging.
  type SceneAddonBehaviour (name: string, ?verbose: bool) =
    inherit MonoBehaviour ()
    
    /// Unique identifier for created object.
    let uid = Guid.NewGuid ()
    
    /// Private variable for instance log method 'LogFn'.
    let mutable logfnvar = SceneAddonBehaviour.LogTrace (name, uid)
    
    /// Private variable for 'Verbose' property.
    let mutable verboselogging = defaultArg verbose false
    
    /// Constructor logic.
    do SceneAddonBehaviour.OnCreate (logfnvar)
    
    /// Unique identifier assigned when the object was created.
    member public obj.Uid with get () = uid
    
    /// Set the name used in future trace and log messages
    /// written by the 'LogFn' method.
    member public obj.SetLogName (newname: string) =
      logfnvar <| sprintf "Rename to '%s' in (future) trace and log output." newname
      logfnvar <- SceneAddonBehaviour.LogTrace (newname, uid)
    
    /// Set Verbose to true to get logging of e.g. change of KSP application
    /// focus. The default value is false.
    member public obj.Verbose
            with get () = verboselogging
             and set b = verboselogging <- b
    
    /// Write message to trace listeners and KSP log file.
    member public obj.LogFn (txt: string) =
      logfnvar txt
    
    /// Get writer for messages to trace listeners and KSP log file.
    /// This is a static (and often less informative) version of 'LogFn'.
    static member private LogTrace (name: string, ?uid: Guid): string -> unit =
      UtilityModule.LogTrace (name, uid)
    
    /// Call Delete to signal Unity to dispose of the object.
    /// Note: Not used for ordinary lifetime management of objects.
    member public obj.Delete () =
      obj.LogFn "Schedule object deletion."
      do GameObject.Destroy obj
    
    // #region Unity and CLR invoked methods
    // ---- ---- ---- ---- ---- ----
    
    // There are different groups of methods in this region.
    // When relevant scenes load, the SceneAddonBehaviour is created;
    // this triggers OnCreate() (by the CLR) and Awake() and OnEnable().
    // When the scenes unload, the object can be disposed of;
    // this triggers OnDisable() and OnDestroy() and Finalize() (by the CLR).
    // The most often used methods are Start() and Update().
    // More specialized methods keep track of when the game looses focus or
    // the game is quit.
    // Note: Five methods intentionally not implemented are commented out!
    
    /// The call to OnCreate is part of the (static) object construction.
    static member private OnCreate (logfn: string -> unit) =
      logfn "Method 'OnCreate' (static constructor)."
    
    /// Awake is called when Unity (loads or) creates the object.
    member public obj.Awake () =
      obj.LogFn "Method 'Awake' (initialize behaviour)."
    
    /// When the object becomes active OnEnable is called.
    member public obj.OnEnable () =
      obj.LogFn "Method 'OnEnable'."
    
    /// When the object becomes inactive OnDisable is called.
    member public obj.OnDisable () =
      obj.LogFn "Method 'OnDisable'."
    
    /// OnDestroy is called when Unity (unloads or) destroys the object.
    member public obj.OnDestroy () =
      obj.LogFn "Method 'OnDestroy'."
    
    /// The call to Finalize is part of the object destruction.
    override obj.Finalize () =
      obj.LogFn "Method 'Finalize' (destructor)."
      base.Finalize ()
    
    ///// Initialization method called once all the relevant scene behaviours
    ///// are created. Initialization code that relies on other scene behaviours
    ///// to exist is often put in the Start() method.
    //member public obj.Start () =
    //  LogError <| sprintf "Method '%s' not implemented!" (System.Reflection.MethodBase.GetCurrentMethod ()).Name
    
    ///// The main update frame, the one that is important for the renderer.
    //member public obj.Update () =
    //  LogError <| sprintf "Method '%s' not implemented!" (System.Reflection.MethodBase.GetCurrentMethod ()).Name
    
    ///// The update frame relevant for physics updates.
    //member public obj.FixedUpdate () =
    //  LogError <| sprintf "Method '%s' not implemented!" (System.Reflection.MethodBase.GetCurrentMethod ()).Name
    
    ///// The late update method is invoked after the usual frame update methods have already executed.
    //member public obj.LateUpdate () =
    //  LogError <| sprintf "Method '%s' not implemented!" (System.Reflection.MethodBase.GetCurrentMethod ()).Name
    
    ///// Used to handle GUI events.
    ///// This method might be called several times per frame.
    ///// It is unlikely that you need to implement this method.
    //member public obj.OnGUI () =
    //  LogError <| sprintf "Method '%s' not implemented!" (System.Reflection.MethodBase.GetCurrentMethod ()).Name
    
    /// Called to notify that the application lost or regained focus.
    /// (On Android loosing focus causes the application to pause).
    member public obj.OnApplicationFocus (hasFocus: bool) =
      if verboselogging
       then sprintf "OnApplicationFocus (%s focus)." (if hasFocus then "gained" else "lost")
            |> obj.LogFn
    
    /// Called to notify that the application is quitting (except for iOS).
    member public obj.OnApplicationQuit () =
      if verboselogging
       then sprintf "Method 'OnApplicationQuit' (from '%A')." HighLogic.LoadedScene
            |> obj.LogFn
    
    /// Called to notify that the application is going to sleep.
    /// (An iOS application pseudo-quit feature, and an Android suspend feature).
    member public obj.OnApplicationPause (isPaused: bool) =
      if verboselogging
       then sprintf "Method 'OnApplicationPause' (sleep) (isPaused= '%b')." isPaused
            |> obj.LogFn
    
    // ---- ---- ---- ---- ---- ----
    // #endregion
  
