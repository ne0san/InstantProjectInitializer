module Try

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

