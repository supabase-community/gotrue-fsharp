﻿open GoTrue

let baseUrl = "https://<project-id>.supabase.co/auth/v1"
let apiKey = "<api-key>"

let connection = goTrueConnection {
     url baseUrl
     headers (Map [ "apiKey", apiKey
                    "Authorization", $"Bearer {apiKey}" ]
     )
}

let result = connection |> signInWithEmail "<email>" "<password>" None |> Async.RunSynchronously
match result with
| Ok r    -> printf $"{r}"
| Error e -> printfn $"{e}"