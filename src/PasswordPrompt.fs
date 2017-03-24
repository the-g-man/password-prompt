module PasswordPrompt

open System.Reflection
open System.IO
open System.Diagnostics

type IconState = Lock | Wait | Warn

type Action = Typing | Password of string | ProcessExit of int | Quit

let getFilePath (name : string) =
  let assembly = Assembly.GetExecutingAssembly ()
  let dir = Path.GetDirectoryName assembly.Location
  dir + Path.DirectorySeparatorChar.ToString () + name

type IconManager () =

  let lockIcon = new Gtk.Image (getFilePath "glyphicons-204-lock.png")
  let waitIcon = new Gtk.Image (getFilePath "glyphicons-541-hourglass.png")
  let warnIcon = new Gtk.Image (getFilePath "glyphicons-79-warning-sign.png")

  let icon newState =
    match newState with
    | Lock -> lockIcon
    | Wait -> waitIcon
    | Warn -> warnIcon

  let mutable state = Lock
  let mutable currentIcon = icon state

  let makeBox () =
    let box = new Gtk.Box (Gtk.Orientation.Horizontal, 0)
    box.BorderWidth <- uint32 2
    box.Add currentIcon
    box

  let box = makeBox ()

  member this.Box = box

  member this.Change newState =
    if newState = state then ()
    else
      state <- newState
      box.Remove currentIcon
      currentIcon <- icon state
      box.Add currentIcon
      currentIcon.Show ()

type EntryManager (action : Action Event) =

  let entry =
    let e = new Gtk.Entry ""
    e.Visibility <- false
    e.Changed.Add (fun _ -> Typing |> action.Trigger)
    e.Activated.Add (fun _ -> Password e.Text |> action.Trigger)
    e

  member this.Entry = entry

  member this.Clear () = this.Entry.Text <- ""

type WindowManager (title : string) =

  let action = new Event<Action> ()
  let iconManager = new IconManager ()
  let entryManager = new EntryManager (action)

  let box =
    let spacing = 2
    let b = new Gtk.Box (Gtk.Orientation.Horizontal, spacing)
    b.Add iconManager.Box
    b.Add entryManager.Entry
    b

  let window =
    new Gtk.Window (title)
    |> fun w ->
      w.Add box
      w.WindowPosition <- Gtk.WindowPosition.Center
      w.DeleteEvent.Add (fun _ -> action.Trigger Quit)
      w.ShowAll ()
      w

  member this.Action = action

  member this.ChangeIcon = iconManager.Change

  member this.ClearAndChangeIcon icon =
    entryManager.Clear ()
    iconManager.Change icon

  member this.Quit = Gtk.Application.Quit

let runScriptWithPassword script action password =
  let startInfo =
    new ProcessStartInfo (
      FileName = script,
      Arguments = password,
      CreateNoWindow = true
    )
  let proc =
    new Process (
      StartInfo = startInfo,
      EnableRaisingEvents = true
    )
  let handleExit _ =
    let exitCode = proc.ExitCode
    proc.Dispose ()
    ProcessExit exitCode |> action
  proc.Exited.Add handleExit
  proc.Start () |> ignore

let dispatch script (windowManager : WindowManager) action =
  let handleAsyncResult action =
    Gtk.Application.Invoke (fun _ _ -> windowManager.Action.Trigger action)
  let handlePassword = runScriptWithPassword script handleAsyncResult
  match action with
  | Typing -> windowManager.ChangeIcon Lock
  | Password password -> windowManager.ChangeIcon Wait ; handlePassword password
  | ProcessExit code ->
      if code = 0
      then windowManager.Quit ()
      else windowManager.ClearAndChangeIcon Warn
  | Quit -> windowManager.Quit ()

[<EntryPoint>]
let main argv =
  Gtk.Application.Init ()
  let w = new WindowManager (argv.[0])
  Event.add (dispatch argv.[1] w) w.Action.Publish
  Gtk.Application.Run ();
  0 // return an integer exit code
