// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.FinalFrontier
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Wrappers
  open Rodhern.Kapoin.Helpers
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  /// Type abbreviation for Final Frontier ribbon objects.
  type FFRibbon = obj
  
  /// The three known profressions are pilot, engineer and scientist.
  type KerbalProfession =
       | Pilot
       | Engineer
       | Scientist
       | Other of string
       static member Parse (title: string) =
         match title with
         | "Pilot" -> Pilot
         | "Engineer" -> Engineer
         | "Scientist" -> Scientist
         | _ -> Other title
  
  /// Experience is granted based on certain activities.
  type ExpLogEntryType =
       | BoardVessel
       | ExitVessel
       | Flight
       | Suborbit
       | Orbit
       | Flyby
       | Escape
       | Land
       | PlantFlag
       | Recover
       | Other of string
       static member Parse (event: string) =
         match event with
         | "BoardVessel" -> BoardVessel
         | "ExitVessel" -> ExitVessel
         | "Flight" -> Flight
         | "Suborbit" -> Suborbit
         | "Orbit" -> Orbit
         | "Flyby" -> Flyby
         | "Escape" -> Escape
         | "Land" -> Land
         | "PlantFlag" -> PlantFlag
         | "Recover" -> Recover
         | _ -> Other event
  
  /// The celestial bodies of the default Kerbin system.
  type ExpLogCelestialBody =
       /// Kerbol, aka "Sun", is at the center of the solar system.
       /// Kerbol is the only celestial body not in an orbit of another body.
       | Kerbol
       /// The closest planet to Kerbol.
       | Moho
       /// The second planet from Kerbol, and neighbour to Kerbin.
       | Eve
       /// Moon of Eve.
       | Gilly
       /// The third planet from Kerbol. Home to all Kerbal-kind.
       | Kerbin
       /// First moon of Kerbin.
       | Mun
       /// Second moon of Kerbin.
       | Minmus
       /// The fourth planet from Kerbol, and neighbour to Kerbin.
       | Duna
       /// Moon of Duna.
       | Ike
       /// A dwarf planet.
       | Dres
       /// A Gas giant. The largest planet in the solar system.
       | Jool
       /// Moon of Jool.
       | Laythe
       /// Moon of Jool.
       | Vall
       /// Moon of Jool.
       | Tylo
       /// Moon of Jool.
       | Bop
       /// Moon of Jool.
       | Pol
       /// Of all known planets the dwarf rock and ice planet Eeloo is the farthest from Kerbol.
       | Eeloo
       /// A special value representing 'Option.None'.
       | NoBody
       /// An unrecognized celestial body.
       | Other of string
       static member Parse (body: string) =
         match body with
         | "Sun" -> Kerbol
         | "Moho" -> Moho
         | "Eve" -> Eve
         | "Gilly" -> Gilly
         | "Kerbin" -> Kerbin
         | "Mun" -> Mun
         | "Minmus" -> Minmus
         | "Duna" -> Duna
         | "Ike" -> Ike
         | "Dres" -> Dres
         | "Jool" -> Jool
         | "Laythe" -> Laythe
         | "Vall" -> Vall
         | "Tylo" -> Tylo
         | "Bop" -> Bop
         | "Pol" -> Pol
         | "Eeloo" -> Eeloo
         | "" -> NoBody
         | _ -> Other body
  
  /// The experience log is a list of activities undertaken.
  type ExpLogEntry =
       { Activity: ExpLogEntryType
         Body: ExpLogCelestialBody
         InProgress: bool
         FlightNo: int }
       static member New (entry: FlightLog.Entry) (inprogress: bool) =
         { Activity= ExpLogEntryType.Parse entry.``type``
           Body= ExpLogCelestialBody.Parse entry.target
           InProgress= inprogress
           FlightNo= entry.flight }
  
  /// Final Frontier keeps track of key statistics.
  type HallOfFameStatRecord =
       { MissionCount: int
         MissionTime: float
         ContractCount: int
         ResearchPoints: float
         DockingCount: int }
  
  /// A record for hauling crew data about.
  type RosterDataRecord<'RibbonEnum> =
       { Name: string
         RosterType: ProtoCrewMember.KerbalType
         RosterStatus: ProtoCrewMember.RosterStatus
         Gender: ProtoCrewMember.Gender
         Profession: KerbalProfession
         ExpLevel: int
         ExpLog: Lazy< ExpLogEntry list >
         AwardedRibbons: Lazy< 'RibbonEnum list >
         HallOfFameStats: Lazy< HallOfFameStatRecord > }
  
  /// Base container class for ribbon,
  /// that links the RibbonEnum value with its FFRibbon object.
  type RibbonLink<'RibbonEnum when 'RibbonEnum :> Enum> (id: 'RibbonEnum, ref: FFRibbon) =
    member public this.Id with get () = id
    member public this.Reference with get () = ref
    static member public EnumValues =
      [ for o in Enum.GetValues(typeof<'RibbonEnum>) do yield o :?> 'RibbonEnum ]
  
  /// Every ribbon must be uniquely identified by the Final Frontier plugin.
  /// The identification takes the form of an integer or string code.
  /// Integer id numbers are for true custom ribbons, primarily awarded by
  /// the player. String codes are used for ribbons handled exclusively by
  /// the plugin.
  type RibbonIdentifier =
       /// True custom ribbons do not have a string code assigned;
       /// instead they have a unique identification number.
       /// The IdNoOffset is added to the BaseId of the ribbon package.
       /// True custom ribbons can be assigned by the player at will.
       | IdNoOffset of int
       /// Coded ribbons have a string code assigned; e.g. 'Some "MyFOCodedRibbon"'.
       /// Coded ribbons are handled by plugins, and cannot be assigned (directly) by the player.
       | Code of string
  
  /// Each ribbon to be registered with the RibbonRegistrar
  /// must have its own RibbonRegisterEntry listing.
  type RibbonRegisterEntry<'RibbonEnum when 'RibbonEnum :> Enum> =
       {
         /// The title of the ribbon, e.g. "Flight Officer".
         Title: string
         /// The description of the ribbon,
         /// e.g. "Worn by crew that hold the pilot rank of Flight Officer."
         Description: string
         /// The name of the png image file.
         /// Do not include folder or extension part of the file path.
         Image: string
         /// The identification key for a ribbon registered by the registrar
         /// is an enumeration value.
         RibbonKey: 'RibbonEnum
         /// The ribbon identification used by Final Frontier can be either an
         /// integer value or a string code.
         /// The ribbon identification is used to keep save game consistency.
         /// Note: The Final Frontier ribbon identification is usually different
         /// to the registrar enumeration key for that ribbon.
         /// I put it down to a minor F# syntax issue that it is difficult to
         /// reuse the enumeration key as identification integer.
         RibbonId: RibbonIdentifier
         /// The prestige point number tells if a ribbon will be displayed near the top
         /// of a kerbal's pile of assigned ribbons.
         Prestige: int
       }
  
  /// The RibbonRegistrar will register the data returned by GetRibbonRegisterData.
  type RibbonRegisterData<'RibbonEnum when 'RibbonEnum :> Enum> =
       {
         /// The BaseId is added to the IdNoOffset to form the ribbon identification number.
         /// All ribbon packages should strive to define a BaseId of their own.
         BaseId: int
         /// The relative folder path for the png images,
         /// e.g. "Rodhern/Ribbons/".
         PngFolder: string
         /// The RibbonRegisterEntries field holds the actual ribbon data listing.
         RibbonRegisterEntries: RibbonRegisterEntry<'RibbonEnum> list
       }
  
  
  [< AbstractClass >]
  /// Base class that installs the custom ribbons.
  /// Add the KSPAddon() attribute to the derived class.
  type RibbonRegistrar<'RibbonEnum when 'RibbonEnum :> Enum and 'RibbonEnum: equality> (?name: string) =
    inherit SceneAddonBehaviour (defaultArg name "FFRibbonRegistrar")
  
