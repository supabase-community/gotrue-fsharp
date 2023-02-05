namespace GoTrue

open System
open System.Net.Http
open System.Text
open FSharp.Json
open GoTrue.Connection
open GoTrue.Http

[<AutoOpen>]
module Client =
    type User = {
      id: string
      // [<JsonField"app_metadata">]
      // appMetadata: obj
      // [<JsonField"user_metadata">]
      // UserMetadata: Map<string, Object>
      aud: string
      email: string option
      phone: string option
      [<JsonField("created_at")>]
      createdAt: string
      [<JsonField("email_confirmed_at")>]
      emailConfirmedAt: string option
      [<JsonField("phone_confirmed_at")>]
      phoneConfirmedAt: string option
      [<JsonField("last_sign_in_at")>]
      lastSignInAt: string option
      role: string
      [<JsonField("updated_at")>]
      updatedAt: string
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
        // [<JsonField("status_code")>]
        // statusCode: int option
        // error: string option 
    }
    
    type AuthOptions = {
        redirectTo: string option
        scopes: string option
    }
    
    type MaybeBuilder() =
        member this.Bind(m, f) = Option.bind f m
        member this.Return(x) = Some x

    let maybe = new MaybeBuilder()
    
    let private signUp (body: Map<string, Object>) (urlParams: string list)
                       (authOptions: AuthOptions option) (httpClient: HttpClient)
                       (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let content = new StringContent(Json.serialize(body), Encoding.UTF8, "application/json")
        let redirectTo =
            maybe {
                let! ao = authOptions
                let! s = ao.redirectTo
                return s
            }
        let updatedUrlParams =
            match redirectTo with
            | Some redirectUrl -> redirectUrl :: urlParams
            | None   -> urlParams
        let queryString = if updatedUrlParams.IsEmpty then "" else "?" + (updatedUrlParams |> String.concat "&")
        let response = connection |> post $"{connection.Url}/signup{queryString}" content httpClient
        response |> deserializeResponse<GoTrueSessionResponse>
        
    let private signIn (body: Map<string, Object>) (urlParams: string list)
                       (authOptions: AuthOptions option) (httpClient: HttpClient)
                       (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let content = new StringContent(Json.serialize(body), Encoding.UTF8, "application/json")
        let redirectTo =
            maybe {
                let! ao = authOptions
                let! s = ao.redirectTo
                return s
            }
        let updatedUrlParams =
            match redirectTo with
            | Some redirectUrl -> redirectUrl :: urlParams
            | None   -> urlParams
        let queryString = if updatedUrlParams.IsEmpty then "" else "?" + (updatedUrlParams |> String.concat "&")
        let response = connection |> post $"{connection.Url}/token{queryString}" content httpClient
        response |> deserializeResponse<GoTrueSessionResponse>
        
    let signUpWithEmail (email: string) (password: string)
                        (authOptions: AuthOptions option) (connection: GoTrueConnection) =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "email", email
            "password", password
        ]
        connection |> signUp body [] authOptions httpClient
        
    let signUpWithPhone (phone: string) (password: string) (authOptions: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "phone", phone
            "password", password
        ]
        connection |> signUp body [] authOptions httpClient
        
    let signInWithEmail (email: string) (password: string) (authOptions: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "email", email
            "password", password
        ]
        connection |> signIn body ["grant_type=password"] authOptions httpClient
        
    let signInWithPhone (phone: string) (password: string) (authOptions: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "phone", phone
            "password", password
        ]
        connection |> signIn body ["grant_type=password"] authOptions httpClient
        
    let signInWithMagicLink (email: string) (authOptions: AuthOptions option)
                            (connection: GoTrueConnection): Result<unit, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "email", email
        ]
        let urlParams = []
        let content = new StringContent(Json.serialize(body), Encoding.UTF8, "application/json")
        let redirectTo =
            maybe {
                let! ao = authOptions
                let! s = ao.redirectTo
                return s
            }
        let updatedUrlParams =
            match redirectTo with
            | Some redirectUrl -> redirectUrl :: urlParams
            | None   -> urlParams
        let queryString = if updatedUrlParams.IsEmpty then "" else "?" + (updatedUrlParams |> String.concat "&")
        let response = connection |> post $"{connection.Url}/magiclink{queryString}" content httpClient
        response |> deserializeEmptyResponse