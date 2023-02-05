namespace GoTrue.Http

open System.Net.Http
open FSharp.Json
open GoTrue.Connection
open GoTrue.Common
open Microsoft.FSharp.Core

[<AutoOpen>]
module Http =
    type AuthError = {
        message: string
        statusCode: System.Net.HttpStatusCode
    }
    
    let private getResponseBody (responseMessage: HttpResponseMessage): string = 
        responseMessage.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously        
            
    let deserializeResponse<'T> (response: Result<HttpResponseMessage, AuthError>): Result<'T, AuthError> =
        match response with
        | Ok r    -> Result.Ok (Json.deserialize<'T> (r |> getResponseBody))
        | Error e -> Result.Error e
        
    let deserializeEmptyResponse (response: Result<HttpResponseMessage, AuthError>): Result<unit, AuthError> =
        match response with
        | Ok _    -> Result.Ok ()
        | Error e -> Result.Error e
        
    let post (url: string) (content: StringContent) (httpClient: HttpClient)
             (connection: GoTrueConnection): Result<HttpResponseMessage, AuthError> =
        try
            let result =
                task {
                    httpClient |> addRequestHeaders (Map.toList connection.Headers)
                    
                    let response = httpClient.PostAsync(url, content)
                    return! response
                } |> Async.AwaitTask |> Async.RunSynchronously
            match result.StatusCode with
            | System.Net.HttpStatusCode.OK ->
                Result.Ok result
            | statusCode                   ->
                Result.Error { message = result |> getResponseBody
                               statusCode = statusCode }
        with e ->
            Result.Error { message = e.ToString()
                           statusCode = System.Net.HttpStatusCode.BadRequest }