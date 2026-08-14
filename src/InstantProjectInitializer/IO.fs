module IO

open System
open System.Net.Http
open Microsoft.Playwright
open FsHttp
open FsToolkit.ErrorHandling

// TODO:テストコード
let requestGitignoreIO (langs: string list) : AsyncValidation<string, string> =
    let url = langs
                |> String.concat ","
                |> sprintf "https://www.toptal.com/developers/gitignore/api/%s"

    let request =
        async {
            let! response =
                http {
                    GET url
                }
                |> Request.sendAsync

            let! body =
                response
                |> Response.toTextAsync

            return body
        }

    // NOTE: System.AggregateException内部にSystem.Net.Http.HttpRequestExceptionだけある状態をErrorに変換
    request
    |> Async.Catch
    |> Async.map (function
        | Choice1Of2 body -> Ok body
        | Choice2Of2 ex ->
            match ex with
            | :? AggregateException as aex ->
                match List.ofSeq aex.InnerExceptions with
                | [ :? HttpRequestException as httpEx ] -> Error [ httpEx.Message ]
                | _ -> raise ex
            | _ -> raise ex)

// TODO: 仕組みがわからん
let private extractFileText (page: IPage) (filename: string) : Async<Result<string, string>> =
    async {
        try
            let block =
                page
                    .Locator("div.relative", PageLocatorOptions(HasText = filename))
                    .First

            do! block.WaitForAsync() |> Async.AwaitTask

            let! text = block.Locator("pre").InnerTextAsync() |> Async.AwaitTask

            if String.IsNullOrWhiteSpace text then
                return Error $"{filename}: 中身が空だった(未知の構造の可能性)"
            else
                return Ok text
        with
        | :? TimeoutException ->
            return Error $"{filename}: タイムアウトまでに要素が見つからなかった"
    }


let requestDevenvNew (langs: string list) : AsyncValidation<(string * string), string> =
    let url = langs
                |> String.concat ","
                |> sprintf "https://devenv.new/?q=%s"
    let defaultTimeoutMs = 30_000.0f
    asyncValidation {
        try
            use! playwright = Playwright.CreateAsync() |> Async.AwaitTask
            let! browser =
                playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = true))
                |> Async.AwaitTask
            let! page = browser.NewPageAsync() |> Async.AwaitTask
            page.SetDefaultTimeout defaultTimeoutMs

            do! page.GotoAsync url |> Async.AwaitTask |> Async.Ignore

            // どちらも評価してから合成する(fail-fastにしない)
            let! nixResult = extractFileText page "devenv.nix"
            and! yamlResult = extractFileText page "devenv.yaml"

            return nixResult, yamlResult
        with ex ->
            return! Error [ ex.Message ]
    }

// TODO: ファイル生成
