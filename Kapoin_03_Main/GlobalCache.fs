// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Cache
  
  open Rodhern.Kapoin.Helpers
  
  
  [< KSPAddon (KSPAddon.Startup.MainMenu, true) >]
  /// Actually instantiate a universal cache object.
  /// Note: The class itself is currently called 'IndexBoard',
  ///  but that name is subject to change.
  type IndexBoard () =
    inherit KapoinCache ()
    
    /// Read-only access to unique cache object instance.
    static member public Instance
     with get () = IndexBoard.GetInstance () :?> IndexBoard
  
