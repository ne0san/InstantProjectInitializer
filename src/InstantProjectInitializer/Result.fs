[<RequireQualifiedAccess>]
// TODO: ResultもしくはValidationモジュールの拡張に変える
// TODO: 指定のエラーだけ変換できるようにする

// NOTE: System.AggregateException内部のSystem.Net.Http.HttpRequestException
// NOTE: そもそもfshttpnに置き換えたら変わるのでは
module Result

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

