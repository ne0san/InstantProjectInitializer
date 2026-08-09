open System.Net.Http

type Validation<'ok, 'error> = Result<'ok, 'error list>

type ValidationBuilder() =
    member _.Return x = Ok x
    member _.ReturnFrom vali = vali

    member _.Bind (vali: Validation<'ok1, 'error>, f: 'ok1 -> Validation<'ok2, 'error>): Validation<'ok2, 'error> =
        match vali with
        | Ok x -> f x
        | Error e -> Error e

    member _.MergeSources(x ,y) =
        match x, y with
        | Ok a, Ok b -> Ok (a, b)
        | Error e, Ok _ -> Error e
        | Ok _, Error e -> Error e
        | Error e1, Error e2 -> Error (e1 @ e2)

    // NOTE: 恒等変換を組み込みメンバーとすることで、同名メソッドオーバーライドによる重複時、組み込み側を優先させる
    member _.Source(vali: Validation<'ok, 'error>) : Validation<'ok, 'error> = vali

let validation = ValidationBuilder()

[<AutoOpen>] // 当該モジュール内ではこのモジュールを明示的にopenしなくても見える状態にする
module ValidationBuilderExtensions =
    type ValidationBuilder with
        member _.Source(res: Result<'ok, 'error>) : Validation<'ok, 'error> =
            res |> Result.mapError List.singleton

type AsyncValidation<'ok, 'error> = Async<Validation<'ok, 'error>>

type AsyncValidationBuilder() =
    member _.Return x = async { return Ok x }
    member _.ReturnFrom avali = avali
    member _.Bind (avali: AsyncValidation<'ok1, 'error>, f: 'ok1 -> AsyncValidation<'ok2, 'error>): AsyncValidation<'ok2, 'error> =
        async {
            let! vali = avali
            match vali with
            | Ok x -> return! f x
            | Error e -> return Error e
        }
    member _.Source(avali: AsyncValidation<'ok, 'error>) : AsyncValidation<'ok, 'error> = avali
    member _.MergeSources(x, y) =
        async {
            let! childX = Async.StartChild x   // xが走り始める(待たない)
            let! childY = Async.StartChild y   // yも走り始める(待たない、xと同時進行中)
            let! resultX = childX              // ここで初めてxの結果を待つ
            let! resultY = childY              // yの結果を待つ
            return validation.MergeSources(resultX, resultY)
            // NOTE: return部分をCEで表す場合
            // return validation {
            //     let! x = resultX
            //     and! y = resultY
            //     return x, y
            // }
        }

let asyncValidation = AsyncValidationBuilder()

[<AutoOpen>]
module AsyncValidationBuilderExtensions =
    type AsyncValidationBuilder with
        // NOTE: 以下のSourceは AsyncValidation<'ok,'error> との間でオーバーロード解決が曖昧になるため、拡張側に実装
        member _.Source(ax: Async<'ok>) : AsyncValidation<'ok, 'error> =
            async {
                let! x = ax
                return Ok x
            }
        member _.Source(vali: Validation<'ok, 'error>) : AsyncValidation<'ok, 'error> = async { return vali }

[<Struct>]
type fileData = { FileName:string; FileData:string }


let ofCatch (f: 'a -> 'b) (a: 'a) =
    try f a |> Ok
    with ex -> Error [ ex.Message ]

let ofAsyncCatch (f: 'a -> Async<'b>) (a: 'a) =
    async {
        try
            let! x = f a
            return Ok x
        with
        | ex -> return Error [ ex.Message ]
    }


// TODO: 外部依存を注入?
let fetchGitignore (langs: string list): AsyncValidation<fileData, string> =
    let fileName = ".gitignore"

    let url = langs |> String.concat "," |> sprintf "https://www.toptal.com/developers/gitignore/api/%s"

    // ここでしかリクエストしない&&cliツールで短命のため、HttpClientをシングルトンにはしない。
    let http = new HttpClient()

    // 例外を吐くAsyncをAsync<Result>に変換
    let awaitTaskResult = ofAsyncCatch Async.AwaitTask

    // Task型をAsync型に変換
    let httpGetAsync = http.GetStringAsync url |> awaitTaskResult

    asyncValidation {
        let! gitignoreData = httpGetAsync

        return { FileName=fileName; FileData=gitignoreData }
    }

[<System.Obsolete("fetacDevenv に以降予定")>]
let genDevenv (langs: string list) =
    // TODO: https://devenv.new/?q=fsharp%20go%20next.js(bun) もしくは`devenv mcp`に移行 コレによりdevenv.nix及びdevenv.yml療法を生成
    let fileName = "devenv.nix"

    let fileData = langs
                  |> List.map (fun l -> $"  languages.{l}.enable = true;")
                  |> String.concat "\n"
                  |> sprintf "{ pkgs, ... }:\n{\n%s\n}"

    { FileName=fileName; FileData=fileData }

// let fetchDevenv (langs: string list) AsyncValidation<(fileData, fileData), string> =
//     // TODO: https://devenv.new/?q=langs にアクセスし、生成終了まで待つ、devenv.nixとdevenv.yamlを持ってきて返却
//     // 一秒ごとにポーリング、ソースの構造が現れればそれらを取得、未知の構造ならerr


// TODO: 型変更
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

// コマンドライン引数先頭がファイル名になるためスキップ
let langs = fsi.CommandLineArgs |> Array.skip 1 |> Array.toList


// fetchGitignore langs |> printfn "%s"

// NOTE:
// async Resultのコンピュテーション式を作り始める
// 言語名を、gitignoreとdevenv(さらに対応するnixpkgs)それぞれの名前に対応変換するパーツがいる これnixpkgsにリクエスト要るか？？？(パッケージ存在確認)
// ↑言語名だけだと対応するパッケージが曖昧(jsとかjavaとか)な場合があるのでパッケージは枠だけ作らせるor曖昧な場合だけ(インタラプトさせるor枠だけにする)



