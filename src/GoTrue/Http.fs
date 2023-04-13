namespace GoTrue

open System.Net
open System.Net.Http
open FSharp.Json
open GoTrue.Connection
open GoTrue.Common

[<AutoOpen>]
module Http =
    type GoTrueError = {
        message: string
        statusCode: HttpStatusCode option
    }
    
    let private getResponseBody (responseMessage: HttpResponseMessage): string = 
        responseMessage.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously
            
    let deserializeResponse<'T> (response: Result<HttpResponseMessage, GoTrueError>): Result<'T, GoTrueError> =        
        match response with
        | Ok r    ->
            printfn $"{r.RequestMessage}"
            Result.Ok (Json.deserialize<'T> (r |> getResponseBody))
        | Error e -> Result.Error e
        
    let deserializeEmptyResponse (response: Result<HttpResponseMessage, GoTrueError>): Result<unit, GoTrueError> =
        match response with
        | Ok _    -> Result.Ok ()
        | Error e -> Result.Error e
        
    let executeHttpRequest (headers: Map<string, string> option) (requestMessage: HttpRequestMessage)
                           (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        try
            let httpClient = connection.HttpClient
            let result =
                task {
                    addRequestHeaders connection.Headers requestMessage.Headers
                    
                    match headers with
                    | Some h -> addRequestHeaders h requestMessage.Headers
                    | _      -> ()
                    
                    let response = httpClient.SendAsync(requestMessage)
                    return! response
                } |> Async.AwaitTask |> Async.RunSynchronously
            match result.StatusCode with
            | HttpStatusCode.OK -> Result.Ok result
            | statusCode        ->
                Result.Error { message    = getResponseBody result
                               statusCode = Some statusCode }
        with e ->
            Result.Error { message    = e.ToString()
                           statusCode = None }
            
    let private getRequestMessage (httpMethod: HttpMethod) (url: string) (urlSuffix: string): HttpRequestMessage =
        new HttpRequestMessage(httpMethod, $"{url}/{urlSuffix}")
        
    let get (urlSuffix: string) (headers: Map<string, string> option)
            (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Get connection.Url urlSuffix

        executeHttpRequest headers requestMessage connection
        
    let delete (urlSuffix: string) (headers: Map<string, string> option) (content: HttpContent option)
               (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Delete connection.Url urlSuffix
        match content with
        | Some c -> requestMessage.Content <- c
        | _      -> ()
        
        executeHttpRequest headers requestMessage connection 
    
    let post (urlSuffix: string) (headers: Map<string, string> option) (content: StringContent)
             (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Post connection.Url urlSuffix
        requestMessage.Content <- content
        
        executeHttpRequest headers requestMessage connection 
            
    let patch (urlSuffix: string) (headers: Map<string, string> option) (content: StringContent)
              (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Patch connection.Url urlSuffix
        requestMessage.Content <- content
        
        executeHttpRequest headers requestMessage connection
        
    let put (urlSuffix: string) (headers: Map<string, string> option) (content: StringContent)
             (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Put connection.Url urlSuffix
        requestMessage.Content <- content
        
        executeHttpRequest headers requestMessage connection 