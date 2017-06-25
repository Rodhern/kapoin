// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers
  
  open System
  open System.Collections.Generic
  open System.Reflection
  open UnityEngine
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  /// A Kapoin cache object, KapoinCache, is a dictionary of object references.
  /// References to objects are indexed by type and name. The name part is
  /// optional; if no name is given, the name is implicitly blank ("").
  /// The reference must be compatible with the type argument. Notice that
  /// although the type argument System.Object is compatible with all
  /// references, it is not very useful. The key for a reference is the
  /// combination of the type argument and the name. All keys must be unique.
  /// There is nothing magical about the constructor or destructor, so multiple
  /// Kapoin cache object instances may co-exist. In practice however, we use
  /// only one Kapoin cache object ('IndexBoard.Instance' in the Kapoin Main
  /// Module). When managed by Unity the cache object will register (and
  /// unregister) itself as a sole static instance.
  type KapoinCache () =
    inherit SceneAddonBehaviour ("KapoinCache", true)
    
    /// Private field for unique cache object instance.
    static let mutable kcObj: KapoinCache option = None
    
    /// Private dictionary for stored references.
    /// Add, access and remove references with AddRef, GetRef and RemoveRef.
    let allrefs  = new Dictionary<Type, Dictionary<string, obj>> ()
    
    /// Update the (static) cache object reference.
    static let setObj (objopt: KapoinCache option) =
      match kcObj, objopt with
      | None, Some kc
        -> kc.LogFn "Register Kapoin Cache Object."
           kcObj <- Some kc
      | Some kc, None
        -> kc.LogFn "Unregister Kapoin Cache Object."
           kcObj <- None
      | None, None
        -> "Uninitialized Kapoin Cache Object (re)unregistered."
           |> LogWarn // log a warning, but do not raise an exception
      | Some kc1, Some kc2 when Object.ReferenceEquals (kc1, kc2)
        -> if assigned kc1
            then kc1.LogFn "Reregister Kapoin Cache Object (error)."
           "Already initialized Kapoin Cache Object (re)registered."
           |> KapoinCacheNotIntializedError.Raise // raise an exception
      | Some kc1, Some kc2
        -> let id1 = if assigned kc1 then kc1.Uid else new Guid ()
           let id2 = if assigned kc2 then kc2.Uid else new Guid ()
           let msg = sprintf "Multiple Kapoin Cache Objects in play (%A and %A)." id1 id2
           if assigned kc1 then kc1.LogFn msg
           if assigned kc2 then kc2.LogFn msg
           KapoinCacheNotIntializedError.Raise msg
    
    /// Static helper function that a descendant type can use to implement
    /// its Instance property.
    /// Notice: There can be only one KapoinCache object (including descendant
    ///  types), because all 'Instance' properties will map to the same static
    ///  variable.
    ///  A descendant Instance property might look something like this:
    ///  "with get () = DescendantClass.GetInstance () :?> DescendantClass".
    ///  The static 'GetInstance' member is automatically inherited.
    static member public GetInstance () =
      match kcObj with
      | None -> KapoinCacheNotIntializedError.Raise "Kapoin Cache Object not available."
      | Some obj -> obj
    
    /// Reports if the unique cache object instance is registered yet.
    static member public Ready with get () = kcObj.IsSome
    
    /// The Unity 'Awake' method.
    member public cache.Awake () =
      base.Awake ()
      cache.LogFn "Cache 'Awake'."
    
    /// Unregister cache object.
    member public cache.OnDestroy () =
      base.OnDestroy ()
      cache.LogFn "Cache 'OnDestroy'."
      do setObj None
    
    /// Queue 'Initialize'.
    member public cache.Start () =
      cache.LogFn "Cache 'Start'."
      base.Invoke ("Initialize", 0.f)
    
    /// Register cache object.
    member public cache.Initialize () =
      cache.LogFn "Cache 'Initialize'."
      do GameObject.DontDestroyOnLoad cache
      do setObj (Some cache)
    
    /// Helper method for 'GetRef' and 'RemoveRef'.
    member private c.LookupAndRemove (T: Type, id: string, remove: bool) =
      if not (allrefs.ContainsKey T)
       then sprintf "Lookup error; no branch for %s." T.Name
            |> KapoinCacheAccessError.Raise
      elif not (allrefs.[T].ContainsKey id)
       then sprintf "Lookup error; key (%s,'%s') not found." T.Name id
            |> KapoinCacheAccessError.Raise
      else
       let result = allrefs.[T].[id]
       if remove then ignore (allrefs.[T].Remove id)
       result
    
    /// Add a reference to cache. The reference is stored by type and name.
    /// The type part of the key is the compile time type 'T. When in doubt
    /// pass generic parameter 'T explicitly (to avoid confusion).
    /// The string part of the key is the optional name argument, or when a
    /// name is not given a blank string.
    /// You cannot add a reference with a key already in use, not even if the
    /// reference is the already cached one. To replace a reference, first
    /// remove the already cached one with RemoveRef.
    member public c.AddRef<'T> (ref: 'T, ?name: string) =
      let T, id = typeof<'T>, defaultArg name ""
      if not (assigned ref)
       then "Cannot add reference; reference was null."
            |> KapoinCacheAccessError.Raise
      elif not (allrefs.ContainsKey T)
       then allrefs.Add (T, new Dictionary<string, obj> ())
      let br = allrefs.[T]
      if br.ContainsKey id
       then sprintf "Cannot add reference; key (%s,'%s') already exists." T.Name id
            |> KapoinCacheAccessError.Raise
       else br.Add (id, ref)
    
    /// Remove reference from cache; the reference itself remains untouched.
    /// Note: The generic parameter 'T should be passed explicitly;
    ///  otherwise F# type inference may well default to System.Object .
    member public c.RemoveRef<'T> (?name: string) =
      let T, id = typeof<'T>, defaultArg name ""
      do c.LookupAndRemove (T, id, true) |> ignore
    
    /// Retrieve cached reference by key (type and name).
    /// Note: The generic parameter 'T should be passed explicitly;
    ///  otherwise F# type inference may well default to System.Object .
    member public c.GetRef<'T> (?name: string) =
      let T, id = typeof<'T>, defaultArg name ""
      c.LookupAndRemove (T, id, false) :?> 'T
    
    /// Look up cached reference by key (type and name).
    /// Returns true if a reference with the given key is in cache.
    /// Notice: Both type and name part of the key must match exactly.
    ///  For instance, you cannot assume that 'T is identical to the run-time
    ///  type of the reference. It is a good idea to pass the generic
    ///  parameter 'T explicitly.
    member public c.ContainsRef<'T> (?name: string) =
      let T, id = typeof<'T>, defaultArg name ""
      if allrefs.ContainsKey T
       then allrefs.[T].ContainsKey id
       else false
    
    /// Look up reference in cache, by chain call to the public members
    /// 'Ready', 'GetInstance', 'ContainsRef' and 'GetRef'.
    /// If the reference exists it is returned as an option value.
    /// If the cache is not yet registered, or if the reference is
    /// not in the cache then TryGet returns None.
    static member public TryGet<'T> (?name: string) =
      if not KapoinCache.Ready
       then None
       else let cache = KapoinCache.GetInstance ()
            let id = defaultArg name ""
            if not (cache.ContainsRef<'T> id)
             then None
             else Some (cache.GetRef<'T> id)
  
