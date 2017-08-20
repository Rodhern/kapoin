// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.ScrapYard
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Wrappers
  open Rodhern.Kapoin.Helpers
  open Rodhern.Kapoin.Helpers.UtilityClasses
  open Rodhern.Kapoin.Helpers.UtilityModule
  
  
  [< AbstractClass >] // ABSTRACT ATTRIBUTE TO BE DELETED
  type ScrapYardHelper =
    inherit System.Object // placeholder
  
  

namespace Rodhern.Kapoin.Helpers.ReflectedWrappers // Maybe merge to ScrapYard namespace ?
  
  open System
  open System.Collections.Generic
  open System.Reflection
  open Rodhern.Kapoin.Wrappers
  open Rodhern.Kapoin.Helpers
  open Rodhern.Kapoin.Helpers.UtilityClasses
  open Rodhern.Kapoin.Helpers.UtilityModule
  
  
  /// TODO
  type KCTHelper =
    
    static member public SomeStaticMember () =
      ()
  
  
  /// TODO
  type KRASHHelper =
    
    /// Private reflection helper method that look up the KRASH plugin
    /// variable 'shelterSimulationActive'.
    static member private ShelterSimulationActive () =
      let staticflags = BindingFlags.Public ||| BindingFlags.Static
      let instanceflags = BindingFlags.Public ||| BindingFlags.Instance
      match
        [ for la in AssemblyLoader.loadedAssemblies do
           if la.name = "KRASH" then
            for t in la.assembly.GetTypes () do
             if t.Name = "KRASHShelter" then
              for pvarinfo in t.GetMember ("persistent", staticflags) do
               let persistvar = (pvarinfo :?> Reflection.FieldInfo).GetValue null
               if assigned persistvar then
                let pvartype = persistvar.GetType ()
                for bvarinfo in pvartype.GetMember ("shelterSimulationActive", instanceflags) do
                 let boolvar = (bvarinfo :?> Reflection.FieldInfo).GetValue persistvar
                 yield boolvar :?> bool ] with
      | [] -> None | [ b ] -> Some b
      | list ->
        "KRASHHelper.ShelterSimulationActive: Multiple results;"
        + sprintf " result list length was %d." list.Length
        |> LogWarn
        None
    
    /// Look up the KRASH variable 'shelterSimulationActive' to determine if
    /// a simulation is currently active or not.
    /// Each time SimulationRunning is read a new lazy variable reference is
    /// created. This means that the calculation is not truly lazy, but it
    /// does allow SimulationRunning to be accessed early and let the DotNet
    /// reflection part of the job run later.
    static member public SimulationRunning
     with get () =
      lazy
        match KRASHHelper.ShelterSimulationActive () with // todo remove LogLine
        | None -> LogLine "KRASHHelper: KRASH bool not present."; false
        | Some true -> LogLine "KRASHHelper: KRASH simulation is running."; true
        | Some false -> LogLine "KRASHHelper: KRASH present but not active."; false
  