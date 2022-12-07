open apnea.ApneaModel
open apnea.ApneaFileReader
open apnea.BinaryFileReader

printfn "Hello from F#"

let ev1 : ApneaEvent = {
    StartDate = (SleepStartDate "2020-10-11")
    EventTime = (TimeString "23:45")
    EventType = Obstructive
}

// printfn "%s" (apneaEventToString ev1)
let allDays = importAllFolders

(* for day in  allDays
    do 
        printfn "%s %d" (sleepStartDateToString day.StartDate) day.Sessions.Length
        for sess in day.Sessions do 
            printfn "  %s" sess *)

testReadHeader

// readOtherMethod "d:\\documents\\cpap\\DATALOG\\20220708\\20220708_234211_BRP.edf"
