namespace GoTrueTest

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open Moq
open Moq.Protected
open GoTrue.Http

[<AutoOpen>]
module Helper =
    let mockHttpMessageHandler (response: string) =
        let mockHandler = Mock<HttpMessageHandler>(MockBehavior.Strict)
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8, "application/json")))
            .Verifiable()
        mockHandler
    
    let mockHttpMessageHandlerWithBody (response: string) (requestBody: string) =
        let mockHandler = Mock<HttpMessageHandler>(MockBehavior.Strict)
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback(fun (req: HttpRequestMessage) (_: CancellationToken) -> 
                req.Content <- new StringContent(requestBody, Encoding.UTF8, "application/json")
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8, "application/json")))
            .Verifiable()
        mockHandler
        
    let mockHttpMessageHandlerFail (error: GoTrueError) =
        let mockHandler = Mock<HttpMessageHandler>(MockBehavior.Strict)
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage(error.statusCode.Value, Content = new StringContent(error.message, Encoding.UTF8, "application/json")))
            .Verifiable()
        mockHandler
        
    let mockHttpMessageHandlerWithBodyFail (error: GoTrueError) (requestBody: string) =
        let mockHandler = Mock<HttpMessageHandler>(MockBehavior.Strict)
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback(fun (req: HttpRequestMessage) (_: CancellationToken) -> 
                req.Content <- new StringContent(requestBody, Encoding.UTF8, "application/json")
            )
            .ReturnsAsync(
                new HttpResponseMessage(error.statusCode.Value, Content = new StringContent(error.message, Encoding.UTF8, "application/json")))
            .Verifiable()
        mockHandler

