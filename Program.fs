open apnea.ApneaModel
open apnea.ApneaFileReader

printfn "Hello from F#"

let ev1 : ApneaEvent = {
    StartDate = (SleepStartDate "2020-10-11")
    EventTime = (TimeString "23:45")
    EventType = Obstructive
}

// printfn "%s" (apneaEventToString ev1)
let allDays = importAllFolders
for item in  allDays
    do printfn "CpapDay %s" (sleepStartDateToString item.StartDate)


