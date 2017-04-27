﻿/*
Copyright (c) 2014-2016, André Kolster (Nereid)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

// Changed to plugin's namespace here.
namespace Rodhern.Kapoin.Wrappers
{
   /// The Final Frontier Adapter class.
   public class FinalFrontierAdapter
   {
      private const int DEFAULT_PRESTIGE = -10000;

      // ExternalInterface in Final Frontier
      private object instanceExternalInterface = null;
      // methods of the ExternalInterface
      private MethodInfo methodGetVersion = null;
      private MethodInfo methodRegisterRibbon = null;
      private MethodInfo methodRegisterCustomRibbon = null;
      private MethodInfo methodAwardRibbonToKerbalByCode = null;
      private MethodInfo methodAwardRibbonToKerbalByRibbon = null;
      private MethodInfo methodAwardRibbonToKerbalsByCode = null;
      private MethodInfo methodAwardRibbonToKerbalsByRibbon = null;
      private MethodInfo methodRevokeRibbonFromKerbalByCode = null;
      private MethodInfo methodRevokeRibbonFromKerbalByRibbon = null;
      private MethodInfo methodIsRibbonAwardedToKerbalByCode = null;
      private MethodInfo methodIsRibbonAwardedToKerbalByRibbon = null;
      private MethodInfo methodGetMissionsFlownForKerbal = null;
      private MethodInfo methodGetDockingsForKerbal = null;
      private MethodInfo methodResearchForKerbal = null;
      private MethodInfo methodTotalMissionTimeForKerbal = null;
      private MethodInfo methodGetContractsCompletedForKerbal = null;

      // Ranks
      private MethodInfo methodRegisterRank = null;
      private MethodInfo methodPromoteKerbal = null;
      private MethodInfo methodAssignRankToKerbal = null;

      /// Protected instance method that iterates
      /// through all exported types from all loaded assemblies
      /// to find the type of a given name.
      protected Type GetType(String name)
      {
         return AssemblyLoader.loadedAssemblies
            .SelectMany(x => x.assembly.GetExportedTypes())
            .SingleOrDefault(t => t.FullName == name);
      }

      private MethodInfo GetMethod(Type type, String name, Type[] parameterTypes)
      {
         MethodInfo info = type.GetMethod(name, parameterTypes);
         if(info==null)
         {
            String signature = "";
            foreach (Type t in parameterTypes)
            {
               if(signature.Length>0) signature = signature + ",";
               signature = signature +t.Name;
            }
            Debug.LogError("failed to register method " + name + "(" + signature + ")");
         }
         return info;
      }

      private MethodInfo GetMethod(Type type, String name)
      {
         MethodInfo info = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
         if(info==null)
         {
            Debug.LogError("failed to register method "+name+"()");
         }
         return info;
      }

      private void RegisterMethods(Type type)
      {
         methodGetVersion = GetMethod(type, "GetVersion");
         methodRegisterRibbon = GetMethod(type, "RegisterRibbon");
         methodRegisterCustomRibbon = GetMethod(type, "RegisterCustomRibbon");
         // award
         methodAwardRibbonToKerbalByCode = GetMethod(type, "AwardRibbonToKerbal", new Type[] { typeof(String), typeof(ProtoCrewMember) });
         methodAwardRibbonToKerbalByRibbon = GetMethod(type,"AwardRibbonToKerbal", new Type[] { typeof(object), typeof(ProtoCrewMember) });
         methodAwardRibbonToKerbalsByCode = GetMethod(type, "AwardRibbonToKerbals", new Type[] { typeof(String), typeof(ProtoCrewMember[]) });
         methodAwardRibbonToKerbalsByRibbon = GetMethod(type, "AwardRibbonToKerbals", new Type[] { typeof(object), typeof(ProtoCrewMember[]) });
         // revoke
         methodRevokeRibbonFromKerbalByCode = GetMethod(type, "RevokeRibbonFromKerbal", new Type[] { typeof(String), typeof(ProtoCrewMember) });
         methodRevokeRibbonFromKerbalByRibbon = GetMethod(type, "RevokeRibbonFromKerbal", new Type[] { typeof(object), typeof(ProtoCrewMember) });
         // misc
         methodIsRibbonAwardedToKerbalByCode = GetMethod(type, "IsRibbonAwardedToKerbal", new Type[] { typeof(String), typeof(ProtoCrewMember) });
         methodIsRibbonAwardedToKerbalByRibbon = GetMethod(type, "IsRibbonAwardedToKerbal", new Type[] { typeof(object), typeof(ProtoCrewMember) });
         methodGetMissionsFlownForKerbal = GetMethod(type, "GetMissionsFlownForKerbal");
         methodGetDockingsForKerbal = GetMethod(type, "GetDockingsForKerbal");
         methodResearchForKerbal = GetMethod(type, "GetResearchForKerbal");
         methodTotalMissionTimeForKerbal = GetMethod(type, "GetTotalMissionTimeForKerbal");
         methodGetContractsCompletedForKerbal= GetMethod(type, "GetContractsCompletedForKerbal");
         // ranks
         methodRegisterRank = GetMethod(type, "RegisterRank", new Type[] { typeof(String), typeof(String) });
         methodPromoteKerbal = GetMethod(type, "PromoteKerbal", new Type[] { typeof(ProtoCrewMember) });
         methodAssignRankToKerbal = GetMethod(type, "AssignRankToKerbal", new Type[] { typeof(object), typeof(ProtoCrewMember) });
      }

      private void LogFailedMethodAccess(String name, Exception e)
      {
         Debug.LogError("failed to access method '"+name+"' [" + e.GetType() + "]:" + e.Message);
      }

      // ***************************************************************************************************************************************
      // ** ** **                                               public methods                                                          ** ** **
      // ***************************************************************************************************************************************

      /**
       * Test if Final Frontier is installed.
       * Returns:
       *   true, if and only if Final Frontier is installed and succesfully connected to this adapter
       */
      public bool IsInstalled()
      {
         return instanceExternalInterface != null;
      }

      /**
       * Version of final Frontier
       * Returns:
       *   version number of final Frontier
       */
      public String GetVersion()
      {
         if (!IsInstalled()) return "not installed";
         if (methodGetVersion != null)
         {
            try
            {
               return (String)methodGetVersion.Invoke(instanceExternalInterface, new object[] {});
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("GetVersion", e);
            }
         }
         return "unknown";
      }

      /**
       * Connect this adapter to Final Frontier.
       */
      public void Plugin()
      {
         try
         {
            Type typeExternalInterface = GetType("Nereid.FinalFrontier.ExternalInterface");
            if (typeExternalInterface != null)
            {
               // create an instance for the external interface
               instanceExternalInterface = Activator.CreateInstance(typeExternalInterface);
               if (instanceExternalInterface != null)
               {
                  RegisterMethods(typeExternalInterface);
                  Debug.Log("plugin of Final Frontier successful");
                  return;
               }
            }
            instanceExternalInterface = null;
            Debug.Log("Final Frontier not installed");
         }
         catch(Exception e)
         {
            Debug.LogError("plugin of Final Frontier Adapater failed: "+e.GetType()+" ('"+e.Message+"')");
            instanceExternalInterface = null;
         }
      }

      /**
       * Register a ribbon. 
       * Important: Do not register the same code twice!
       * Parameter:
       *   code : unique code for ribbon.
       *   pathToRibbonTexture: path to ribbon png file relative to GameData folder (without suffix).
       *   nmae: name of the ribbon
       *   description: description text
       *   first: true, if this ribbon will just awarded as a "first time" ribbon
       *   prestige: prestige (currently just used for ribbon ordering)
       * Returns:
       *   Registered ribbon or null, if failed
       * Throws:
       *   ArgumentException, if code was already registered
       */
      public object RegisterRibbon(String code, String pathToRibbonTexture, String name, String description = "", bool first = false, int prestige = DEFAULT_PRESTIGE)
      {
         if (IsInstalled() && methodRegisterRibbon != null)
         {
            try
            {
               return methodRegisterRibbon.Invoke(instanceExternalInterface, new object[] { code, pathToRibbonTexture, name, description, first, prestige });
            }
            catch(Exception e)
            {
               LogFailedMethodAccess("RegisterRibbon", e);
            }
         }
         return null;
      }

      /**
       * Register a custom ribbon. 
       * Important: Do not register the same id twice!
       * Parameter:
       *   id : unique id for ribbon (valid range start with 1001).
       *   pathToRibbonTexture: path to ribbon png file relative to GameData folder (without suffix).
       *   name: name of the ribbon
       *   description: description text
       *   prestige: prestige (currently just used for ribbon ordering)
       * Returns:
       *   Registered custom ribbon or null, if failed
       * Throws:
       *   ArgumentException, if id was already registered
       */
      public object RegisterCustomRibbon(int id, String pathToRibbonTexture, String name, String description = "", int prestige = DEFAULT_PRESTIGE)
      {
         if (IsInstalled() && methodRegisterCustomRibbon != null)
         {
            try
            {
               return methodRegisterCustomRibbon.Invoke(instanceExternalInterface, new object[] { id, pathToRibbonTexture, name, description, prestige });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("RegisterCustomRibbon", e);
            }
         }
         return null;
      }

      /**
       * Award a ribbon to a kerbal. If a ribbon is awarded twice only the first award counts.
       * Parameter:
       *   code: code of the ribbon
       *   kerbal: kerbal that will be awarded with a ribbon
       */
      public void AwardRibbonToKerbal(String code, ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodAwardRibbonToKerbalByCode != null)
         {
            try
            {
              methodAwardRibbonToKerbalByCode.Invoke(instanceExternalInterface, new object[] { code, kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("AwardRibbonToKerbal", e);
            }
         }
      }

      /**
       * Award a ribbon to a kerbal. If a ribbon is awarded twice only the first award counts.
       * Parameter:
       *   ribbon: ribbon
       *   kerbal: kerbal that will be awarded with a ribbon
       */
      public void AwardRibbonToKerbal(object ribbon, ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodAwardRibbonToKerbalByRibbon != null)
         {
            try
            {
              methodAwardRibbonToKerbalByRibbon.Invoke(instanceExternalInterface, new object[] { ribbon, kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("AwardRibbonToKerbal", e);
            }
         }
      }

      /**
       * Award a ribbon to multiple kerbals. If a ribbon is awarded twice only the first award counts.
       * Parameter:
       *   code: code of the ribbon
       *   kerbal: kerbals that will be awarded with a ribbon
       */
      public void AwardRibbonToKerbals(String code, ProtoCrewMember[] kerbals)
      {
         if (IsInstalled() && methodAwardRibbonToKerbalsByCode != null)
         {
            try
            {
               methodAwardRibbonToKerbalsByCode.Invoke(instanceExternalInterface, new object[] { code, kerbals });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("AwardRibbonToKerbals", e);
            }         
         }
      }

      /**
       * Award a ribbon to multiple kerbals. If a ribbon is awarded twice only the first award counts.
       * Parameter:
       *   ribbon: ribbon
       *   kerbals: kerbals that will be awarded with a ribbon
       */
      public void AwardRibbonToKerbals(object ribbon, ProtoCrewMember[] kerbals)
      {
         if (IsInstalled() && methodAwardRibbonToKerbalsByRibbon != null)
         {
            try
            {
               methodAwardRibbonToKerbalsByRibbon.Invoke(instanceExternalInterface, new object[] { ribbon, kerbals });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("AwardRibbonToKerbals", e);
            } 
         }
      }

      /**
       * Returns true, if a ribbon is awarded to a kerbal
       * Parameter:
       *   ribbon: ribbon
       *   kerbal: kerbal that is to check
       */
      public bool IsRibbonAwardedToKerbal(object ribbon, ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodIsRibbonAwardedToKerbalByRibbon != null)
         {
            try 
            { 
               return (bool)methodIsRibbonAwardedToKerbalByRibbon.Invoke(instanceExternalInterface, new object[] { ribbon, kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("IsRibbonAwardedToKerbal", e);
            } 
         }
         return false;
      }

      /**
       * Returns true, if a ribbon is awarded to a kerbal
       * Parameter:
       *   code:  code of the ribbon
       *   kerbal: kerbal that is to check
       */
      public bool IsRibbonAwardedToKerbal(String code, ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodIsRibbonAwardedToKerbalByCode != null)
         {
            try
            {
               return (bool)methodIsRibbonAwardedToKerbalByCode.Invoke(instanceExternalInterface, new object[] { code, kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("IsRibbonAwardedToKerbal", e);
            } 
         }
         return false;
      }

      /**
       * Revoke a ribbon from a kerbal. 
       * Parameter:
       *   ribbon: ribbon to be revoked
       *   kerbal: kerbal that will lose the ribbon
       */
      public void RevokeRibbonFromKerbal(object ribbon, ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodRevokeRibbonFromKerbalByRibbon != null)
         {
            try
            {
               methodRevokeRibbonFromKerbalByRibbon.Invoke(instanceExternalInterface, new object[] { ribbon, kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("RevokeRibbonFromKerbal", e);
            }
         }
      }

      /**
       * Revoke a ribbon from a kerbal. 
       * Parameter:
       *   code: code of the ribbon to be revoked
       *   kerbal: kerbal that will lose the ribbon
       */
      public void RevokeRibbonFromKerbal(String code, ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodRevokeRibbonFromKerbalByCode != null)
         {
            try
            {
               methodRevokeRibbonFromKerbalByCode.Invoke(instanceExternalInterface, new object[] { code, kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("RevokeRibbonFromKerbal", e);
            }
         }
      }


      /**
       * Get the number of completed missions for a kerbal. 
       * Parameter
       *   kerbal: kerbal that we are interested in
       * Returns:
       *   number of completed missions for the kerbal
       */
      public int GetMissionsFlownForKerbal(ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodGetMissionsFlownForKerbal != null)
         {
            try
            {
               return (int)methodGetMissionsFlownForKerbal.Invoke(instanceExternalInterface, new object[] { kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("GetMissionsFlownForKerbal", e);
            }
         }
         return 0;
      }

      /**
       * Get the number of dockings for a kerbal. 
       * Parameter
       *   kerbal: kerbal that we are interested in
       * Returns:
       *   number of dockings for the kerbal
       */
      public int GetDockingsForKerbal(ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodGetDockingsForKerbal != null)
         {
            try
            {
               return (int)methodGetDockingsForKerbal.Invoke(instanceExternalInterface, new object[] { kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("GetDockingsForKerbal", e);
            }
         }
         return 0;
      }

      /**
       * Get the number of completed contracts for a kerbal. 
       * Parameter
       *   kerbal: kerbal that we are interested in
       * Returns:
       *   number of number of completed contracts for the kerbal
       */
      public int GetContractsCompletedForKerbal(ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodGetContractsCompletedForKerbal != null)
         {
            try
            {
               return (int)methodGetContractsCompletedForKerbal.Invoke(instanceExternalInterface, new object[] { kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("GetContractsCompletedForKerbal", e);
            }
         }
         return 0;
      }

      /**
       * Get the accumulated research points for a kerbal. 
       * Parameter
       *   kerbal: kerbal that we are interested in
       * Returns:
       *   accumulated research points of this kerbal
       */
      public double GetResearchForKerbal(ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodResearchForKerbal != null)
         {
            try
            {
               return (double)methodResearchForKerbal.Invoke(instanceExternalInterface, new object[] { kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("GetResearchForKerbal", e);
            }
         }
         return 0;
      }

      /**
       * Get the total mission time for a kerbal. 
       * Parameter
       *   kerbal: kerbal that we are interested in
       * Returns:
       *   total mission time of this kerbal
       */
      public double GetTotalMissionTimeForKerbal(ProtoCrewMember kerbal)
      {
         if (IsInstalled() && methodTotalMissionTimeForKerbal != null)
         {
            try
            {
               return (double)methodTotalMissionTimeForKerbal.Invoke(instanceExternalInterface, new object[] { kerbal });
            }
            catch (Exception e)
            {
               LogFailedMethodAccess("GetTotalMissionTimeForKerbal", e);
            }
         }
         return 0;
      }
   }
}

