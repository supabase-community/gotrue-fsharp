namespace GoTrue

open System.Net.Http
open System.Text
open FSharp.Json
open GoTrue.Common
open GoTrue.Connection
open GoTrue.Http

/// Contains helper functions and types for performing auth request 
module AuthRequestCommon =
    /// Represents options for auth request
    type AuthOptions = {
        redirectTo: string option
        scopes: string option
    }
        
    /// Represents helper builder pro checking `Option` types
    type MaybeBuilder() =
        member this.Bind(m, f) = Option.bind f m
        member this.Return(x) = Some x

    let maybe = MaybeBuilder()
    
    /// Creates valid url param string from list of url params
    let internal getUrlParamsString (urlParams: string list): string =
        if urlParams.IsEmpty then "" else "?" + (String.concat "&" urlParams)
    
    /// Performs base auth request. Handles body, headers and url of request.
    /// Deserializes response with `deserializeWith` function.
    let performAuthRequest<'T> (body: Map<string, obj> option) (headers: Map<string, string> option) (urlParams: string list)
                               (pathSuffix: string) (options: AuthOptions option) (connection: GoTrueConnection)
                               (deserializeWith: Result<HttpResponseMessage, GoTrueError> -> Result<'T, GoTrueError>): Result<'T, GoTrueError> =
        try
            let serializedBody =
                match body with
                | Some b -> Json.serialize b
                | _      -> ""
            
            let content = getStringContent serializedBody
            
            let redirectTo =
                maybe {
                    let! authOptions = options
                    let! rt = authOptions.redirectTo
                    return rt
                }
            
            let updatedUrlParams =
                match redirectTo with
                | Some redirectUrl -> urlParams @ [ $"redirect_to={redirectUrl}" ]
                | None   -> urlParams
            let queryString = getUrlParamsString updatedUrlParams
            let urlSuffix = $"{pathSuffix}{queryString}"
            
            let response = post urlSuffix headers content connection
            deserializeWith response
        with
            | :? System.NullReferenceException as ex ->
                Error { message = ex.Message ; statusCode = None }
            | e ->
                Error { message = e.Message ; statusCode = None }