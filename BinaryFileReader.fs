// based on https://stackoverflow.com/questions/49457111/fsharp-binaryreader-how-to-read-till-the-end-of-a-file

namespace apnea

module BinaryFileReader =

    open System  // convert
    open System.IO  // File, BinaryReader
    open ApneaModel

    type EdfHeader = {        
        StartDateTime: SleepStartDate
        BytesInHeader: int
        NumberOfRecords: int        
        RecordLength: int
    }

    let skipReaderToPosition (reader:BinaryReader) (position:int) =
        reader.BaseStream.Seek(position, SeekOrigin.Begin) |> ignore 


    let fileLength (filepath : string): int =         
        let f : FileInfo = FileInfo filepath
        int(f.Length) // int64 -> int32 could be lossy, but EDF files are not that big


    let getNBytesAsString (reader : BinaryReader) (nBytes : int): string =    
        let byteArray = reader.ReadBytes(nBytes)
        System.Text.Encoding.Default.GetString(byteArray).Trim()

    // string (dd.mm.yyhh.mm.ss) -> string (yymmdd hh.mm.ss)
    let formatEdfDateTime (edfDateTime : string) : string =
        sprintf "Start Date %s-%s-%s %s" 
            edfDateTime.[6..7]
            edfDateTime.[3..4]
            edfDateTime.[0..1]
            (edfDateTime.[8..].Replace(".", ":"))

    let readEdfHeader (filename : string) : EdfHeader =
        use stream = File.Open(filename, FileMode.Open, FileAccess.Read)
        use reader = new BinaryReader(stream)

        // #0 -> #8 (8)
        // look for 0 in first 8 bytes; fail if not found
        // STREAM POSITION 0 to 7
        let firstEight = getNBytesAsString reader 8
        if (firstEight <> "0") then failwith (sprintf "First eight bytes in file doesn't match expectations: %s" filename)
        
        // #8 -> #168 (160)
        // SKIP 2 x 80 bytes to position 168
        // let skip0 = getNBytesAsString reader 160
        skipReaderToPosition reader  168

        // #168 -> 184 (16)
        // START DATE spec is 2 x 8 bytes: 29.08.2223.45.18 (dd.mm.yyhh.mm.sssss)
        let dateString : string = getNBytesAsString reader 16
        printfn "date string %s" dateString

        // # 184 -> 192 (8) 
        // no skip: consecutive reads
        let bytesInHeader = int(getNBytesAsString reader 8)
        
        // # 192 -> 236 (44)
        // SKIP 44 reserved
        // let skip1 = getNBytesAsString reader 44
        // _ = br.ReadBytes(44);
        skipReaderToPosition reader 236

        // NUMBER OF RECORDS
        // numberOfRecords = int.Parse(GetStringFromNBytes(8));
        let numberOfRecords = int(getNBytesAsString reader 8)
        printfn "number of records %d" numberOfRecords

       
        let recordLength = ((fileLength filename) - bytesInHeader) / numberOfRecords

        {
            StartDateTime = SleepStartDate dateString
            BytesInHeader = bytesInHeader
            NumberOfRecords = numberOfRecords
            RecordLength = recordLength
        }


    let testReadHeader : unit =
        let header = readEdfHeader "d:\\documents\\cpap\\DATALOG\\20220708\\20220709_014647_EVE.edf"
        printfn "%O" header


(* 
        private void ReadEventsFromFile()
        {
            /*
             * Sample record
             * 0 +9240Hypopnea                 Ü§+             * 
             */

            // Position stream at beginning of first record
            // Skip header and first record that holds no data.
            int start = numberOfBytesInHeader + recordLength;
            for (int position = start; position < fileSize; position += recordLength)
            {
                // reposition reading head to start of next record
                br.BaseStream.Seek(position, SeekOrigin.Begin);
                byte[] byteArray = br.ReadBytes(recordLength);

                // SEQUENTIAL READING
                // each of the the following class will depend on the position of the previous ones to remain on track

                // the first data is at byte 6
                int nextPosition = 6;

                // Time of event
                string stringValue;
                // looking for NAK (hex: 15, dec: 21)
                (stringValue, nextPosition) = GetStringFromIndexToChar(byteArray, nextPosition, 21);
                int secondsSinceStart = int.Parse(stringValue);
                DateTime dateTimeOccurred = StartDateTime.AddSeconds(secondsSinceStart);

                // Duration of event
                // looking for DC4 (hex: 14, dec: 20)
                (stringValue, nextPosition) = GetStringFromIndexToChar(byteArray, nextPosition, 20);
                int durationSeconds = int.Parse(stringValue);

                // Description
                (stringValue, _) = GetStringFromIndexToChar(byteArray, nextPosition, 20);
                string description = stringValue;
                ApneaEvent apneaEvent = new(description: description, occurredDateTime: dateTimeOccurred, durationSeconds: durationSeconds);
                _events.Add(apneaEvent);
            }
        }


*)