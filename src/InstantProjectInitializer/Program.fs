open System
open Argu
open UseCase
open IO

type CliArgs =
    | [<MainCommand>] Langs of lang: string list
    | [<AltCommandLine("-c")>] Context of context: string

    interface IArgParserTemplate with
        member arg.Usage =
            match arg with
            | Langs _   -> "Target language names (multiple languages may be specified, separated by spaces)"
            | Context _ -> "Additional context passed to devenv.new API (e.g. \"with postgres and redis\")"

[<EntryPoint>]
let main argv =
    let parser  = ArgumentParser.Create<CliArgs>(programName = "iip")
    let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    let langs   = results.GetResult Langs
    let context = results.TryGetResult Context

    let runCli = run requestGitignoreIO requestDevenvNewApi File.WriteAllText

    match runCli langs context |> Async.RunSynchronously with
    | Ok () ->
        0
    | Error (errors: string list) ->
        errors |> List.iter (eprintfn "Error: %O")
        1
