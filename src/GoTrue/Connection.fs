namespace GoTrue.Connection

[<AutoOpen>]
module Connection =
    type GoTrueConnection = {
        Url: string
        Headers: Map<string, string>
    }
    
    type GoTrueConnectionBuilder() =
        member _.Yield _ =
            {   Url = ""
                Headers = Map[] }
       
        [<CustomOperation("url")>]
        member _.Url(connection, url) =
            { connection with Url = url }
        
        [<CustomOperation("headers")>]
        member _.Headers(connection, headers) =
            { connection with Headers = headers }
            
    let goTrueConnection = GoTrueConnectionBuilder()