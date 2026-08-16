open System
open Argu
open UseCase
open IO

type CliArgs =
    | [<MainCommand>] Langs of lang: string list

    interface IArgParserTemplate with
        member arg.Usage =
            match arg with
            | Langs _   -> "Target language names (multiple languages may be specified, separated by spaces)"

[<EntryPoint>]
let main argv =
    let parser  = ArgumentParser.Create<CliArgs>(programName = "iip")
    let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    let langs   = results.GetResult Langs

    let getGitignoreFn = getGitignore requestGitignoreIO
    let getDevenvFn = getDevenv requestDevenvNew
    let putFileDataListFn = putFileDataList File.WriteAllText

    let runCli = run getGitignoreFn getDevenvFn getDirenv putFileDataListFn

    match langs |> runCli |> Async.RunSynchronously with
    | Ok () ->
        0
    | Error (errors: string list) ->
        errors |> List.iter (eprintfn "Error: %O")
        1
