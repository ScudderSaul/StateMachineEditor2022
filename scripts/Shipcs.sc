Name: StarShip;

States:  Uninit, Shooting, Flying, Crashed, Destroyed ;

Events: Start, Faster, Slower, Climb, Reverse, 
       Decend, Fire, NoFire, Crash, Switch, EraseNow, DrawNow ;

Actions: Initialize, MoreForward, LessForward, MoveUp, MoveDown, Wreck
       SwitchDirection, FireOne, Draw, Erase, Reset ;

State: Uninit
 Event: Start : Initialize : Flying
 Event: Default : Nop : Uninit ;

State: Shooting
 Event: NoFire : Nop : Flying
 Event: DrawNow : Draw : Shooting
 Event: EraseNow : Erase : Shooting
 Event: Faster : MoreForward : Shooting
 Event: Slower : LessForward : Shooting
 Event: Climb  : MoveUp : Shooting
 Event: Decend : MoveDown : Shooting
 Event: Crash : Wreck : Crashed
 Event: Switch : SwitchDirection : Shooting
 Event: Default: Nop : Shooting ;
 
State: Flying
 Event: Fire : FireOne : Shooting
 Event: DrawNow : Draw : Flying
 Event: EraseNow : Erase : Flying
 Event: Faster : MoreForward : Flying
 Event: Slower : LessForward : Flying
 Event: Climb  : MoveUp : Flying
 Event: Decend : MoveDown : Flying
 Event: Crash : Wreck : Crashed
 Event: Switch : SwitchDirection : Flying
 Event: Default: Nop : Flying ;

State: Crashed
 Event: DrawNow : Draw : Crashed
 Event: EraseNow : Erase : Crashed
 Event: Crash : Draw : Destroyed
 Event: Default: Nop : Crashed ;

State: Destroyed
 Event: DrawNow : Reset : Uninit
 Event: Default: Nop : Destroyed ;

Start: Uninit;

