// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers
  
  open System
  open System.Collections.Generic
  open System.Reflection
  open UnityEngine
  
  
  /// A utility module with logging, parsing and other general purpose methods.
  /// An F# module is a static class with particular syntax.
  /// Method members in a module are given special F# privileges (e.g. nested class namespace names).
  /// Static members of ordinary classes have to be fully qualified, which looks ugly in everyday code.
  module UtilityModule =
    begin
    
    /// Checks if an object variable is assigned a value different from null.
    let assigned (o: obj) =
        System.Object.ReferenceEquals (o, null) |> not
    
    
    [< AbstractClass >]
    /// A static list of names that are blacklisted.
    /// The blacklisted names are used to censor 'LogTrace' such that all
    /// messages to the associated log methods are discarded (ignored).
    type LoggerBlackList =
      #if TRACE // if TRACE is not defined LoggerBlackList does nothing at all.
      [< DefaultValue >] static val mutable private blacklist: SortedList<string, obj>
      #endif
      /// Add a name to the list of censored names.
      /// Matching 'LogTrace' log methods, present and future, are muted.
      static member Add (name: string) =
      #if TRACE
        if not (assigned LoggerBlackList.blacklist)
         then LoggerBlackList.blacklist <- new SortedList<_,_> ()
        if not (LoggerBlackList.blacklist.ContainsKey name)
         then LoggerBlackList.blacklist.Add (name, null)
      #else
        ()
      #endif
      /// Remove a name from the list of censored names.
      /// Associated log methods are unmuted.
      static member Remove (name: string) =
      #if TRACE
        if (assigned LoggerBlackList.blacklist)
           && (LoggerBlackList.blacklist.ContainsKey name)
         then LoggerBlackList.blacklist.Remove name |> ignore
      #else
        ()
      #endif
      /// Look up if the given name is in the list of censored names.
      static member IsBlackListed (name: string) =
      #if TRACE
        (assigned LoggerBlackList.blacklist) && (LoggerBlackList.blacklist.ContainsKey name)
      #else
        false
      #endif
    
    /// Private helper function that sends text to the 'TRACE' output.
    let private TraceWriteLine (tracekey: string) (txt: string) =
      sprintf "[%s] %s" tracekey txt
      |> Diagnostics.Trace.WriteLine
    
    /// Write trace messages to the trace listeners and the KSP.log file.
    /// Extended 'LogTrace' method that optionally implements an initial
    /// 'delay', so that the initialization message is not output until just
    /// before the first genuine message is.
    /// If the name given as name parameter is listed in the LoggerBlackList
    /// at the time when a message is about to be written, then that message
    /// is discarded instead.
    let public LogTraceDelayed (name: string, uidopt: Guid option, delayed: bool): string -> unit =
      #if TRACE
      let key = match uidopt with
                | Some uid -> sprintf "Rodhern/Kapoin/%s/%A" name uid
                | None -> sprintf "Rodhern/Kapoin/%s/Trace" name
      let write (txt: string) =
        if not (LoggerBlackList.IsBlackListed name) then
          do TraceWriteLine key txt
          do Debug.Log (sprintf "[%s] %s" key txt)
      let selectedwrite = // select the default or a delayed write function
        if not delayed then
          do write "Initialize/reinitialize/curry 'LogTrace'."
          write
         else
          let delayflag = ref true
          fun txt ->
            if !delayflag then
              do write "Delayed 'LogTrace' initialization message."
              delayflag:= false
            write txt
      selectedwrite
      #else
      ignore
      #endif
    
    /// Write trace messages to the trace listeners and the KSP.log file.
    /// The 'LogTrace' method is supposed to be called curried;
    /// i.e. initially without the message text parameter.
    /// A message is written every time 'name' is passed to 'LogTrace',
    /// so if both 'name' and text is passed two messages are written.
    /// However, if 'TRACE' is not defined the function calls are ignored.
    let public LogTrace (name: string, uidopt: Guid option): string -> unit =
      LogTraceDelayed (name, uidopt, false)
    
    /// Write debug message to the KSP.log file.
    /// If 'TRACE' is defined then also send the message to trace listeners.
    let public LogLine s =
      do TraceWriteLine "Rodhern/Kapoin/Debug" s
      do Debug.Log ("[Rodhern/Kapoin] " + s)
    
    /// Write warning message to the KSP.log file.
    /// If 'TRACE' is defined then also send the message to trace listeners.
    let public LogWarn s =
      do TraceWriteLine "Rodhern/Kapoin/Warning" s
      do Debug.LogWarning ("[Rodhern/Kapoin] " + s)
    
    /// Write error message to the KSP.log file.
    /// If 'TRACE' is defined then also send the message to trace listeners.
    let public LogError s =
      do TraceWriteLine "Rodhern/Kapoin/Error" s
      do Debug.LogError ("[Rodhern/Kapoin] " + s)
    
    /// Write message on screen, to log file and to trace listeners.
    let public Msg s =
      do TraceWriteLine "Rodhern/Kapoin/Message" s
      do Debug.Log ("[Rodhern/Kapoin] " + s)
      do ScreenMessages.PostScreenMessage(s, 10.f, ScreenMessageStyle.LOWER_CENTER) |> ignore
    
    
    /// Parsing floating point values w.r.t. "en-US" culture ensures that the
    /// decimal separator is a period, and not for instance a comma or such.
    let private fixedculture =
        new System.Globalization.CultureInfo "en-US"
    
    /// Write double precision floating point value as a string.
    let float2str (v: float) =
        v.ToString fixedculture
    
    /// Read double precision floating point value from a string.
    let str2float (s: string) =
        let result = ref System.Double.NaN
        if System.Double.TryParse (s, System.Globalization.NumberStyles.Float, fixedculture, result)
         then Some !result
         else None
    
    /// Write 32 bit integer value as a string.
    let int2str (v: int) =
        v.ToString fixedculture
    
    /// Read 32 bit integer value from a string.
    let str2int (s: string) =
        let result = ref (System.Int32 ())
        if System.Int32.TryParse (s, System.Globalization.NumberStyles.Integer, fixedculture, result)
         then Some !result
         else None
    
    /// Type abbreviation.
    type public VersionTuple = int*int*int*int
    
    /// Write four integer version value as a string.
    let ver2str ((a,b,c,d): VersionTuple) =
        sprintf "%0d.%0d.%0d.%0d" a b c d
      
    /// Read four integer values separated by period.
    let str2ver (name: string): VersionTuple option =
        if String.IsNullOrEmpty name
         then None
         else match name.Split [|'.'|] |> Array.map str2int with
              | [| Some a; Some b; Some c; Some d |] -> Some (a, b, c, d)
              | _ -> None
    
    end
  
  

namespace Rodhern.Kapoin.Helpers.UtilityClasses
  
  open System
  open Rodhern.Kapoin.Helpers
  
  
  /// Particular null reference exception variation,
  /// used when the Kapoin cache object is accessed
  /// in its uninitialized or disposed states.
  type KapoinCacheNotIntializedError (message: string) =
    inherit NullReferenceException (message)
    with static member public Raise s =
                                UtilityModule.LogError s
                                raise (new KapoinCacheNotIntializedError (s))
  
  /// Particular null reference exception variation,
  /// used for failed cache look ups et cetera.
  type KapoinCacheAccessError (message: string) =
    inherit NullReferenceException (message)
    with static member public Raise s =
                                UtilityModule.LogError s
                                raise (new KapoinCacheAccessError (s))
  
  /// Exception type for popups, toolbars et cetera.
  exception public KapoinGUIError of string
    with static member public Raise s =
                                UtilityModule.LogError s
                                raise (KapoinGUIError s)
  
  /// Exception type for persistence file load and save errors.
  exception public KapoinPersistenceError of string
    with static member public Raise s =
                                UtilityModule.LogError s
                                raise (KapoinPersistenceError s)
  
  /// Exception type for errors specific to FFAdapterHelper.
  exception public FFAdapterHelperError of string
    with static member public Raise s =
                                UtilityModule.LogError s
                                raise (FFAdapterHelperError s)
  
  /// Exception type for custom contracts, missions and progress goals.
  exception public KapoinContractError of string
    with static member public Raise s =
                                UtilityModule.LogError s
                                raise (KapoinContractError s)
  
