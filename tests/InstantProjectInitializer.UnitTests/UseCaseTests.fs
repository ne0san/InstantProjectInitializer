module UseCaseTests

open Expecto
open FsToolkit.ErrorHandling

[<Tests>]
let getGitignoreTests =
    testList "getGitignore" [
        testCase "成功時、.gitignore の fileData を返す" <| fun _ ->
            let langs = [ "F#"; "Python" ]
            let stub = fun _ -> AsyncValidation.ok "gitignore content"
            let result = UseCase.getGitignore stub langs |> Async.RunSynchronously
            Expect.equal result (Ok { UseCase.FileName = ".gitignore"; UseCase.FileData = "gitignore content" }) ""

        testCase "requestGitignore に langs が渡され、一度だけ呼ばれる" <| fun _ ->
            let langs = [ "F#"; "Python" ]
            let mutable calledWith = []
            let mutable callCount = 0
            let stub = fun ls ->
                calledWith <- ls
                callCount <- callCount + 1
                AsyncValidation.ok "data"
            UseCase.getGitignore stub langs |> Async.RunSynchronously |> ignore
            Expect.equal calledWith langs "指定した langs が渡されるべき"
            Expect.equal callCount 1 "一度だけ呼ばれるべき"

        testCase "異常系: requestGitignore が Error を返す場合、Error を返す" <| fun _ ->
            let stub = fun _ -> AsyncValidation.error "fetch failed"
            let result = UseCase.getGitignore stub [] |> Async.RunSynchronously
            Expect.equal result (Error [ "fetch failed" ]) ""
    ]

[<Tests>]
let getDevenvTests =
    testList "getDevenv" [
        testCase "成功時、devenv.nix と devenv.yaml の fileData を返す" <| fun _ ->
            let langs = [ "F#" ]
            let stub = fun _ -> AsyncValidation.ok ("nix content", "yaml content")
            let result = UseCase.getDevenv stub langs |> Async.RunSynchronously
            let expected =
                Ok (
                    { UseCase.FileName = "devenv.nix";  UseCase.FileData = "nix content" },
                    { UseCase.FileName = "devenv.yaml"; UseCase.FileData = "yaml content" }
                )
            Expect.equal result expected ""

        testCase "requestDevenv に langs が渡され、一度だけ呼ばれる" <| fun _ ->
            let langs = [ "F#"; "Go" ]
            let mutable calledWith = []
            let mutable callCount = 0
            let stub = fun ls ->
                calledWith <- ls
                callCount <- callCount + 1
                AsyncValidation.ok ("nix", "yaml")
            UseCase.getDevenv stub langs |> Async.RunSynchronously |> ignore
            Expect.equal calledWith langs "指定した langs が渡されるべき"
            Expect.equal callCount 1 "一度だけ呼ばれるべき"

        testCase "異常系: requestDevenv が Error を返す場合、Error を返す" <| fun _ ->
            let stub = fun _ -> AsyncValidation.error "fetch failed"
            let result = UseCase.getDevenv stub [] |> Async.RunSynchronously
            Expect.equal result (Error [ "fetch failed" ]) ""
    ]

[<Tests>]
let getDirenvTests =
    testList "getDirenv" [
        testCase "FileName が .envrc である" <| fun _ ->
            let result = UseCase.getDirenv ()
            Expect.equal result.FileName ".envrc" ""

        testCase "FileData に use devenv が含まれる" <| fun _ ->
            let result = UseCase.getDirenv ()
            Expect.stringContains result.FileData "use devenv" ""

        testCase "FileData に eval \"$(devenv direnvrc)\" が含まれる" <| fun _ ->
            let result = UseCase.getDirenv ()
            Expect.stringContains result.FileData "eval \"$(devenv direnvrc)\"" ""
    ]

[<Tests>]
let putFileDataListTests =
    testList "putFileDataList" [
        testCase "空リストのとき writeFile は呼ばれない" <| fun _ ->
            let mutable callCount = 0
            UseCase.putFileDataList (fun _ -> callCount <- callCount + 1) []
            Expect.equal callCount 0 "呼ばれないべき"

        testCase "単一要素のとき writeFile が一度だけ呼ばれる" <| fun _ ->
            let mutable callCount = 0
            let item = { UseCase.FileName = "a.txt"; UseCase.FileData = "content" }
            UseCase.putFileDataList (fun _ -> callCount <- callCount + 1) [item]
            Expect.equal callCount 1 "一度だけ呼ばれるべき"

        testCase "単一要素のとき正しい引数で writeFile が呼ばれる" <| fun _ ->
            let mutable captured = []
            let item = { UseCase.FileName = "a.txt"; UseCase.FileData = "hello" }
            UseCase.putFileDataList (fun arg -> captured <- arg :: captured) [item]
            Expect.equal captured [("a.txt", "hello")] ""

        testCase "複数要素のとき全要素に対して writeFile が呼ばれる" <| fun _ ->
            let mutable callCount = 0
            let items = [
                { UseCase.FileName = "a.txt"; UseCase.FileData = "a" }
                { UseCase.FileName = "b.txt"; UseCase.FileData = "b" }
                { UseCase.FileName = "c.txt"; UseCase.FileData = "c" }
            ]
            UseCase.putFileDataList (fun _ -> callCount <- callCount + 1) items
            Expect.equal callCount 3 "3回呼ばれるべき"

        testCase "複数要素のとき順番どおりに writeFile が呼ばれる" <| fun _ ->
            let mutable captured = []
            let items = [
                { UseCase.FileName = "first.txt";  UseCase.FileData = "1" }
                { UseCase.FileName = "second.txt"; UseCase.FileData = "2" }
            ]
            UseCase.putFileDataList (fun (name, _) -> captured <- name :: captured) items
            Expect.equal (List.rev captured) ["first.txt"; "second.txt"] "順番どおりに呼ばれるべき"
    ]

[<Tests>]
let runTests =
    testList "run" [
        testCase "成功時、Ok を返す" <| fun _ ->
            let gitignoreStub = fun _ -> AsyncValidation.ok "gi"
            let devenvStub    = fun _ -> AsyncValidation.ok ("nix", "yaml")
            let putStub       = fun _ -> ()
            let result = UseCase.run gitignoreStub devenvStub putStub ["F#"] |> Async.RunSynchronously
            Expect.isOk result "Ok を返すべき"

        testCase "成功時、writeFileFn が 4 回呼ばれる" <| fun _ ->
            let gitignoreStub = fun _ -> AsyncValidation.ok "gi"
            let devenvStub    = fun _ -> AsyncValidation.ok ("nix", "yaml")
            let mutable callCount = 0
            let putStub = fun _ -> callCount <- callCount + 1
            UseCase.run gitignoreStub devenvStub putStub ["F#"] |> Async.RunSynchronously |> ignore
            Expect.equal callCount 4 "4回呼ばれるべき"

        testCase "成功時、writeFileFn に渡されるファイル名に全ファイル名が含まれる" <| fun _ ->
            let gitignoreStub = fun _ -> AsyncValidation.ok "gi"
            let devenvStub    = fun _ -> AsyncValidation.ok ("nix", "yaml")
            let mutable capturedNames = []
            let putStub = fun (name, _) -> capturedNames <- name :: capturedNames
            UseCase.run gitignoreStub devenvStub putStub ["F#"] |> Async.RunSynchronously |> ignore
            Expect.contains capturedNames ".gitignore"  ""
            Expect.contains capturedNames "devenv.nix"  ""
            Expect.contains capturedNames "devenv.yaml" ""
            Expect.contains capturedNames ".envrc"      ""

        testCase "requestGitignoreFn が Error を返す場合、Error を返す" <| fun _ ->
            let gitignoreStub = fun _ -> AsyncValidation.error "gitignore error"
            let devenvStub    = fun _ -> AsyncValidation.ok ("nix", "yaml")
            let putStub       = fun _ -> ()
            let result = UseCase.run gitignoreStub devenvStub putStub [] |> Async.RunSynchronously
            Expect.isError result "Error を返すべき"

        testCase "requestDevenvFn が Error を返す場合、Error を返す" <| fun _ ->
            let gitignoreStub = fun _ -> AsyncValidation.ok "gi"
            let devenvStub    = fun _ -> AsyncValidation.error "devenv error"
            let putStub       = fun _ -> ()
            let result = UseCase.run gitignoreStub devenvStub putStub [] |> Async.RunSynchronously
            Expect.isError result "Error を返すべき"

        testCase "requestGitignoreFn と requestDevenvFn が両方 Error を返す場合、両エラーが収集される" <| fun _ ->
            let gitignoreStub = fun _ -> AsyncValidation.error "gitignore error"
            let devenvStub    = fun _ -> AsyncValidation.error "devenv error"
            let putStub       = fun _ -> ()
            let result = UseCase.run gitignoreStub devenvStub putStub [] |> Async.RunSynchronously
            match result with
            | Error errors -> Expect.hasLength errors 2 "両方のエラーが収集されるべき"
            | Ok _ -> failtest "Error を期待したが Ok だった"

        testCase "Error 時、writeFileFn は呼ばれない" <| fun _ ->
            let gitignoreStub = fun _ -> AsyncValidation.error "error"
            let devenvStub    = fun _ -> AsyncValidation.ok ("nix", "yaml")
            let mutable putCalled = false
            let putStub = fun _ -> putCalled <- true
            UseCase.run gitignoreStub devenvStub putStub [] |> Async.RunSynchronously |> ignore
            Expect.isFalse putCalled "Error 時に writeFileFn は呼ばれないべき"
    ]
