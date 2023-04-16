namespace GoTrue

open System
open System.Net.Http
open System.Text
open System.Web
open FSharp.Json
open GoTrue.AuthRequestCommon
open GoTrue.Common
open GoTrue.Connection
open GoTrue.Http

[<AutoOpen>]
module ClientHelpers =
    type UserAttributes = {
        email:    string option
        phone:    string option
        password: string option
        data:     Map<string, obj> option
    }
    
    type Provider =
        | Apple
        | Azure
        | Bitbucket
        | Discord
        | Facebook
        | GitHub
        | GitLab
        | Google
        | KeyCloak
        | LinkedIn
        | Notion
        | Slack
        | Spotify
        | Twitch
        | Twitter
        | WorkOS
        | Zoom
    
    let internal signUp<'T> (body: Map<string, obj>) (urlParams: string list) (options: AuthOptions option) (connection: GoTrueConnection)
                          (deserializeWith: Result<HttpResponseMessage, GoTrueError> -> Result<'T, GoTrueError>): Result<'T, GoTrueError> =
        performAuthRequest<'T> (Some body) None urlParams "signup" options connection deserializeWith
        
    let internal signIn<'T> (body: Map<string, obj>) (urlParams: string list) (pathSuffix: string)
                          (options: AuthOptions option) (connection: GoTrueConnection)
                          (deserializeWith: Result<HttpResponseMessage, GoTrueError> -> Result<'T, GoTrueError>): Result<'T, GoTrueError> =
        performAuthRequest<'T> (Some body) None urlParams pathSuffix options connection deserializeWith
        
    let internal addUrlParamIfPresent (key: string) (value: string option) (urlParams: string list): string list =
        match value with
        | Some v -> urlParams @ [$"{key}={v}"]
        | _      -> urlParams

[<AutoOpen>]
module Client =    
    let signUpWithEmail (email: string) (password: string) (options: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
            "password", password
        ]
        
        signUp<GoTrueSessionResponse> body [] options connection deserializeResponse 
        
    let signUpWithPhone (phone: string) (password: string) (options: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "phone", phone
            "password", password
        ]
        
        signUp<GoTrueSessionResponse> body [] options connection deserializeResponse
        
    let signInWithEmail (email: string) (password: string) (options: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
            "password", password
        ]
        
        signIn<GoTrueSessionResponse> body ["grant_type=password"] "token" options connection deserializeResponse
        
    let signInWithPhone (phone: string) (password: string) (options: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "phone", phone
            "password", password
        ]
        
        signIn<GoTrueSessionResponse> body ["grant_type=password"] "token" options connection deserializeResponse
    
    let getOAuthUrl (provider: Provider) (options: AuthOptions option) (connection: GoTrueConnection): string =
        let redirectTo =
            maybe {
                let! ao = options
                let! r = ao.redirectTo
                return r
            }
        let scopes =
            maybe {
                let! ao = options
                let! s = ao.scopes
                return s
            }
        let providerString = provider.ToString().ToLower()
        
        let urlParams =
            [ $"provider={providerString}" ]
            |> addUrlParamIfPresent "redirect_to" redirectTo
            |> addUrlParamIfPresent "scopes" scopes
            
        let url = connection.Url
        let joinedParams = String.concat "&" urlParams
        $"{url}/authorize?{joinedParams}"
        
    let getSessionFromUrl (originUrl: Uri) (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let url = 
            match String.IsNullOrEmpty originUrl.Query with
            | true  ->
                let decoded = originUrl.ToString().Replace("#", "?")
                Uri decoded
            | false ->
                let decoded = originUrl.ToString().Replace("#", "&")
                Uri decoded

        let query = HttpUtility.ParseQueryString url.Query
        let errorDescription = query.["error_description"]
        
        match String.IsNullOrEmpty errorDescription with
        | false -> Error { message = errorDescription ; statusCode = None }
        | true  -> 
            let accessToken = query.["access_token"]
            let expiresIn = query.["expires_in"]
            let refreshToken = query.["refresh_token"]
            let tokenType = query.["token_type"]
            let providerToken = query.["provider_token"]
            let providerRefreshToken = query.["provider_refresh_token"]

            if String.IsNullOrEmpty accessToken then
                Error { message = "No access_token detected"
                        statusCode = None }
            elif String.IsNullOrEmpty expiresIn then
                Error { message = "No expires_in detected"
                        statusCode = None }
            elif String.IsNullOrEmpty refreshToken then
                Error { message = "No refresh_token detected"
                        statusCode = None }
            elif String.IsNullOrEmpty tokenType then
                Error { message = "No token_type detected"
                        statusCode = None }
            else
                let headers = Map [ "Authorization", $"Bearer {accessToken}" ]
                
                let user = get "user" (Some headers) connection
                let deserializedUser = deserializeResponse<User> user
                
                match deserializedUser with
                | Ok u ->
                    Ok { accessToken          = accessToken 
                         expiresIn            = Int32.Parse expiresIn
                         refreshToken         = refreshToken                         
                         tokenType            = Some tokenType
                         providerToken        = Some providerToken
                         providerRefreshToken = Some providerRefreshToken
                         user                 = Some u }
                | Error e -> Error e
        
    let signInWithProvider (provider: Provider) (options: AuthOptions option)
                           (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let providerUrl = getOAuthUrl provider options connection
        let uri = Uri providerUrl
        getSessionFromUrl uri connection
        
    let signInWithMagicLink (email: string) (options: AuthOptions option)
                            (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[ "email", email ]
        
        signIn<unit> body [] "magiclink" options connection deserializeEmptyResponse
    
    let signInWithEmailOtp (email: string) (options: AuthOptions option)
                           (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[ "email", email ]
        
        signIn<unit> body [] "otp" options connection deserializeEmptyResponse
    
    let signInWithPhoneOtp (phone: string) (options: AuthOptions option)
                           (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[ "phone", phone ]
        
        signIn<unit> body [] "otp" options connection deserializeEmptyResponse
        
    let verifyEmailOtp (email: string) (token: string) (options: AuthOptions option)
                       (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
            "token", token
        ]
        
        signIn<GoTrueSessionResponse> body [] "verify" options connection deserializeResponse
        
    let verifyPhoneOtp (phone: string) (token: string) (options: AuthOptions option)
                       (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "phone", phone
            "token", token
        ]
        
        signIn<GoTrueSessionResponse> body [] "verify" options connection deserializeResponse
        
    let updateUser (attributes: UserAttributes) (token: string)
                   (connection: GoTrueConnection): Result<User, GoTrueError> =
        let content = getStringContent (Json.serialize attributes)
        let headers = Map<string, string>[ "Authorization", $"Bearer {token}" ]
        
        let result = put "user" (Some headers) content connection
        deserializeResponse<User> result
        
    let signOut (connection: GoTrueConnection): Result<unit, GoTrueError> =
        performAuthRequest None None [] "logout" None connection deserializeEmptyResponse
        
    let resetPassword (email: string) (options: AuthOptions option) (captchaToken: string option)
                      (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj> [
            "email", email
            "gotrue_meta_security", Map<string, string>[ "captcha_token", ("", captchaToken) ||> Option.defaultValue ]         
        ]
        
        performAuthRequest<unit> (Some body) None [] "recover" options connection deserializeEmptyResponse
        
    let refreshToken (refreshToken: string) (accessToken: string) (options: AuthOptions option)
                     (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[ "refresh_token", refreshToken ]
        let headers = Map<string, string> [ "Authorization", $"Bearer {accessToken}" ]
            
        performAuthRequest<GoTrueSessionResponse>
            (Some body) (Some headers) ["grant_type=refresh_token"]
            "token" options connection deserializeResponse