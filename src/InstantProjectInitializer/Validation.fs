[<RequireQualifiedAccess>]
module Validation

let ofCatch (exMatcher: exn -> bool) (f: 'a -> 'b) (a: 'a) =
    try f a |> Ok
    with | ex ->
        if exMatcher ex then
            Error [ ex.Message ]
        else
            raise ex


let ofAsyncCatch (exMatcher: exn -> bool) (f: 'a -> Async<'b>) (a: 'a) =
    async {
        try
            let! x = f a
            return Ok x
        with
        | ex ->
            if exMatcher ex then
                return Error [ ex.Message ]
            else
                return raise ex
    }

