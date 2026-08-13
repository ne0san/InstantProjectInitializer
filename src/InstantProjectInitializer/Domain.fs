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

let putFileData (writeFile: string * string -> unit) (fileData: fileData) =
    writeFile (fileData.FileName, fileData.FileData)

let putFileDatas (writeFile: string * string -> unit) (fileDatas: fileData list) =
    for f in fileDatas do
        putFileData writeFile f


