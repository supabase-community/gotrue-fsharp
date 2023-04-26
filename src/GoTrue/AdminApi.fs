namespace GoTrue

open System
open System.Net.Http
open System.Text
open FSharp.Json
open GoTrue.AuthRequestCommon
open GoTrue.Common
open GoTrue.Connection
open GoTrue.Http

/// Contains helper functions and types for `AdminApi.fs` module.
[<AutoOpen>]
module AdminApiHelpers =
    /// Represents user created by admin
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
    
    /// Represents type of link to be generated
    type GenerateLinkType =
        | SignUp
        | Invite
        | MagicLink
        | Recovery
        | EmailChangeCurrent
        | EmailChangeNew
        
    /// Adds key-value pair to map if value is given
    let internal addItemToMapIfPresent<'a> (key: string) (value: 'a option) (map: Map<string, obj>): Map<string, obj> =
        match value with
        | Some v -> map |> Map.add key v
        | _      -> map
        
    /// Converts `GenerateLinkType` to snake_case string form
    let linkTypeToSnakeCase (linkType: GenerateLinkType): string =
        let rec loop lt acc =
            match lt with
            | "" -> acc
            | _ ->
                let c, rest = lt[0], lt[1..]
                if Char.IsUpper c then
                    let acc' = if acc = "" || acc.EndsWith("_") then acc + (Char.ToLowerInvariant c).ToString() else acc + "_" + (Char.ToLowerInvariant c).ToString()
                    loop rest acc'
                else
                    loop rest (acc + string c)

        loop (linkType.ToString()) ""
        
/// Contains functions for admin. Needs service_role key.
/// Never expose your service_role key for public. Do not use on client side!
[<AutoOpen>]
module AdminApi =
    /// Creates user with given attributes
    let createUser (attributes: AdminUserAttributes) (connection: GoTrueConnection): Async<Result<User, GoTrueError>> =
        let body = Json.serialize attributes
        let content = getStringContent body
        
        async {
            let! response = post "admin/users" None content connection
            return deserializeResponse<User> response
        }
        
    /// Deletes user by id
    let deleteUser (id: string) (connection: GoTrueConnection): Async<Result<unit, GoTrueError>> =
        let urlSuffix = $"admin/users/{id}"
        
        async {
            let! response = delete urlSuffix None None connection
            return deserializeEmptyResponse response
        }
        
    /// Lists users with respect to page and perPage params
    let listUsers (page: int option) (perPage: int option)
                  (connection: GoTrueConnection): Async<Result<User list, GoTrueError>> =
        let pathSuffix = "admin/users"
        
        let pageUrlParam = Option.bind (fun x -> Some $"page={x}") page
        let perPageUrlParam = Option.bind (fun x -> Some $"per_page={x}") perPage
        
        let urlParams =
            [pageUrlParam ; perPageUrlParam]
            |> List.filter (fun item -> item.IsSome)
            |> List.map (fun item -> item.Value)
            
        let queryString = getUrlParamsString urlParams
        let urlSuffix = $"{pathSuffix}{queryString}"    
    
        async {
            let! response = get urlSuffix None connection
            return deserializeResponse<User list> response
        }
        
    /// Sends invitation email to user with given email address
    let inviteUserByEmail (email: string) (options: AuthOptions option)
                          (connection: GoTrueConnection): Async<Result<User, GoTrueError>> =
        let body = Map<string, obj>[ "email", email ]
        
        performAuthRequest (Some body) None [] "invite" options connection deserializeResponse<User>
        
    /// Gets user detail by given uid
    let getUserById (uid: string) (connection: GoTrueConnection): Async<Result<User, GoTrueError>> =
        let urlSuffix = $"admin/users/{uid}"
        
        async {
            let! response = get urlSuffix None connection
            return deserializeResponse<User> response
        }
        
    /// Updates user with given uid with given attributes
    let updateUserById (uid: string) (attributes: AdminUserAttributes)
                       (connection: GoTrueConnection): Async<Result<User, GoTrueError>> =
        let urlSuffix = $"admin/users/{uid}"
        
        let body = Json.serialize attributes
        let content = getStringContent body
        
        async {
            let! response = put urlSuffix None content connection
            return deserializeResponse<User> response
        }
    
    /// Generates link
    let generateLink (email: string) (linkType: GenerateLinkType) (options: AuthOptions option) (password: string option)
                     (data: Map<string, obj> option) (connection: GoTrueConnection): Async<Result<User, GoTrueError>> =
        let body =
            Map<string, obj>[
                "email", email
                "type", linkTypeToSnakeCase linkType
            ]
            |> addItemToMapIfPresent "data" data
            |> addItemToMapIfPresent "password" password
              
        performAuthRequest (Some body) None [] "admin/generate_link" options connection deserializeResponse<User>