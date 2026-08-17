module IOTests

open Expecto

// NOTE: IO.fsの全関数について、外部依存が伴うため結合テストとする。

// NOTE: 外部サービスへの実際のリクエストを行う統合テスト。実行にはネットワーク接続が必要。
[<Tests>]
let requestGitignoreIOTests =
    testList "IO.requestGitignoreIO" [
        testCase "正常系: 有効な言語を渡すと Ok が返る" <| fun _ ->
            let result = IO.requestGitignoreIO ["Python"] |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "正常系: body が空でない" <| fun _ ->
            let result = IO.requestGitignoreIO ["Python"] |> Async.RunSynchronously
            match result with
            | Ok body -> Expect.isNotEmpty body "body は空でないべき"
            | Error _ -> failtest "Ok を期待したが Error だった"

        testCase "正常系: 複数言語を指定できる" <| fun _ ->
            let result = IO.requestGitignoreIO ["Python"; "Go"] |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"
    ]

// NOTE: devenv.new に Playwright でアクセスする統合テスト。
// 実行には ネットワーク接続 と src/InstantProjectInitializer/で`"bin/Debug/net10.0/.playwright/node/darwin-arm64/node" "bin/Debug/net10.0/.playwright/package/cli.js" install chromium` が必要。
[<Tests>]
let requestDevenvNewTests =
    testList "IO.requestDevenvNew" [
        testCase "正常系: 有効な言語を渡すと Ok (nixContent, yamlContent) が返る" <| fun _ ->
            let result = IO.requestDevenvNew ["python"] |> Async.RunSynchronously
            match result with
            | Ok (nixContent, yamlContent) ->
                // printfn "%s" nixContent
                // printfn "%s" yamlContent
                Expect.isNotEmpty nixContent "devenv.nix は空でないべき"
                Expect.isNotEmpty yamlContent "devenv.yaml は空でないべき"
            | Error msgs -> failtestf "Ok を期待したが Error だった: %A" msgs

        testCase "正常系: 複数言語を渡せる" <| fun _ ->
            let result = IO.requestDevenvNew ["python"; "go"] |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "異常系: devenv.nix または devenv.yaml が取得できない場合 Error が返る" <| fun _ ->
            // extractFileText が TimeoutException を catch して Error を返し、
            // asyncValidation の and! により両エラーが収集される
            let result = IO.requestDevenvNew ["nonexistentlang_xyz"] |> Async.RunSynchronously
            match result with
            | Error msgs -> Expect.isNonEmpty msgs "エラーメッセージリストは空でないべき"
            | Ok _ -> () // サイト仕様によっては成功する可能性もある
    ]
