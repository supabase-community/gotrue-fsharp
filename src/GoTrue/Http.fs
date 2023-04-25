namespace GoTrue

open System.Net
open System.Net.Http
open FSharp.Json
open GoTrue.Common
open GoTrue.Connection

/// Contains functions for performing http request and serialization/deserialization of data
[<AutoOpen>]
module Http =
    type GoTrueError = {
        message: string
        statusCode: HttpStatusCode option
    }
    
    /// Parses HttpResponseMessage to it's string form
    let private getResponseBody (responseMessage: HttpResponseMessage): string = 
        responseMessage.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously
            
    /// Deserializes given response
    let deserializeResponse<'T> (response: Result<HttpResponseMessage, GoTrueError>): Result<'T, GoTrueError> =        
        try 
            match response with
            | Ok r    ->
                printfn $"{r.RequestMessage}"
                Result.Ok (Json.deserialize<'T> (getResponseBody r))
            | Error e -> Result.Error e
        with
            | :? System.NullReferenceException as ex ->
                Error { message = ex.Message ; statusCode = None }
            | _ ->
                Error { message = "Unexpected error" ; statusCode = None }
        
    /// Deserializes empty (unit) response
    let deserializeEmptyResponse (response: Result<HttpResponseMessage, GoTrueError>): Result<unit, GoTrueError> =
        match response with
        | Ok _    -> Result.Ok ()
        | Error e -> Result.Error e
        
    /// Executes http response with given headers, requestMessage and handles possible exceptions
    let executeHttpRequest (requestMessage: HttpRequestMessage) (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        try
            let httpClient = connection.HttpClient
            let result =
                task {
                    let connectionHeaders =
                        match requestMessage.Headers.Contains "Authorization" &&
                              connection.Headers.ContainsKey "Authorization" with
                        | true -> connection.Headers.Remove "Authorization"
                        | false -> connection.Headers
                    addRequestHeaders connectionHeaders requestMessage.Headers
                    
                    let response = httpClient.SendAsync(requestMessage)
                    return! response
                } |> Async.AwaitTask |> Async.RunSynchronously
            match result.StatusCode with
            | HttpStatusCode.OK -> Result.Ok result
            | statusCode        -> Result.Error { message    = getResponseBody result
                                                  statusCode = Some statusCode }
        with e -> Result.Error { message    = e.ToString()
                                 statusCode = None }
            
    /// Constructs HttpRequestMessage with given method, url and headers
    let private getRequestMessage (httpMethod: HttpMethod) (url: string) (urlSuffix: string)
                                  (headers: Map<string, string> option): HttpRequestMessage =
        let requestMessage = new HttpRequestMessage(httpMethod, $"{url}/{urlSuffix}")
        match headers with
        | Some h -> addRequestHeaders h requestMessage.Headers
        | _      -> ()
        requestMessage
        
    /// Performs http GET request
    let get (urlSuffix: string) (headers: Map<string, string> option)
            (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Get connection.Url urlSuffix headers

        executeHttpRequest requestMessage connection
        
    /// Performs http DELETE request
    let delete (urlSuffix: string) (headers: Map<string, string> option) (content: HttpContent option)
               (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Delete connection.Url urlSuffix headers
        match content with
        | Some c -> requestMessage.Content <- c
        | _      -> ()
        
        executeHttpRequest requestMessage connection 
    
    /// Performs http POST request
    let post (urlSuffix: string) (headers: Map<string, string> option) (content: StringContent)
             (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Post connection.Url urlSuffix headers
        requestMessage.Content <- content
        
        executeHttpRequest requestMessage connection 
            
    /// Performs http PATCH request
    let patch (urlSuffix: string) (headers: Map<string, string> option) (content: StringContent)
              (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Patch connection.Url urlSuffix headers
        requestMessage.Content <- content
        
        executeHttpRequest requestMessage connection
        
    /// Performs http PUT request
    let put (urlSuffix: string) (headers: Map<string, string> option) (content: StringContent)
             (connection: GoTrueConnection): Result<HttpResponseMessage, GoTrueError> =
        let requestMessage = getRequestMessage HttpMethod.Put connection.Url urlSuffix headers
        requestMessage.Content <- content
        
        executeHttpRequest requestMessage connection 