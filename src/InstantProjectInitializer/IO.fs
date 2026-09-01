module IO

open System.Text.Json
open FsHttp
open FsToolkit.ErrorHandling


// NOTE: 将来的にHttpClientをコンポジションルートから注入させる可能性あり
// NOTE: ↑この場合、IOTests.fsの異常系でタイムアウトパターンを確実に検証できる状態にする
let private httpTimeout = 30_000.0
let private taskCanceledExceptionMacher (e: exn) =
    e :? System.Threading.Tasks.TaskCanceledException

/// <summary>
/// Request.sendAsync時のタイムアウト例外をErrorとして扱う
/// </summary>
let private requestSendAsyncTimeoutValidation = Validation.ofAsyncCatch taskCanceledExceptionMacher Request.sendAsync



/// <summary>
/// gitignore.ioからgitignoreの内容を取得
/// </summary>
/// <param name="langs">言語一覧</param>
/// <returns>gitignoreファイル内容</returns>
let requestGitignoreIO (langs: string list) : AsyncValidation<string, string> =
    let url = langs
                |> String.concat ","
                |> sprintf "https://www.toptal.com/developers/gitignore/api/%s"

    asyncValidation {
        let! response =
            http {
                GET url
                config_transformHttpClient (fun httpClient ->
                    httpClient.Timeout <- System.TimeSpan.FromMilliseconds httpTimeout
                    httpClient)
            }
            |> requestSendAsyncTimeoutValidation
        match response |> Response.toResult with
            | Ok res -> return! res |> Response.toTextAsync
            | Error errorRes ->
                let! msg = errorRes |> Response.toTextAsync
                return! Error [ msg ]
        }

type DevenvNewResponse = { devenv_nix: string; devenv_yaml: string }
/// <summary>
/// devenvNewのAPIから生成されるdevenv関連ファイルを取得
/// </summary>
/// <param name="langs">指定言語一覧</param>
/// <returns>devenv.nix,devenv.yaml</returns>
let requestDevenvNewApi (langs: string list) : AsyncValidation<(string * string), string> =
    let url = langs
                |> String.concat ","
                |> sprintf "https://devenv.new/api/generate?q=%s"
    asyncValidation {
        let! response =
            http {
                POST url
                config_transformHttpClient (fun httpClient ->
                    httpClient.Timeout <- System.TimeSpan.FromMilliseconds httpTimeout
                    httpClient)
            }
            |> requestSendAsyncTimeoutValidation
        match response |> Response.toResult with
            | Ok res ->
                let! jsonBody = res |> Response.toJsonAsync
                let devenvRes = jsonBody.Deserialize<DevenvNewResponse>()
                return devenvRes.devenv_nix, devenvRes.devenv_yaml
            | Error errorRes ->
                let! msg = errorRes |> Response.toTextAsync
                return! Error [ msg ]
    }
