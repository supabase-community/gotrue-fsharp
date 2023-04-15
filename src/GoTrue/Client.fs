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
    
    let internal signUp<'T> (body: Map<string, obj>) (urlParams: string list) (options: AuthOptions option) (connection: GoTrueConnection)
                          (deserializeWith: Result<HttpResponseMessage, GoTrueError> -> Result<'T, GoTrueError>): Result<'T, GoTrueError> =
        performAuthRequest<'T> (Some body) urlParams "signup" options connection deserializeWith
        
    let internal signIn<'T> (body: Map<string, obj>) (urlParams: string list) (pathSuffix: string)
                          (options: AuthOptions option) (connection: GoTrueConnection)
                          (deserializeWith: Result<HttpResponseMessage, GoTrueError> -> Result<'T, GoTrueError>): Result<'T, GoTrueError> =
        performAuthRequest<'T> (Some body) urlParams pathSuffix options connection deserializeWith
        
    let internal addUrlParamIfPresent (value: string option) (urlParams: string list): string list =
        match value with
        | Some v -> v :: urlParams
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
    
    let private getOAuthUrl (provider: string) (options: AuthOptions option) (connection: GoTrueConnection): string =
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
        
        let urlParams =
            [ $"provider={provider}" ]
            |> addUrlParamIfPresent redirectTo
            |> addUrlParamIfPresent scopes
            
        let url = connection.Url
        let joinedParams = String.concat "&" urlParams
        $"{url}/authorize?{joinedParams}"
        
    let signInWithProvider (provider: string) (options: AuthOptions option) (connection: GoTrueConnection) =
        let providerUrl = connection |> getOAuthUrl provider options
        let uri = Uri(providerUrl)
        let x = HttpUtility.ParseQueryString(provider)
        printfn $"{providerUrl}"
        provider
        
    let signInWithMagicLink (email: string) (options: AuthOptions option) (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
        ]
        
        signIn<unit> body [] "magiclink" options connection deserializeEmptyResponse
    
    // let inviteUserByEmail (email: string) (options: AuthOptions option) (bearer: string) (connection: GoTrueConnection)
    //                   : Result<GoTrueSessionResponse, AuthError> =
    //     let httpClient = new HttpClient()
    //     let connHeader = connection.Headers
    //                      |> Map.change "Bearer" (fun currentBearer ->
    //                         match currentBearer with
    //                         | Some _ -> Some bearer
    //                         | None   -> None)
    //     let updatedConnection = goTrueConnection {
    //          url connection.Url
    //          headers connHeader
    //     }
    //     let body = Map<string, Object>[
    //         "email", email
    //     ]
    //     deserializeResponse |> performAuthRequest body [] "invite" options httpClient updatedConnection
        
    let signInWithEmailOtp (email: string) (options: AuthOptions option)
                     (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
        ]
        
        signIn<unit> body [] "otp" options connection deserializeEmptyResponse
    
    let signInWithPhoneOtp (phone: string) (options: AuthOptions option)
                     (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[
            "phone", phone
        ]
        
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
                   (connection: GoTrueConnection): Result<UserResponse, GoTrueError> =
        let content = new StringContent(Json.serialize attributes, Encoding.UTF8, "application/json")
        
        let result = put "user" (Some (Map<string, string>["Authorization", $"Bearer {token}"])) content connection
        deserializeResponse<UserResponse> result
        
    let refreshToken (refreshToken: string) (accessToken: string) (options: AuthOptions option)
                     (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "refresh_token", refreshToken
        ]
        
        // TODO: Replace token properly
        let updatedConnection = 
            { connection with Headers = connection.Headers |> Map.change "Authorization" (fun _ -> Some accessToken) }
            
        performAuthRequest<GoTrueSessionResponse>
            (Some body) ["grant_type=refresh_token"]
            "token" options updatedConnection deserializeResponse