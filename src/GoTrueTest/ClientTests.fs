namespace GoTrueTest

open System.Net.Http
open System.Threading
open FsUnit.Xunit
open Moq
open Moq.Protected
open Xunit
open GoTrue

[<Collection("signUp")>]
module SignUpTests =
    [<Fact>]
    let ``signUpWithEmail returns GoTrueSessionResponse if successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "password":"secret-password"
            }"""
        let response =
            """{
                "access_token": "access-token",
                "expires_in": 3600,
                "refresh_token": "refresh-token",
                "token_type": "bearer",
                "provider_token": null,
                "provider_refresh_token": null,
                "user": {
                        "id": "user-id",
                        "aud": "authenticated",
                        "email": "email@email.com",
                        "phone": null,
                        "email_confirmed_at": "2023-01-01T12:00:00Z",
                        "phone_confirmed_at": null,
                        "last_sign_in_at": "2023-01-01T12:00:00Z",
                        "role": "authenticated",
                        "created_at": "2023-01-01T12:00:00Z",
                        "updated_at": "2023-01-01T12:00:00Z",
                        "confirmation_sent_at": null,
                        "recovery_sent_at": "2023-01-01T12:00:00Z",
                        "email_change_sent_at": null,
                        "new_email": null,
                        "invited_at": null,
                        "action_link": null
                }
            }"""
        let expectedResponse =
            { accessToken = "access-token"
              expiresIn = 3600
              refreshToken = "refresh-token"
              tokenType = Some "bearer"
              providerToken = None
              providerRefreshToken = None
              user =
                Some
                    { id = "user-id"
                      aud = "authenticated"
                      email = Some "email@email.com"
                      phone = None
                      emailConfirmedAt = Some "2023-01-01T12:00:00Z"
                      phoneConfirmedAt = None
                      lastSignInAt = Some "2023-01-01T12:00:00Z"
                      role =  "authenticated"
                      createdAt = "2023-01-01T12:00:00Z"
                      updatedAt ="2023-01-01T12:00:00Z"
                      confirmationSentAt = None
                      recoverySentAt = Some "2023-01-01T12:00:00Z"
                      emailChangeSentAt = None
                      newEmail = None
                      invitedAt = None
                      actionLink = None }
            }
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signUpWithEmail "email@email.com" "secret-password" None connection 
        
        // Assert
        match result with
        | Ok session ->
            session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/signup" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``signUpWithPhone returns GoTrueSessionResponse if successful`` () =
        // Arrange
        let requestBody =
            """{
                "phone":"09123456789",
                "password":"secret-password"
            }"""
        let response =
            """{
                "access_token": "access-token",
                "expires_in": 3600,
                "refresh_token": "refresh-token",
                "token_type": "bearer",
                "provider_token": null,
                "provider_refresh_token": null,
                "user": {
                        "id": "user-id",
                        "aud": "authenticated",
                        "email": null,
                        "phone": "09123456789",
                        "email_confirmed_at": null,
                        "phone_confirmed_at": "2023-01-01T12:00:00Z",
                        "last_sign_in_at": "2023-01-01T12:00:00Z",
                        "role": "authenticated",
                        "created_at": "2023-01-01T12:00:00Z",
                        "updated_at": "2023-01-01T12:00:00Z",
                        "confirmation_sent_at": null,
                        "recovery_sent_at": "2023-01-01T12:00:00Z",
                        "email_change_sent_at": null,
                        "new_email": null,
                        "invited_at": null,
                        "action_link": null
                }
            }"""
        let expectedResponse =
            { accessToken = "access-token"
              expiresIn = 3600
              refreshToken = "refresh-token"
              tokenType = Some "bearer"
              providerToken = None
              providerRefreshToken = None
              user =
                Some
                    { id = "user-id"
                      aud = "authenticated"
                      email = None
                      phone = Some "09123456789"
                      emailConfirmedAt = None
                      phoneConfirmedAt = Some "2023-01-01T12:00:00Z"
                      lastSignInAt = Some "2023-01-01T12:00:00Z"
                      role =  "authenticated"
                      createdAt = "2023-01-01T12:00:00Z"
                      updatedAt ="2023-01-01T12:00:00Z"
                      confirmationSentAt = None
                      recoverySentAt = Some "2023-01-01T12:00:00Z"
                      emailChangeSentAt = None
                      newEmail = None
                      invitedAt = None
                      actionLink = None }
            }
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signUpWithPhone "09123456789" "secret-password" None connection 
        
        // Assert
        match result with
        | Ok session ->
            session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/signup" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("signIn")>]
module SignInTests =
    [<Fact>]
    let ``signInWithEmail returns GoTrueSessionResponse if successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "password":"secret-password"
            }"""
        let response =
            """{
                "access_token": "access-token",
                "expires_in": 3600,
                "refresh_token": "refresh-token",
                "token_type": "bearer",
                "provider_token": null,
                "provider_refresh_token": null,
                "user": {
                        "id": "user-id",
                        "aud": "authenticated",
                        "email": "email@email.com",
                        "phone": null,
                        "email_confirmed_at": "2023-01-01T12:00:00Z",
                        "phone_confirmed_at": null,
                        "last_sign_in_at": "2023-01-01T12:00:00Z",
                        "role": "authenticated",
                        "created_at": "2023-01-01T12:00:00Z",
                        "updated_at": "2023-01-01T12:00:00Z",
                        "confirmation_sent_at": null,
                        "recovery_sent_at": "2023-01-01T12:00:00Z",
                        "email_change_sent_at": null,
                        "new_email": null,
                        "invited_at": null,
                        "action_link": null
                }
            }"""
        let expectedResponse =
            { accessToken = "access-token"
              expiresIn = 3600
              refreshToken = "refresh-token"
              tokenType = Some "bearer"
              providerToken = None
              providerRefreshToken = None
              user =
                Some
                    { id = "user-id"
                      aud = "authenticated"
                      email = Some "email@email.com"
                      phone = None
                      emailConfirmedAt = Some "2023-01-01T12:00:00Z"
                      phoneConfirmedAt = None
                      lastSignInAt = Some "2023-01-01T12:00:00Z"
                      role =  "authenticated"
                      createdAt = "2023-01-01T12:00:00Z"
                      updatedAt ="2023-01-01T12:00:00Z"
                      confirmationSentAt = None
                      recoverySentAt = Some "2023-01-01T12:00:00Z"
                      emailChangeSentAt = None
                      newEmail = None
                      invitedAt = None
                      actionLink = None }
            }
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signInWithEmail "email@email.com" "secret-password" None connection 
        
        // Assert
        match result with
        | Ok session ->
            session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/signup" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
    