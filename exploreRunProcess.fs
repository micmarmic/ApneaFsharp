open System.Threading.Tasks
open System.Diagnostics
let runExeProcess (pathToExe : string) (pathFileToConvert : string) (pathWorkDir : string)=
    
    let startInfo = ProcessStartInfo()
    startInfo.FileName <- pathToExe
    startInfo.Arguments <-  pathFileToConvert
    startInfo.WorkingDirectory <-  pathWorkDir 
    startInfo.CreateNoWindow <-  true

    printfn "start info %s" startInfo.Arguments
    
    Process.Start(startInfo).WaitForExit()
  

runExeProcess @"d:\Documents\programming\c#\CPAPGraphsCSharp\EDFFileReader\edf2ascii\edf2ascii.exe"  @"D:\TEMP2\20220712\20220628_235657_EVE.edf" @"D:\TEMP2\20220712"
