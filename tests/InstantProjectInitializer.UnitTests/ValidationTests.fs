module ValidationTests

open Expecto

[<Tests>]
let ofCatchTests =
    testList "Validation.ofCatch" [
        testCase "成功時、 Ok を返す" <| fun _ ->
            let result = Validation.ofCatch (fun _ -> false) (fun x -> x + 1) 1
            Expect.equal result (Ok 2) "Ok 2 を返すべき"

        testCase "対象の例外発生時、 Error を返す" <| fun _ ->
            let ex = System.InvalidOperationException("test error")
            let result =
                Validation.ofCatch
                    (fun e -> e :? System.InvalidOperationException)
                    (fun _ -> raise ex)
                    ()
            Expect.equal result (Error [ "test error" ]) "Error [\"test error\"] を返すべき"

        testCase "対象外の例外発生時、再 raise する" <| fun _ ->
            let ex = System.InvalidOperationException("unmatched")
            Expect.throws
                (fun () ->
                    Validation.ofCatch
                        (fun e -> e :? System.ArgumentException)
                        (fun _ -> raise ex)
                        ()
                    |> ignore)
                "マッチしない例外は再 raise されるべき"
    ]
[<Tests>]
let ofAsyncCatchTests =
    testList "Validation.ofAsyncCatch" [
        testCase "成功時、Okを返す" <| fun _ ->
            let result = Validation.ofAsyncCatch (fun _ -> false) (fun x -> async { return x + 1 } ) 1 |> Async.RunSynchronously
            Expect.equal result (Ok 2) "Ok 2 を返すべき"

        testCase "対象の例外発生時、 Error を返す" <| fun _ ->
            let ex = System.InvalidOperationException("test error")
            let result =
                Validation.ofAsyncCatch
                    (fun e -> e :? System.InvalidOperationException)
                    (fun _ -> raise ex)
                    ()
                    |> Async.RunSynchronously
            Expect.equal result (Error [ "test error" ]) "Error [\"test error\"] を返すべき"

        testCase "対象外の例外発生時、再 raise する" <| fun _ ->
            let ex = System.InvalidOperationException("unmatched")
            Expect.throws
                (fun () ->
                    Validation.ofAsyncCatch
                        (fun e -> e :? System.ArgumentException)
                        (fun _ -> raise ex)
                        ()
                        |> Async.RunSynchronously
                    |> ignore)
                "マッチしない例外は再 raise されるべき"
    ]

