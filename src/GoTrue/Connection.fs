namespace GoTrue

open System.Net.Http

/// Contains CE for creating connection
[<AutoOpen>]
module Connection =
    /// Represents current client version
    let version = "0.0.1"

    /// Represents client info header with current version
    let clientInfo = ("X-Client-Info", $"gotrue-client-v{version}")
    
    /// Represents base connection
    type GoTrueConnection = {
        Url: string
        Headers: Map<string, string>
        HttpClient: HttpClient
    }
    
    type GoTrueConnectionBuilder() =
        member _.Yield _ =
            {   Url = ""
                Headers = Map []
                HttpClient = new HttpClient() }
       
        [<CustomOperation("url")>]
        member _.Url(connection, url) =
            { connection with Url = url }
        
        [<CustomOperation("headers")>]
        member _.Headers(connection, headers) =
            let k, v = clientInfo
            { connection with Headers = headers |> Map.add k v }
            
        [<CustomOperation("httpClient")>]
        member _.HttpClient(connection, httpClient) =
            { connection with HttpClient = httpClient }
            
    let goTrueConnection = GoTrueConnectionBuilder()