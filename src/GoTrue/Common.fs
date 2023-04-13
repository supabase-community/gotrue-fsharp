namespace GoTrue.Common

open System.Net.Http.Headers


[<AutoOpen>]
module Common =
    let internal addRequestHeaders (headers: Map<string, string>) (httpRequestHeaders: HttpRequestHeaders): unit =
        headers |> Seq.iter (fun (KeyValue(k, v)) -> httpRequestHeaders.Add(k, v))