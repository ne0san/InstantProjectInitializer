module UseCase

open FsToolkit.ErrorHandling

[<Struct>]
type fileData = { FileName:string; FileData:string }


/// <summary>
/// gitignore用ファイルデータ取得
/// </summary>
/// <param name="requestGitignore">外部にgitignore取得する用関数</param>
/// <param name="langs">言語一覧</param>
/// <returns>生成したgitignore</returns>
let getGitignore (requestGitignore: string list -> AsyncValidation<string,string>) (langs: string list): AsyncValidation<fileData, string> =
    let fileName = ".gitignore"

    asyncValidation {
        let! gitignoreData = requestGitignore langs

        return { FileName = fileName; FileData = gitignoreData }
    }

/// <summary>
/// devenv用ファイルデータ取得
/// </summary>
/// <param name="requestDevenv">外部にgdevenv取得する用関数</param>
/// <param name="langs">言語一覧</param>
/// <returns>生成したdevedv.nixおよびdevenv.yaml</returns>
let getDevenv (requestDevenv: string list -> AsyncValidation<(string * string), string>) (langs: string list): AsyncValidation<(fileData * fileData), string> =
    let nixFileName = "devenv.nix"
    let yamlFileName = "devenv.yaml"

    asyncValidation {
        let! devenvNixData, devenvYamlData = requestDevenv langs

        return { FileName = nixFileName; FileData = devenvNixData },
               { FileName = yamlFileName; FileData = devenvYamlData }

    }

/// <summary>
/// direnv用データ取得
/// </summary>
/// <returns>.envrcデータ</returns>
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

/// <summary>
/// ファイル出力フロー
/// </summary>
/// <param name="writeFile">ファイル書き出し関数</param>
/// <param name="fileDataList">書き出すファイルデータ一覧</param>
/// <returns></returns>
let rec putFileDataList (writeFile: string * string -> unit) (fileDataList: fileData list): unit =
    match fileDataList with
    | head :: tail ->
        writeFile (head.FileName, head.FileData)
        putFileDataList writeFile tail
    | [] -> ()

/// <summary>
/// 全体フロー定義
/// </summary>
let run
    (requestGitignoreFn: string list -> AsyncValidation<string,string>)
    (requestDevenvFn: string list -> AsyncValidation<(string * string), string>)
    (writeFileFn: string * string -> unit)
    (langs: string list)
    : AsyncValidation<unit, string> =
    let getGitignoreFn = getGitignore requestGitignoreFn
    let getDevenvFn = getDevenv requestDevenvFn
    let putFileDataListFn = putFileDataList writeFileFn

    asyncValidation {
        printfn "Start Retrieving Target Files..."
        let! gitignoreData = getGitignoreFn langs
        and! devenvNixData, devenvYamlData = getDevenvFn langs
        let direnvData = getDirenv ()

        printfn "All files were successfully retrieved"
        printfn "Start Exporting File..."

        let fileDataList = [gitignoreData; devenvNixData; devenvYamlData; direnvData]

        putFileDataListFn fileDataList
        printfn "File Export Complete"
    }
