// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.FinalFrontier
  
  open System
  open Rodhern.Kapoin.Helpers.FinalFrontier
  
  
  /// Enumeration for custom Kapoin ribbons.
  type KapoinRibbonEnum =
    | Plt1 = 11
    | Plt2 = 12
    | Plt3 = 13
    | Plt4 = 14
    | Plt5 = 15
    | Eng1 = 21
    | Eng2 = 22
    | Eng3 = 23
    | Eng4 = 24
    | Eng5 = 25
    | Sci1 = 31
    | Sci2 = 32
    | Sci3 = 33
    | Sci4 = 34
    | Sci5 = 35
    | Aux1 = 40
    | Aux2 = 42
    | Aux3 = 43
    | Aux4 = 44
    | Aux5 = 46
  
  
  /// Class that installs the custom ribbons that is otherwise installed by
  /// FinalFrontierCustomRibbons.cfg.
  type KapoinRibbonRegistrar () =
    inherit RibbonRegistrar<KapoinRibbonEnum> "KapoinRibbonRegistrar"
    
    /// The base id used when registrering customs ribbons.
    let [<Literal>] BaseId = 11000
    
    /// The folder of ribbon png images.
    let [<Literal>] PngFolder = "Rodhern/Ribbons/"
  
