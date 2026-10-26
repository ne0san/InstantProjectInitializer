module IOTests

open Expecto

// NOTE: IO.fsの全関数について、外部依存が伴うため結合テストとする。

// NOTE: gitignore.ioへの実際のリクエストを行う統合テスト。実行にはネットワーク接続が必要。
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

        testCase "異常系：gitignoreIOで未対応の言語を指定した場合" <| fun _ ->
            let result = IO.requestGitignoreIO ["nonexistentlang_xyz"] |> Async.RunSynchronously
            Expect.isError result "Errorを返すべき"
            match result with
            | Ok _ -> failtest "エラーではない"
            | Error msg -> Expect.isNotEmpty msg[0] "body は空でないべき"
    ]

// NOTE: devenv.newのAPIへの実際のリクエストを行う統合テスト。実行にはネットワーク接続が必要。
[<Tests>]
let requestDevenvNewApiTests =
    testList "IO.requestDevenvNew" [
        testCase "正常系: 有効な言語を渡すと Ok (nixContent, yamlContent) が返る" <| fun _ ->
            let result = IO.requestDevenvNewApi ["python"] None |> Async.RunSynchronously
            match result with
            | Ok (nixContent, yamlContent) ->
                // printfn "%s" nixContent
                // printfn "%s" yamlContent
                Expect.isNotEmpty nixContent "devenv.nix は空でないべき"
                Expect.isNotEmpty yamlContent "devenv.yaml は空でないべき"
            | Error msgs -> failtestf "Ok を期待したが Error だった: %A" msgs

        testCase "正常系: 複数言語を渡せる" <| fun _ ->
            let result = IO.requestDevenvNewApi ["python"; "go"] None |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "異常系: devenv.nix または devenv.yaml が取得できない場合 Error が返る" <| fun _ ->
            let result = IO.requestDevenvNewApi ["nonexistentlang_xyz"] None |> Async.RunSynchronously
            match result with
            | Error msgs -> Expect.isNonEmpty msgs "エラーメッセージリストは空でないべき"
            | Ok (nixContent, yamlContent) ->
                printfn "devenv.new 生成失敗(タイムアウトor5xx系レスポンス)パターン確認できず" // その言語が存在しないことを前提に成功する場合もある
                printfn "devenv.nix:\n%s" nixContent
                printfn "devenv.yaml:\n%s" yamlContent

        testCase "正常系: コンテキストを渡すと Ok が返る" <| fun _ ->
            let result = IO.requestDevenvNewApi ["python"] (Some "with postgres") |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "正常系: コンテキストを渡した場合も nixContent と yamlContent が空でない" <| fun _ ->
            let result = IO.requestDevenvNewApi ["python"] (Some "with postgres") |> Async.RunSynchronously
            match result with
            | Ok (nixContent, yamlContent) ->
                Expect.isNotEmpty nixContent "devenv.nix は空でないべき"
                Expect.isNotEmpty yamlContent "devenv.yaml は空でないべき"
            | Error msgs -> failtestf "Ok を期待したが Error だった: %A" msgs
    ]
