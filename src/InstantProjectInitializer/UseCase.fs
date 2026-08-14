module Domain

open FsToolkit.ErrorHandling

[<Struct>]
type fileData = { FileName:string; FileData:string }


// gitIgnoreを取得して返却
let getGitignore (langs: string list) (requestGitignore: string list -> AsyncValidation<string,string>): AsyncValidation<fileData, string> =
    let fileName = ".gitignore"

    asyncValidation {
        let! gitignoreData = requestGitignore langs

        return { FileName = fileName; FileData = gitignoreData }
    }

let getDevenv (langs: string list) (requestDevenv: string list -> AsyncValidation<(string * string), string>): AsyncValidation<(fileData * fileData), string> =
    let nixFileName = "devenv.nix"
    let yamlFileName = "devenv.yaml"

    asyncValidation {
        let! devenvNixData, devenvYamlData = requestDevenv langs

        return { FileName = nixFileName; FileData = devenvNixData },
               { FileName = yamlFileName; FileData = devenvYamlData }

    }

let getDirenv () =
    let envrcFileName = ".envrc"
    let envrcData = """
    #!/usr/bin/env bash

    export DIRENV_WARN_TIMEOUT=20s

    eval "$(devenv direnvrc)"

    # `use devenv` supports the same options as the `devenv shell` command.
    #
    # To silence all output, use `--quiet`.
    #
    # Example usage: use devenv --quiet --impure --option services.postgres.enable:bool true
    use devenv
    """
    { FileName = envrcFileName; FileData = envrcData }


let rec putFileDatalist (writeFile: string * string -> unit) (fileDataList: fileData list): unit =
    match fileDataList with
    | head :: tail ->
        writeFile (head.FileName, head.FileData)
        putFileDatalist writeFile tail
    | [] -> ()

let run
    (getGitignoreFn: string list -> AsyncValidation<fileData, string>)
    (getDevenvFn: string list -> AsyncValidation<fileData * fileData, string>)
    (getDirenvFn: unit -> fileData)
    (putFileDataListFn: fileData list -> unit)
    (langs: string list)
    : AsyncValidation<unit, string> =
    asyncValidation {
        let! gitignoreData = getGitignoreFn langs
        and! devenvNixData, devenvYamlData = getDevenvFn langs
        let direnvData = getDirenvFn ()

        let fileDataList = [gitignoreData; devenvNixData; devenvYamlData; direnvData]

        putFileDataListFn fileDataList
    }
