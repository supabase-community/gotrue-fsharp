namespace GoTrueTest

open System.Net
open System.Net.Http
open System.Threading
open FsUnit.Xunit
open Moq
open Moq.Protected
open Xunit
open GoTrue
open GoTrue.Common
open GoTrue.Http
open GoTrue.AdminApiHelpers

[<Collection("createUser")>]
module CreateUserTests =
    [<Fact>]
    let ``createUser returns GoTrueSessionResponse if successful`` () =
        // Arrange
        let requestBody =
            """{
                "email": "email@email.com",
                "phone": null,
                "password": "secret-password",
                "data": null,
                "user_metadata": null,
                "app_metadata": null,
                "email_confirm": true,
                "phone_confirm": null,
                "ban_duration": null
            }"""
        let response =
            """{
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
            }"""
        let expectedResponse =
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
        let attributes =
            { email = Some "email@email.com"
              phone = None
              password = Some "secret-password"
              data = None
              userMetadata = None
              appMetadata = None
              emailConfirm = Some true
              phoneConfirm = None
              banDuration = None }
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.createUser attributes |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok user -> user |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``createUser returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "password":"secret-password"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let attributes =
            { email = Some "email@email.com"
              phone = None
              password = Some "secret-password"
              data = None
              userMetadata = None
              appMetadata = None
              emailConfirm = Some true
              phoneConfirm = None
              banDuration = None }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.createUser attributes |> Async.RunSynchronously
        
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
                    req.RequestUri.ToString() = "http://example.com/admin/users" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("deleteUser")>]
module DeleteUserTests =
    [<Fact>]
    let ``deleteUser returns unit if successful`` () =
        // Arrange
        
        let mockHandler = mockHttpMessageHandler ""
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.deleteUser "user-id" |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok ok -> ok |> should equal ()
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Delete &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users/user-id"),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``createUser returns GoTrueError if not successful`` () =
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
        let result = connection |> AdminApi.deleteUser "user-id" |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Delete &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users/user-id"),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("listUsers")>]
module ListUsersTests =
    [<Fact>]
    let ``listUsers returns User list if successful`` () =
        // Arrange
        let response =
            """[
                {
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
                },
                {
                    "id": "user-id-2",
                    "aud": "authenticated",
                    "email": "email2@email.com",
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
            ]"""
        let expectedResponse =
            [
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
             { id = "user-id-2"
               aud = "authenticated"
               email = Some "email2@email.com"
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
             ]
        
        let mockHandler = mockHttpMessageHandler response
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.listUsers (Some 1) (Some 100) |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok users -> users |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Get &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users?page=1&per_page=100"),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``listUsers returns GoTrueError if not successful`` () =
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
        let result = connection |> AdminApi.listUsers (Some 1) (Some 100) |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Get &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users?page=1&per_page=100"),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("inviteUserByEmail")>]
module InviteUserByEmailTests =
    [<Fact>]
    let ``inviteUserByEmail returns User if successful`` () =
        // Arrange
        let requestBody = """{"email": "email@email.com"}"""
        let response =
            """{
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
            }"""
        let expectedResponse =
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
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.inviteUserByEmail "email@email.com" None |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok user -> user |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/invite" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``inviteUserByEmail returns GoTrueError if not successful`` () =
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
        let result = connection |> AdminApi.inviteUserByEmail "email@email.com" None |> Async.RunSynchronously
        
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
                    req.RequestUri.ToString() = "http://example.com/invite" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("getUserById")>]
module GetUserByIdTests =
    [<Fact>]
    let ``getUserById returns User if successful`` () =
        // Arrange
        let response =
            """{
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
            }"""
        let expectedResponse =
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
        
        let mockHandler = mockHttpMessageHandler response
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.getUserById "user-id" |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok user -> user |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Get &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users/user-id"),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``getUserById returns GoTrueError if not successful`` () =
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
        let result = connection |> AdminApi.getUserById "user-id" |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Get &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users/user-id"),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("updateUserById")>]
module UpdateUserByIdTests =
    [<Fact>]
    let ``updateUserById returns User if successful`` () =
        // Arrange
        let requestBody =
            """{
                "email": "email@email.com",
                "phone": null,
                "password": "secret-password",
                "data": null,
                "user_metadata": null,
                "app_metadata": null,
                "email_confirm": true,
                "phone_confirm": null,
                "ban_duration": null
            }"""
        let response =
            """{
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
            }"""
        let expectedResponse =
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
        let attributes =
            { email = Some "email@email.com"
              phone = None
              password = Some "secret-password"
              data = None
              userMetadata = None
              appMetadata = None
              emailConfirm = Some true
              phoneConfirm = None
              banDuration = None }
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.updateUserById "user-id" attributes |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok user -> user |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Put &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/users/user-id" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``updateUserById returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "email":"email@email.com",
                "password":"secret-password"
            }"""
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        
        let attributes =
            { email = Some "email@email.com"
              phone = None
              password = Some "secret-password"
              data = None
              userMetadata = None
              appMetadata = None
              emailConfirm = Some true
              phoneConfirm = None
              banDuration = None }
        
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result = connection |> AdminApi.updateUserById "user-id" attributes |> Async.RunSynchronously
        
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
                    req.RequestUri.ToString() = "http://example.com/admin/users/user-id" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
[<Collection("generateLink")>]
module GenerateLinkTests =
    [<Fact>]
    let ``generateLink returns User if successful`` () =
        // Arrange
        let requestBody =
            """{
                "email": "email@email.com",
                "type": "sign_up",
                "password": "strong-password"
            }"""
        let response =
            """{
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
            }"""
        let expectedResponse =
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
        
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = goTrueConnection {
            url "http://example.com"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }
        
        // Act
        let result =
            connection
            |> AdminApi.generateLink "email@email.com" SignUp None (Some "strong-password") None
            |> Async.RunSynchronously
        
        // Assert
        match result with
        | Ok user -> user |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.ToString() = "http://example.com/admin/generate_link" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())
            
    [<Fact>]
    let ``generateLink returns GoTrueError if not successful`` () =
        // Arrange
        let requestBody =
            """{
                "email": "email@email.com",
                "type": "sign_up",
                "password": "strong-password"
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
        let result =
            connection 
            |> AdminApi.generateLink "email@email.com" SignUp None (Some "strong-password") None
            |> Async.RunSynchronously
        
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
                    req.RequestUri.ToString() = "http://example.com/admin/generate_link" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>())