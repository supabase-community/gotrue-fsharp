namespace GoTrue.Common

open System.Net.Http

[<AutoOpen>]
module Common =
    let private addRequestHeader (key: string) (value: string) (client: HttpClient) =
        client.DefaultRequestHeaders.Add(key, value)
    
    let internal addRequestHeaders (headers: (string * string) list) (client: HttpClient) =
        headers
        |> List.iter (fun (key, value) -> client |> addRequestHeader key value)
