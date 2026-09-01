module IntegrationTests

open Expecto
open UseCase
open IO

// NOTE: ファイル出力については`File.WriteAllText`を用いるのみのため、現時点ではファイル自体の作成は行わない

let makeDryRun () =
    // ファイル出力スタブ
    let writeTextCalls = ResizeArray<string * string>()
    let writeTextStub arg =
        writeTextCalls.Add arg

    let dryRunCli = run requestGitignoreIO requestDevenvNewApi writeTextStub

    writeTextCalls, dryRunCli

[<Tests>]
let IntegrationTests =
    ptestList "DryRunIntegrationTest" [
        ptestCase "正常系: 有効な言語を渡す場合" <| fun _ ->
        let writeTexoCalls, dryRunCli = makeDryRun ()
        Expect.equal (1 + 1) 3 "わざと失敗させるテスト"
    ]
// どう書きたいか
// 事前準備、出力ファイルだけ変数に導入させるスタブを全てのテストで用意
// expectoで共通のセットアップをどう書かせるか

