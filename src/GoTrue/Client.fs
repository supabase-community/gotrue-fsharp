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
    }
    
    type AuthOptions = {
        redirectTo: string option
        scopes: string option
    }
    
    type MaybeBuilder() =
        member this.Bind(m, f) = Option.bind f m
        member this.Return(x) = Some x

    let maybe = MaybeBuilder()
    
    let private performAuthRequest<'T> (body: Map<string, Object>) (urlParams: string list) (pathSuffix: string)
                            (authOptions: AuthOptions option) (httpClient: HttpClient) (connection: GoTrueConnection)
                            (deserializeWith: Result<HttpResponseMessage, AuthError> -> Result<'T, AuthError>): Result<'T, AuthError> =
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
        let response = connection |> post $"{connection.Url}/{pathSuffix}{queryString}" content httpClient
        response |> deserializeWith
    
    let private signUp<'T> (body: Map<string, Object>) (urlParams: string list) (authOptions: AuthOptions option)
                       (httpClient: HttpClient) (connection: GoTrueConnection)
                       (deserializeWith: Result<HttpResponseMessage, AuthError> -> Result<'T, AuthError>): Result<'T, AuthError> =
        deserializeWith |> performAuthRequest body urlParams "signup" authOptions httpClient connection
        
    let private signIn<'T> (body: Map<string, Object>) (urlParams: string list) (pathSuffix: string)
                           (authOptions: AuthOptions option) (httpClient: HttpClient) (connection: GoTrueConnection)
                           (deserializeWith: Result<HttpResponseMessage, AuthError> -> Result<'T, AuthError>): Result<'T, AuthError> =
        deserializeWith |> performAuthRequest body urlParams pathSuffix authOptions httpClient connection
        
    let signUpWithEmail (email: string) (password: string) (authOptions: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "email", email
            "password", password
        ]
        deserializeResponse |> signUp body [] authOptions httpClient connection
        
    let signUpWithPhone (phone: string) (password: string) (authOptions: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "phone", phone
            "password", password
        ]
        deserializeResponse |> signUp body [] authOptions httpClient connection
        
    let signInWithEmail (email: string) (password: string) (authOptions: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "email", email
            "password", password
        ]
        deserializeResponse |> signIn body ["grant_type=password"] "token" authOptions httpClient connection
        
    let signInWithPhone (phone: string) (password: string) (authOptions: AuthOptions option)
                        (connection: GoTrueConnection): Result<GoTrueSessionResponse, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "phone", phone
            "password", password
        ]
        deserializeResponse |> signIn body ["grant_type=password"] "token" authOptions httpClient connection
        
    let signInWithMagicLink (email: string) (authOptions: AuthOptions option)
                            (connection: GoTrueConnection): Result<unit, AuthError> =
        let httpClient = new HttpClient()
        let body = Map<string, Object>[
            "email", email
        ]
        deserializeEmptyResponse |> signIn body [] "magiclink" authOptions httpClient connection