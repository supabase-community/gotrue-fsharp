namespace GoTrue.Common

open System.Net.Http.Headers
open FSharp.Json


[<AutoOpen>]
module Common =
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
    
    type UserResponse = {
        user: User option
    }
    
    let internal addRequestHeaders (headers: Map<string, string>) (httpRequestHeaders: HttpRequestHeaders): unit =
        headers |> Seq.iter (fun (KeyValue(k, v)) -> httpRequestHeaders.Add(k, v))