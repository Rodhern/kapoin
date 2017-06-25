// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.GameSettings
  
  open System
  open System.Reflection
  
  
  /// Game settings class for Kapoin.
  type KapoinParameterNode () =
    inherit GameParameters.CustomParameterNode ()
    
    /// Title property.
    /// The title that will appear at the top of the Kapoin settings column.
    override node.Title with get () = "Common Kapoin Options"
    
    /// Defines the (public) tab name for the Kapoin settings.
    override node.Section with get () = "Kapoin"
    
    /// Defines the order within the tab (in case of multiple columns).
    override node.SectionOrder with get () = 1
    
    /// The game modes that can use Kapoin.
    override node.GameMode with get () = GameParameters.GameMode.CAREER
    
    /// Whether Kapoin uses difficulty presets.
    override node.HasPresets with get () = true
    
    /// This method is used to fill in default values for known difficulty presets.
    override node.SetDifficultyPreset (preset: GameParameters.Preset) =
      match preset with
      | GameParameters.Preset.Easy
      | GameParameters.Preset.Normal
        -> node.TrackProgress <- false
           // other settings go here
      | GameParameters.Preset.Moderate
      | GameParameters.Preset.Hard
        -> node.TrackProgress <- true
           // other settings go here
      | GameParameters.Preset.Custom
      | _
        -> () // i.e. unchanged settings
    
    // Fields can be 'disabled', which in this context means invisible.
    // E.g. for certain game modes.
    override node.Enabled (field: MemberInfo, parameters: GameParameters) =
      true // not yet implemented
    
    // Fields can become 'uninteractible', which means the user cannot change them.
    override node.Interactible (field: MemberInfo, parameters: GameParameters) =
      true // not yet implemented
    
    // The possible values of (all) string parameters are controlled by the ValidValues method.
    override node.ValidValues(field: MemberInfo): System.Collections.IList =
      null // not yet implemented
    
    
    [< DefaultValue; GameParameters.CustomParameterUI ("Track Progress", toolTip = "Enable progress tracking in Career Mode.") >]
    /// An example setting; in this case a checkbox.
    /// Kapoin Career Progress Tracking should be turned off when 'TrackProgress' is selected 'false'.
    val mutable public TrackProgress: bool
  
