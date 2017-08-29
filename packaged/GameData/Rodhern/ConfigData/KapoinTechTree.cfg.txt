// **** *** ** ****** ** ****** ** ****** ** ****** ** *** ****
// **        Kapoin Tweaked Technology Tree (ver. 01)        **
// **** *** ** ****** ** ****** ** ****** ** ****** ** *** ****
// 
// This is a Module Manager config patch. The patch will move
// some parts, e.g. the Stayputnik, to a different tech node.
// 
// The difficulty will increase a bit if you play with the
// tweaked tech tree. For instance, all of the early control
// surfaces are grouped in one tech node, which will likely
// force you to spend more science points to unlock more tech
// nodes early on.
// 
// Rename the file extension, i.e. change the name from
// "KapoinTechTree.cfg.md" to "KapoinTechTree.cfg", to have
// Module Manager detect and apply the patch.
// 
// Note that the patch will apply to all games, not just Kapoin
// games, so only rename this file if you want to have the
// parts moved in all of your games.
// 


@TechTree
{
  @RDNode:HAS[#id[stability]]
  {
    @title = Basic Flight
    @description = Sticking fin and wing shaped boards to objects will make them fly. It has been done for ages.
  }
}


@PART[RC_cone]:NEEDS[RealChute]:FINAL
 { @TechRequired = survivability }

@PART[Engineer7500]:NEEDS[KerbalEngineer]:FINAL
 { @TechRequired = engineering101
   %entryCost = 400 }

@PART[EngineerChip]:NEEDS[KerbalEngineer]:FINAL
 { @TechRequired = engineering101
   %entryCost = 600 }

@PART[KAS_CPort1]:NEEDS[KAS]:FINAL
 { @TechRequired = fuelSystems
   @description = Classic multi-purpose refueling port. }

@PART[KAS_Pylon1]:NEEDS[KAS]:FINAL
 { @TechRequired = advConstruction }

// KAS_Strut1 in specializedConstruction

@PART[KAXsportprop]:NEEDS[KAX]:FINAL
 { @TechRequired = aerodynamicSystems }

@PART[EjectionModule]:NEEDS[VanguardTechnologies]:FINAL
 { @TechRequired = aerodynamicSystems }


@PART[sensorThermometer]:FINAL
 { @TechRequired = survivability }

@PART[sensorBarometer]:FINAL
 { @TechRequired = stability }

@PART[stackDecoupler]:FINAL
 { @TechRequired = basicRocketry }

@PART[radialDecoupler]:FINAL
 { @TechRequired = basicRocketry }

@PART[solidBooster]:FINAL
 { @TechRequired = generalRocketry }

@PART[solidBooster1-1]:FINAL
 { @TechRequired = advRocketry }

@PART[radPanelSm]:FINAL
 { @TechRequired = basicScience }

@PART[ServiceBay_125]:FINAL
 { @TechRequired = basicScience }

@PART[probeCoreSphere]:FINAL
{ @TechRequired = engineering101 }

@PART[batteryPack]:FINAL
{ @TechRequired = survivability }

@PART[ladder1]:FINAL
{ @TechRequired = generalConstruction }

@PART[telescopicLadder]:FINAL
{ @TechRequired = spaceExploration }

@PART[landerCabinSmall]:FINAL
{ @TechRequired = landing }

@PART[Mk1FuselageStructural]:FINAL
{ @TechRequired = aviation }

@PART[MK1CrewCabin]:FINAL
{ @TechRequired = aerodynamicSystems }

@PART[tailfin]:FINAL
 { @TechRequired = flightControl }

@PART[StandardCtrlSrf]:FINAL
 { @TechRequired = flightControl }

@PART[wingConnector2]:FINAL
 { @TechRequired = stability }

@PART[wingConnector]:FINAL
 { @TechRequired = stability }

@PART[wingConnector3]:FINAL
 { @TechRequired = aviation }

@PART[wingConnector4]:FINAL
 { @TechRequired = aviation }

@PART[wingConnector5]:FINAL
 { @TechRequired = aviation }

@PART[sweptWing1]:FINAL
 { @TechRequired = supersonicFlight }

@PART[sweptWing2]:FINAL
 { @TechRequired = supersonicFlight }

@PART[structuralWing2]:FINAL
 { @TechRequired = supersonicFlight }

@PART[structuralWing3]:FINAL
 { @TechRequired = supersonicFlight }

@PART[structuralWing4]:FINAL
 { @TechRequired = supersonicFlight }

@PART[delta_small]:FINAL
 { @TechRequired = supersonicFlight }

@PART[deltaWing]:FINAL
 { @TechRequired = supersonicFlight }
