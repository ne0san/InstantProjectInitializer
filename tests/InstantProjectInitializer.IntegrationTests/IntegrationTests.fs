module IntegrationTests

open Expecto
open UseCase
open IO

// NOTE: ファイル出力については`File.WriteAllText`を用いるのみのため、現時点ではファイル自体の作成は行わない

let makeDryRun () =
    let writeTextCalls = ResizeArray<string * string>()
    let writeTextStub arg = writeTextCalls.Add arg
    let dryRunCli = run requestGitignoreIO requestDevenvNewApi writeTextStub
    writeTextCalls, dryRunCli

[<Tests>]
let integrationTests =
    testList "DryRunIntegrationTest" [
        testCase "正常系: 単一言語を渡すと Ok を返す" <| fun _ ->
            let _, dryRunCli = makeDryRun ()
            let result = dryRunCli ["Python"] None |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "正常系: 単一言語を渡すと 4 ファイルが出力される" <| fun _ ->
            let writeTextCalls, dryRunCli = makeDryRun ()
            dryRunCli ["Python"] None |> Async.RunSynchronously |> ignore
            Expect.equal writeTextCalls.Count 4 "4ファイルが出力されるべき"

        testCase "正常系: 出力ファイル名に .gitignore, devenv.nix, devenv.yaml, .envrc が含まれる" <| fun _ ->
            let writeTextCalls, dryRunCli = makeDryRun ()
            dryRunCli ["Python"] None |> Async.RunSynchronously |> ignore
            let fileNames = writeTextCalls |> Seq.map fst |> Set.ofSeq
            Expect.contains fileNames ".gitignore"  ".gitignore が含まれるべき"
            Expect.contains fileNames "devenv.nix"  "devenv.nix が含まれるべき"
            Expect.contains fileNames "devenv.yaml" "devenv.yaml が含まれるべき"
            Expect.contains fileNames ".envrc"      ".envrc が含まれるべき"

        testCase "正常系: 複数言語を渡すと Ok を返す" <| fun _ ->
            let _, dryRunCli = makeDryRun ()
            let result = dryRunCli ["Python"; "Go"] None |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "正常系: 複数言語を渡すと 4 ファイルが出力される" <| fun _ ->
            let writeTextCalls, dryRunCli = makeDryRun ()
            dryRunCli ["Python"; "Go"] None |> Async.RunSynchronously |> ignore
            Expect.equal writeTextCalls.Count 4 "4ファイルが出力されるべき"

        testCase "異常系: 存在しない言語を渡すと Error を返す" <| fun _ ->
            let _, dryRunCli = makeDryRun ()
            let result = dryRunCli ["nonexistentlang_xyz"] None |> Async.RunSynchronously
            Expect.isError result "Error を返すべき"

        testCase "異常系: 存在しない言語を渡した場合、ファイルは出力されない" <| fun _ ->
            let writeTextCalls, dryRunCli = makeDryRun ()
            dryRunCli ["nonexistentlang_xyz"] None |> Async.RunSynchronously |> ignore
            Expect.equal writeTextCalls.Count 0 "Error 時にファイル出力は行われないべき"

        // NOTE: devenv.newは言語指定なしの場合500を返す想定だが、Okが返る場合もあり結果が安定しないためスキップ
        ptestCase "異常系: 言語指定なしの場合 Error を返す" <| fun _ ->
            let _, dryRunCli = makeDryRun ()
            let result = dryRunCli [] None |> Async.RunSynchronously
            Expect.isError result "Error を返すべき"

        ptestCase "異常系: 言語指定なしの場合、ファイルは出力されない" <| fun _ ->
            let writeTextCalls, dryRunCli = makeDryRun ()
            dryRunCli [] None |> Async.RunSynchronously |> ignore
            Expect.equal writeTextCalls.Count 0 "Error 時にファイル出力は行われないべき"

        testCase "正常系: コンテキストを渡すと Ok を返す" <| fun _ ->
            let _, dryRunCli = makeDryRun ()
            let result = dryRunCli ["Python"] (Some "with postgres") |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "正常系: コンテキストを渡すと 4 ファイルが出力される" <| fun _ ->
            let writeTextCalls, dryRunCli = makeDryRun ()
            dryRunCli ["Python"] (Some "with postgres") |> Async.RunSynchronously |> ignore
            Expect.equal writeTextCalls.Count 4 "4ファイルが出力されるべき"
    ]
