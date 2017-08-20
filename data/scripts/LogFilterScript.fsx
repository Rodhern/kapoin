open System.IO


/// Read all lines from UTF-8 encoded text file as a sequence.
let linesFromFile filename =
    seq { use reader = File.OpenText filename
          while not reader.EndOfStream
             do yield reader.ReadLine () }


/// Create a new UTF-8 encoded text file and
/// write all lines from a sequence to the new file.
let linesTofile filename (lines: string seq) =
    use writer = File.CreateText filename
    for line in lines
     do writer.WriteLine line


/// Filter to apply for each line.
let lineFilter keeperPhrases discardPhrases (line: string) =
    (Array.exists line.Contains keeperPhrases)
    && not (Array.exists line.Contains discardPhrases)


let inputfilename = @"KSP.log"
let outputfilename = @"filtered.log"
let textToKeep = [| @"Kapoin"; @"[EXC"; @"[ERR"; @"[WRN"; |]
let textToDiscard = [| @"Debug (post)" |]


// Read, filter and write sequence:
do linesFromFile inputfilename
   |> Seq.filter (lineFilter textToKeep textToDiscard)
   |> linesTofile outputfilename
