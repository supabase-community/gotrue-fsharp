namespace GoTrue

open System.Net.Http
open System.Text
open FSharp.Json
open GoTrue.AuthRequestCommon
open GoTrue.Common
open GoTrue.Connection
open GoTrue.Http

[<AutoOpen>]
module AdminApiHelpers =
    type AdminUserAttributes = {
        email:        string option
        phone:        string option
        password:     string option
        data:         Map<string, obj> option
        [<JsonField("user_metadata")>]
        userMetadata: Map<string, obj> option
        [<JsonField("app_metadata")>]
        appMetadata:  Map<string, obj> option
        [<JsonField("email_confirm")>]
        emailConfirm: bool option
        [<JsonField("phone_confirm")>]
        phoneConfirm: bool option
        [<JsonField("ban_duration")>]
        banDuration:  string option
    }
    
    type GenerateLinkType =
        | SignUp
        | Invite
        | MagicLink
        | Recovery
        | EmailChangeCurrent
        | EmailChangeNew
        
    let internal addItemToMapIfPresent<'a> (key: string) (value: 'a option) (map: Map<string, obj>): Map<string, obj> =
        match value with
        | Some v -> map |> Map.add key v
        | _      -> map

[<AutoOpen>]
module AdminApi =
    let signOut (connection: GoTrueConnection): Result<unit, GoTrueError> =
        performAuthRequest None [] "logout" None connection deserializeEmptyResponse
    
    let createUser (attributes: AdminUserAttributes) (connection: GoTrueConnection): Result<UserResponse, GoTrueError> =
        let body = Json.serialize attributes
        let content = new StringContent(body, Encoding.UTF8, "application/json") 
        
        let response = post "admin/users" None content connection
        deserializeResponse<UserResponse> response
        
    let deleteUser (id: string) (connection: GoTrueConnection): Result<unit, GoTrueError> =
        let urlSuffix = $"admin/users/{id}"
        
        let response = delete urlSuffix None None connection
        deserializeEmptyResponse response
        
    let listUsers (page: int option) (perPage: int option) (connection: GoTrueConnection): Result<User list, GoTrueError> =
        let pathSuffix = "admin/users"
        
        let pageUrlParam = Option.bind (fun x -> Some $"page={x}") page
        let perPageUrlParam = Option.bind (fun x -> Some $"per_page={x}") perPage
        
        let urlParams =
            [pageUrlParam ; perPageUrlParam]
            |> List.filter (fun item -> item.IsSome)
            |> List.map (fun item -> item.Value)
            
        let queryString = getUrlParamsString urlParams
        let urlSuffix = $"{pathSuffix}{queryString}"    
    
        let response = delete urlSuffix None None connection
        deserializeResponse<User list> response
        
    let inviteUserByEmail (email: string) (options: AuthOptions option)
                          (connection: GoTrueConnection): Result<UserResponse, GoTrueError> =
        let body = Map<string, obj>[
            "email", email
        ]
        performAuthRequest (Some body) [] "invite" options connection deserializeResponse<UserResponse>
        
    let getUserById (uid: string) (connection: GoTrueConnection): Result<UserResponse, GoTrueError> =
        let urlSuffix = $"admin/users/{uid}"
        
        let response = get urlSuffix None connection
        deserializeResponse<UserResponse> response
        
    let updateUserById (uid: string) (attributes: AdminUserAttributes)
                       (connection: GoTrueConnection): Result<UserResponse, GoTrueError> =
        let urlSuffix = $"admin/users/{uid}"
        
        let body = Json.serialize attributes
        let content = new StringContent(body, Encoding.UTF8, "application/json")
        
        let response = put urlSuffix None content connection
        deserializeResponse<UserResponse> response
    
    let generateLink (email: string) (linkType: GenerateLinkType) (options: AuthOptions option) (password: string option)
                     (data: Map<string, obj> option) (connection: GoTrueConnection): Result<UserResponse, GoTrueError> =
        //TODO: linkType to snake_case 
        let body =
            Map<string, obj>[
                "email", email
                "type", linkType
            ]
            |> addItemToMapIfPresent "data" data
            |> addItemToMapIfPresent "password" password
              
        performAuthRequest (Some body) [] "admin/generate_lin" options connection deserializeResponse<UserResponse>