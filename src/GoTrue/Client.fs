namespace GoTrue

open System
open System.Net.Http
open System.Web
open FSharp.Json
open GoTrue.AuthRequestCommon
open GoTrue.Connection
open GoTrue.Http

[<AutoOpen>]
module ClientHelpers = 
    let inline signUp<'T> (body: Map<string, obj>) (urlParams: string list) (options: AuthOptions option) (connection: GoTrueConnection)
                          (deserializeWith: Result<HttpResponseMessage, GoTrueError> -> Result<'T, GoTrueError>): Result<'T, GoTrueError> =
        performAuthRequest<'T> (Some body) urlParams "signup" options connection deserializeWith
        
    let inline signIn<'T> (body: Map<string, obj>) (urlParams: string list) (pathSuffix: string)
                          (options: AuthOptions option) (connection: GoTrueConnection)
                          (deserializeWith: Result<HttpResponseMessage, GoTrueError> -> Result<'T, GoTrueError>): Result<'T, GoTrueError> =
        performAuthRequest<'T> (Some body) urlParams pathSuffix options connection deserializeWith

[<AutoOpen>]
module Client =
    type User = {
      id: string
      aud: string
      email: string option
      phone: string option
      // [<JsonField"app_metadata">]
      // appMetadata: Map<string, obj>
      // [<JsonField"user_metadata">]
      // UserMetadata: Map<string, obj>
      [<JsonField("email_confirmed_at")>]
      emailConfirmedAt: string option
      [<JsonField("phone_confirmed_at")>]
      phoneConfirmedAt: string option
      [<JsonField("last_sign_in_at")>]
      lastSignInAt: string option
      role: string
      [<JsonField("created_at")>]
      createdAt: string
      [<JsonField("updated_at")>]
      updatedAt: string
      [<JsonField("confirmation_sent_at")>]
      confirmationSentAt: string option
      [<JsonField("recovery_sent_at")>]
      recoverySentAt: string option
      [<JsonField("email_change_sent_at")>]
      emailChangeSentAt: string option
      [<JsonField("new_email")>]
      newEmail: string option
      [<JsonField("invited_at")>]
      invitedAt: string option
      [<JsonField("action_link")>]
      actionLink: string option
    }
    
    type GoTrueSessionResponse = {
        [<JsonField("access_token")>]
        accessToken: string
        [<JsonField("expires_in")>]
        expiresIn: int
        [<JsonField("refresh_token")>]
        refreshToken: string
        [<JsonField("token_type")>]
        tokenType: string option
        [<JsonField("provider_token")>]
        providerToken: string option
        provider: string option
        url: string option
        user: User option
    }
        
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
     
    let private getUrlForProvider (provider: string) (options: AuthOptions option) (connection: GoTrueConnection): string =
        let urlParams = [$"provider={provider}"]
                         // ; "redirect_to=io.supabase.flutterquickstart://login-callback/"]
        // let scopes =
        //     maybe {
        //         let! ao = options
        //         let! s = ao.scopes
        //         return s
        //     }
        // match scopes with
        // Some s ->
        let url = connection.Url
        let joinedParams = urlParams |> String.concat "&"
        $"{url}/authorize?{joinedParams}"
        
    let signInWithProvider (provider: string) (options: AuthOptions option) (connection: GoTrueConnection) =
        let providerUrl = connection |> getUrlForProvider provider options
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
        
    let sendEmailOTP (email: string) (options: AuthOptions option)
                     (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
        ]
        
        signIn<unit> body [] "otp" options connection deserializeEmptyResponse
    
    let sendPhoneOTP (phone: string) (options: AuthOptions option)
                     (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let body = Map<string, obj>[
            "phone", phone
        ]
        
        signIn<unit> body [] "otp" options connection deserializeEmptyResponse
        
    let verifyEmailOTP (email: string) (token: string) (options: AuthOptions option)
                       (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
            "token", token
        ]
        
        signIn<GoTrueSessionResponse> body [] "verify" options connection deserializeResponse
        
    let verifyPhoneOTP (phone: string) (token: string) (options: AuthOptions option)
                       (connection: GoTrueConnection): Result<GoTrueSessionResponse, GoTrueError> =
        let body = Map<string, obj>[
            "phone", phone
            "token", token
        ]
        
        signIn<GoTrueSessionResponse> body [] "verify" options connection deserializeResponse
        
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