// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.MainModule.Contracts
  
  open Contracts
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.Contracts
  open Rodhern.Kapoin.MainModule.Contracts.SRCData
  open Rodhern.Kapoin.MainModule.Contracts.MilestoneData
  
  
  // Build up the Kerbal Space Center
  
  // Todo, add contract parameters for each building and for kerbal crew et cetera
  // 
  
  
  /// Milestone contract (MS I) 'Expand KSC'.
  type CMS_I () =
    // inherit KapoinContract ()
    
    //[< StaticRequirementCheck >]
    /// TODO
    static member public CMS_I_SRC () =
      let action (node, day) =
        failwith "Action for CMS_I_SRC not implemented!"
      { new SRCAction () with override a.Execute () = SRCheckResult.WriteResult typeof<CMS_I> action }
  
