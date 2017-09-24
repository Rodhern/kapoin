// **** **** **** **** **** **** **** **** **** **** **** **** ****
// **  Copyright (c) 2017, Robert Nielsen. All rights reserved.  **
// **** **** **** **** **** **** **** **** **** **** **** **** ****

namespace Rodhern.Kapoin.Helpers.Events
  
  open System
  open System.Collections.Generic
  open Rodhern.Kapoin.Helpers.UtilityModule
  open Rodhern.Kapoin.Helpers.UtilityClasses
  
  
  /// DESC_MISS
  type SoftResetContext =
       /// A hard reset is usually done when the player quits a game.
       | Hard
       /// Even when a soft reset is due, the reset is not actually carried
       /// out yet, instead the current game context is saved to the soft
       /// reset stack, and the reset is delayed till game load time.
       | Soft of Game
       /// Under ideal circumstances every (standard) load will pair up with
       /// a recent soft reset (the head of the soft reset stack).
       | StdLoad
       /// Revert loads are quite tricky. Maybe reverts are more about
       /// signalling the end of the scene than the beginning of a new.
       | RevertLoad
    with
    
    /// Customized string output format
    /// (ignoring the soft reset game parameter).
    override this.ToString () =
      match this with
      | Hard -> "Hard"
      | Soft _ -> "Soft"
      | StdLoad -> "Load"
      | RevertLoad -> "Revert"
  
  
  [< StructuralEquality; NoComparison >]
  /// DESC_MISS
  type SoftResetRecord =
     { /// The title of the game.
       GameTitle: string
       /// The random game seed.
       GameSeed: int
       /// The present game time.
       GameUT: float
     }
     with
    
    /// Constructor. Create a soft reset record from the parameters of the
    /// given game variable. The game variable is optional, if none is given
    /// then HighLogic.CurrentGame is used.
    static member public CurrentGame (?game: Game) =
      match defaultArg game HighLogic.CurrentGame with
      | null
        -> "Cannot create soft reset record; current game variable was unavailable."
           |> KapoinPersistenceError.Raise
      | gpar
        -> { GameTitle= gpar.Title; GameSeed= gpar.Seed; GameUT= gpar.UniversalTime }
    
    /// Customized string output format.
    override this.ToString () =
      sprintf "{UT= %.4f, Seed= %d, Title= '%s'}" this.GameUT this.GameSeed this.GameTitle
  
  
  /// DESC_MISS
  type SoftResetCounter =
     { /// TODO
       SoftResetStraightCount: int
       /// TODO
       SoftResetShadowedCount: int
       /// TODO
       SoftResetShadowCount: int
       /// TODO
       SoftResetSim: int
       /// TODO
       RevertCount: int
       /// TODO
       LoadCount: int }
     with
    
    /// Customized string output format.
    override this.ToString () =
      sprintf "(Resets= %d:%d:%d:(s=%d), Loads= %d, Reverts= %d)"
              this.SoftResetStraightCount this.SoftResetShadowedCount this.SoftResetShadowCount
              this.SoftResetSim this.LoadCount this.RevertCount
  
  
  /// Type abbreviation for a tuple option value with a soft reset game
  /// context key (SoftResetRecord) and reset count (SoftResetCounter).
  type SoftResetItemOption = (SoftResetRecord*SoftResetCounter) option
  
  
  /// TODO
  type SoftResetStack =
    val mutable stacktop: SoftResetItemOption
    val mutable stacktopshadow: SoftResetItemOption
    val stacktail: ResizeArray<SoftResetRecord*SoftResetCounter>
    val mutable logfn: string -> unit
    
    /// Default constructor.
    new () = { stacktop= None; stacktopshadow= None; stacktail= new ResizeArray<_> (); logfn= ignore }
    
    /// Function that checks if a SoftResetItemOption matches the given key.
    static member private MatchOpt (key: SoftResetRecord) =
      function
      | None -> false
      | Some (k, _) -> k = key
    
    /// TODO , another public method, but do we want it public ?
    member public srs.IsSimulationContext () =
      let matchkey =
        SoftResetRecord.CurrentGame ()
        |> SoftResetStack.MatchOpt
      if matchkey srs.stacktop
       then (snd srs.stacktop.Value).SoftResetSim
      elif matchkey srs.stacktopshadow
       then (snd srs.stacktopshadow.Value).SoftResetSim
       else 0
      |> (<) 0 // true if argument is greater than zero
    
    /// Look up the key in the stack tail.
    /// If the item is found remove it and return it as an option value.
    member private srs.TailGet key =
      let matchkey item = (fst item) = key
      if srs.stacktail.Exists (Predicate<_> matchkey)
       then let result = srs.stacktail.Find (Predicate<_> matchkey)
            do srs.stacktail.Remove result |> ignore
            Some result
       else None
    
    /// TODO
    member private srs.PushToTail () =
      if srs.stacktopshadow.IsSome
       then srs.stacktail.Add srs.stacktopshadow.Value
            srs.stacktopshadow <- None
      if srs.stacktop.IsSome
       then srs.stacktail.Add srs.stacktop.Value
            srs.stacktop <- None
    
    /// Find, remove and return the keyed counter.
    member private srs.Pop (key: SoftResetRecord) =
      let matchkey = SoftResetStack.MatchOpt key
      if matchkey srs.stacktop
       then snd srs.stacktop.Value
      elif matchkey srs.stacktopshadow
       then snd srs.stacktopshadow.Value
      else
       let tailopt = srs.TailGet key
       if tailopt.IsSome
        then sprintf "%s%s" (key.ToString ()) ((snd tailopt.Value).ToString ())
             |> sprintf "Key %s located in soft reset stack tail."
             |> srs.logfn
             snd tailopt.Value
        else { SoftResetStraightCount= 0; SoftResetShadowedCount= 0; SoftResetShadowCount= 0;
               SoftResetSim= 0; RevertCount= 0; LoadCount= 0 }
    
    /// Insert or update keyed counter.
    member private srs.Put (key: SoftResetRecord, count: SoftResetCounter) =
      let matchkey = SoftResetStack.MatchOpt key
      if matchkey srs.stacktop
       then srs.stacktop <- Some (key, count)
      elif matchkey srs.stacktopshadow
       then srs.stacktopshadow <- Some (key, count)
      else
       do srs.PushToTail () // (if present) move the top items to the tail
       do srs.TailGet key |> ignore // (if present) remove they keyed count from the tail
       srs.stacktop <- Some (key, count)
    
    /// Insert or update keyed counter.
    member private srs.Put ((mkey,mcount),(skey,scount)) =
      if SoftResetStack.MatchOpt mkey srs.stacktopshadow
         || SoftResetStack.MatchOpt skey srs.stacktop
       then
        let p (opt: SoftResetItemOption) =
          match opt with
          | None -> "{no item}"
          | Some (k, c) -> sprintf "%s%s" (k.ToString ()) (c.ToString ())
        let msg = "Cross matched shadow items on the top of the soft reset stack;"
                  + sprintf " existing pair %s / %s; new pair %s / %s."
                    (p srs.stacktop) (p srs.stacktopshadow) (p (Some (mkey,mcount))) (p (Some (skey,scount)))
        srs.logfn msg
        LogWarn msg
      // in any case, push to tail, remove old copies and store new values
      do srs.PushToTail ()
      do srs.TailGet mkey |> ignore
      do srs.TailGet skey |> ignore
      srs.stacktop <- Some (mkey, mcount)
      srs.stacktopshadow <- Some (skey, scount)
    
    /// Empties the soft reset stack.
    /// Remark: The 'logfn' variable is kept as is.
    member private srs.Clear () =
      srs.stacktop <- None
      srs.stacktopshadow <- None
      srs.stacktail.Clear ()
    
    /// TODO (the only public function of the class - except see 'IsSimulationContext' above)
    member public srs.UpdateSoftResetStack (context: SoftResetContext, debuglogfn: string -> unit, issimulation: Lazy<bool>) =
      do srs.logfn <- debuglogfn // update the log writer at every opportunity
      match context with
      | Hard
        -> srs.Clear ()
      | Soft gameparam
        -> let currentgame = SoftResetRecord.CurrentGame ()
           let currentcount = srs.Pop currentgame
           let sim = if issimulation.Value then 1 else 0
           let gpargame = SoftResetRecord.CurrentGame gameparam
           if gpargame = currentgame
            then
             let newcount = { currentcount with SoftResetStraightCount= currentcount.SoftResetStraightCount + 1; SoftResetSim= currentcount.SoftResetSim + sim }
             srs.Put (currentgame, newcount)
            else
             let gparcount = srs.Pop gpargame
             let newcount = { currentcount with SoftResetShadowedCount= currentcount.SoftResetShadowedCount + 1; SoftResetSim= currentcount.SoftResetSim + sim }
             let newgparcount = { gparcount with SoftResetShadowCount= gparcount.SoftResetShadowCount + 1; SoftResetSim= gparcount.SoftResetSim + sim }
             srs.Put ((currentgame, newcount), (gpargame, newgparcount))
      | RevertLoad
        -> let currentgame = SoftResetRecord.CurrentGame ()
           let currentcount = srs.Pop currentgame
           let newcount = { currentcount with RevertCount= currentcount.RevertCount + 1 }
           srs.Put (currentgame, newcount)
      | StdLoad
        -> let currentgame = SoftResetRecord.CurrentGame ()
           let currentcount = srs.Pop currentgame
           let newcount = { currentcount with LoadCount= currentcount.LoadCount + 1 }
           srs.Put (currentgame, newcount)
      let prt1 (opt: SoftResetItemOption) =
        match opt with
        | None -> "{no item}"
        | Some (k, _) -> k.ToString ()
      let prt2 (opt: SoftResetItemOption) =
        match opt with
        | None -> "(no count)"
        | Some (_, c) -> c.ToString ()
      srs.logfn <| sprintf "Debug: UpdateSoftResetStack; result of %O reset;" context
      srs.logfn <| sprintf "       stack top (main): %s," (prt1 srs.stacktop)
      srs.logfn <| sprintf "                  count: %s." (prt2 srs.stacktop)
      srs.logfn <| sprintf "     stack top (shadow): %s," (prt1 srs.stacktopshadow)
      srs.logfn <| sprintf "                  count: %s." (prt2 srs.stacktopshadow)
  
