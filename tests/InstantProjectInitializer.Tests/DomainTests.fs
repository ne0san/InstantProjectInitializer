module DomainTests

open Expecto
open FsToolkit.ErrorHandling

[<Tests>]
let getGitignoreTests =
    testList "getGitignore" [
        testCase "成功時、.gitignore の fileData を返す" <| fun _ ->
            let langs = [ "F#"; "Python" ]
            let stub = fun _ -> AsyncValidation.ok "gitignore content"
            let result = Domain.getGitignore langs stub |> Async.RunSynchronously
            Expect.equal result (Ok { Domain.FileName = ".gitignore"; Domain.FileData = "gitignore content" }) ""

        testCase "requestGitignore に langs が渡され、一度だけ呼ばれる" <| fun _ ->
            let langs = [ "F#"; "Python" ]
            let mutable calledWith = []
            let mutable callCount = 0
            let stub = fun ls ->
                calledWith <- ls
                callCount <- callCount + 1
                AsyncValidation.ok "data"
            Domain.getGitignore langs stub |> Async.RunSynchronously |> ignore
            Expect.equal calledWith langs "指定した langs が渡されるべき"
            Expect.equal callCount 1 "一度だけ呼ばれるべき"

        testCase "異常系: requestGitignore が Error を返す場合、Error を返す" <| fun _ ->
            let stub = fun _ -> AsyncValidation.error "fetch failed"
            let result = Domain.getGitignore [] stub |> Async.RunSynchronously
            Expect.equal result (Error [ "fetch failed" ]) ""
    ]

[<Tests>]
let getDevenvTests =
    testList "getDevenv" [
        testCase "成功時、devenv.nix と devenv.yaml の fileData を返す" <| fun _ ->
            let langs = [ "F#" ]
            let stub = fun _ -> AsyncValidation.ok ("nix content", "yaml content")
            let result = Domain.getDevenv langs stub |> Async.RunSynchronously
            let expected =
                Ok (
                    { Domain.FileName = "devenv.nix";  Domain.FileData = "nix content" },
                    { Domain.FileName = "devenv.yaml"; Domain.FileData = "yaml content" }
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
            Domain.getDevenv langs stub |> Async.RunSynchronously |> ignore
            Expect.equal calledWith langs "指定した langs が渡されるべき"
            Expect.equal callCount 1 "一度だけ呼ばれるべき"

        testCase "異常系: requestDevenv が Error を返す場合、Error を返す" <| fun _ ->
            let stub = fun _ -> AsyncValidation.error "fetch failed"
            let result = Domain.getDevenv [] stub |> Async.RunSynchronously
            Expect.equal result (Error [ "fetch failed" ]) ""
    ]

[<Tests>]
let getDirenvTests =
    testList "getDirenv" [
        testCase "FileName が .envrc である" <| fun _ ->
            let result = Domain.getDirenv ()
            Expect.equal result.FileName ".envrc" ""

        testCase "FileData に use devenv が含まれる" <| fun _ ->
            let result = Domain.getDirenv ()
            Expect.stringContains result.FileData "use devenv" ""

        testCase "FileData に eval \"$(devenv direnvrc)\" が含まれる" <| fun _ ->
            let result = Domain.getDirenv ()
            Expect.stringContains result.FileData "eval \"$(devenv direnvrc)\"" ""
    ]
