namespace GoTrueTest

open System.Net
open System.Net.Http
open System.Threading
open FsUnit.Xunit
open Moq
open Moq.Protected
open Xunit
open GoTrue
open GoTrue.AuthRequestCommon

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
        | Ok session -> session |> should equal expectedResponse
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
    let ``signUpWithEmail returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "password":"secret-password"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
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
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
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
        | Ok session -> session |> should equal expectedResponse
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
    let ``signUpWithPhone returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "phone":"09123456789",
                "password":"secret-password"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
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
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
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
        | Ok session -> session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/token?grant_type=password" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
    
    [<Fact>]
    let ``signInWithEmail returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "password":"secret-password"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
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
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/token?grant_type=password" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())     
    
    [<Fact>]
    let ``signInWithPhone returns GoTrueSessionResponse if successful`` () =
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
        let result = Client.signInWithPhone "09123456789" "secret-password" None connection 
        
        // Assert
        match result with
        | Ok session -> session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/token?grant_type=password" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``signInWithPhone returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "phone":"09123456789",
                "password":"secret-password"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signInWithPhone "09123456789" "secret-password" None connection 
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/token?grant_type=password" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
            
[<Collection("getOAuthUrl")>]
module GetOAuthUrlTests =
    [<Fact>]
    let ``getOAuthUrl returns url with provider as url param if options is None`` () =
        // Arrange
        let provider = Google
        let options = None
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
        }
        
        // Act
        let result = Client.getOAuthUrl provider options connection
        
        // Assert
        result |> should equal $"{connection.Url}/authorize?provider=google"
        
    [<Fact>]
    let ``getOAuthUrl returns url with provider and redirectTo as url params when option has redirectTo set`` () =
        // Arrange
        let provider = Google
        let options = { redirectTo = Some "https://example.com" ; scopes = None }
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
        }
        
        // Act
        let result = Client.getOAuthUrl provider (Some options) connection
        
        // Assert
        result |> should equal $"{connection.Url}/authorize?provider=google&redirect_to=https://example.com"
        
    [<Fact>]
    let ``getOAuthUrl returns url with provider and scopes as url params when option has scopes set`` () =
        // Arrange
        let provider = Google
        let options = { redirectTo = None ; scopes = Some "scope" }
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
        }
        
        // Act
        let result = Client.getOAuthUrl provider (Some options) connection
        
        // Assert
        result |> should equal $"{connection.Url}/authorize?provider=google&scopes=scope"
        
    [<Fact>]
    let ``getOAuthUrl returns url with provider, redirectTo and scopes as url params when option has all fields set`` () =
        // Arrange
        let provider = Google
        let options = { redirectTo = Some "https://example.com" ; scopes = Some "scope" }
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
        }
        
        // Act
        let result = Client.getOAuthUrl provider (Some options) connection
        
        // Assert
        result |> should equal $"{connection.Url}/authorize?provider=google&redirect_to=https://example.com&scopes=scope"
        
[<Collection("signInWithMagicLink")>]
module SignInWithMagicLinkTests =
    [<Fact>]
    let ``signInWithMagicLink returns GoTrueSessionResponse if successful`` () =
        // Arrange
        let requestBody = """{"email":"email@email.com"}"""
        
        let mockHandler = mockHttpMessageHandlerWithBody "" requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signInWithMagicLink "email@email.com" None connection 
        
        // Assert
        match result with
        | Ok session -> session |> should equal ()
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/magiclink" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("signInWithOtp")>]
module SignInWithOtpTests =
    [<Fact>]
    let ``signInWithEmailOtp returns unit if successful`` () =
        // Arrange
        let requestBody = """{"email":"email@email.com"}"""
        
        let mockHandler = mockHttpMessageHandlerWithBody "" requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signInWithEmailOtp "email@email.com" None connection 
        
        // Assert
        match result with
        | Ok session -> session |> should equal ()
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/otp" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``signInWithEmailOtp returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody = """{"email":"email@email.com"}"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signInWithEmailOtp "email@email.com" None connection 
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/otp" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
    
    [<Fact>]
    let ``signInWithPhoneOtp returns unit if successful`` () =
        // Arrange
        let requestBody = """{"phone":"09123456789"}"""
        
        let mockHandler = mockHttpMessageHandlerWithBody "" requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signInWithPhoneOtp "09123456789" None connection 
        
        // Assert
        match result with
        | Ok session -> session |> should equal ()
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/otp" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``signInWithPhoneOtp returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody = """{"phone":"09123456789"}"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signInWithPhoneOtp "09123456789" None connection 
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/otp" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("verifyOtp")>]
module VerifyOtpTests =
    [<Fact>]
    let ``verifyEmailOtp returns GoTrueSessionResponse if successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "token":"otp-token"
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
        let result = Client.verifyEmailOtp "email@email.com" "otp-token" None connection 
        
        // Assert
        match result with
        | Ok session -> session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/verify" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
    
    [<Fact>]
    let ``verifyEmailOtp returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "token":"otp-token"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.verifyEmailOtp "email@email.com" "otp-token" None connection 
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/verify" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``verifyPhoneOtp returns GoTrueSessionResponse if successful`` () =
        // Arrange
        let requestBody =
            """{
                "phone":"09123456789",
                "token":"otp-token"
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
        let result = Client.verifyPhoneOtp "09123456789" "otp-token" None connection 
        
        // Assert
        match result with
        | Ok session -> session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/verify" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
    
    [<Fact>]
    let ``verifyPhoneOtp returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "phone":"09123456789",
                "token":"otp-token"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.verifyPhoneOtp "09123456789" "otp-token" None connection 
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/verify" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("updateUser")>]
module UpdaterUserTests =
    [<Fact>]
    let ``updateUser returns User if successful`` () =
        // Arrange
        let attributes =
            { email = Some "email@email.com"
              phone = Some "09123456789"
              password = None
              data = None }
        let requestBody =
            """{
                "email": "email@email.com",
                "phone": 09123456789,
                "password": null,
                "data": null
            }"""
        let response =
            """{
                "id": "user-id",
                "aud": "authenticated",
                "email": "email@email.com",
                "phone": "09123456789",
                "email_confirmed_at": "2023-01-01T12:00:00Z",
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
            }"""
        let expectedResponse =
            { id = "user-id"
              aud = "authenticated"
              email = Some "email@email.com"
              phone = Some "09123456789"
              emailConfirmedAt = Some "2023-01-01T12:00:00Z"
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
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.updateUser attributes "token" connection
        
        // Assert
        match result with
        | Ok session -> session |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Put &&
                    req.Headers.Contains("apiKey") &&
                    req.Headers.Contains("Authorization") &&
                    req.RequestUri.ToString() = "http://example.com/user" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``updateUser returns GoTrueError if not successful`` () =
        // Arrange
        let attributes =
            { email = Some "email@email.com"
              phone = Some "09123456789"
              password = None
              data = None }
        let requestBody =
            """{
                "email": "email@email.com",
                "phone": 09123456789,
                "password": null,
                "data": null
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.updateUser attributes "token" connection
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Put &&
                    req.Headers.Contains("apiKey") &&
                    req.Headers.Contains("Authorization") &&
                    req.RequestUri.ToString() = "http://example.com/user" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())

[<Collection("signOut")>]
module SingOutTests =
    [<Fact>]
    let ``signOut returns unit if successful`` () =
        // Arrange
        let mockHandler = mockHttpMessageHandler ""
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signOut connection 
        
        // Assert
        match result with
        | Ok session -> session |> should equal ()
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/logout"),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``signOut returns GoTrueError if not successful`` () =
        // Arrange
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let mockHandler = mockHttpMessageHandlerFail expectedError
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = Client.signOut connection 
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/logout"),
                ItExpr.IsAny<CancellationToken>())