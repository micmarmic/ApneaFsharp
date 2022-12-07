let NAK : byte = 21uy
let DC4 : byte = 20uy
let byteArray : byte array = [| 48uy; 49uy; NAK; DC4 |]
let isNAK x =  x = NAK
let result = byteArray |> Array.findIndex (fun b -> isNAK b)
printfn "index %d array %s" result (byteArray.ToString())
let subArray = byteArray.[.. result - 1]
printfn "String result: %s" (System.Text.Encoding.Default.GetString(byteArray.[0 .. (result - 1)]).Trim())