﻿module MainApp

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.IO
open FSharpx
open ParseIn

type MainWindow = XAML<"src/MainWindow.xaml">

let readBinaryFile (filePath:string) = File.ReadAllBytes(filePath)
let readFileLines (filePath:string) = File.ReadLines(filePath)

type App(window : MainWindow) = 
    let _window = window
    let _protoInput : TextBox = _window.Root.FindName "protoInput" |> unbox
    let _protoBrowse : Button = _window.Root.FindName "protoBrowse" |> unbox
    let _dataInput : TextBox = _window.Root.FindName "dataInput" |> unbox
    let _dataBrowse : Button = _window.Root.FindName "dataBrowse" |> unbox
    let _dataNew : Button = _window.Root.FindName "dataNew" |> unbox
    
    let tryReadProtoFile filePath =
        //async {
            if File.Exists(filePath) then
                let fileLines = readFileLines filePath
                let protoDescriptor = parseProtoFile filePath fileLines
                do ()
        //} |> Async.StartAsTask

    let previewDragHandler (e : DragEventArgs) =
        e.Effects <- DragDropEffects.Link
        e.Handled <- true

    let previewDropHandler (e : DragEventArgs) =
        let filePath : string [] = e.Data.GetData DataFormats.FileDrop |> unbox
        let source : TextBox = e.Source |> unbox
        source.Text <- String.Format("{0}", filePath.[0])

    let protoBrowseClickHandler (e : RoutedEventArgs) =
        let openFileDialog = new Forms.OpenFileDialog()
        openFileDialog.Filter <- "Proto files (*.proto)|*.proto|All Files (*.*)|*.*"
        let result = openFileDialog.ShowDialog ()
        if result.Equals(Forms.DialogResult.OK) then
            let filePath = openFileDialog.FileName
            let task = tryReadProtoFile filePath
            _protoInput.Text <- filePath

    let dataBrowseClickHandler (e : RoutedEventArgs) =
        let openFileDialog = new Forms.OpenFileDialog()
        openFileDialog.Filter <- "All Files (*.*)|*.*"
        let result = openFileDialog.ShowDialog ()
        if result.Equals(Forms.DialogResult.OK) then 
            _dataInput.Text <- openFileDialog.FileName

    let dataNewClickHandler (e : RoutedEventArgs) =
        let saveFileDialog = new Forms.SaveFileDialog()
        saveFileDialog.Filter <- "Binary file (*.bin)|*.bin"
        saveFileDialog.FileName <- "data"
        saveFileDialog.DefaultExt <- ".bin"
        let result = saveFileDialog.ShowDialog ()
        if result.Equals(Forms.DialogResult.OK) then 
            _dataInput.Text <- saveFileDialog.FileName

    do
        _window.Root.Loaded.Add(fun _ ->
            _protoInput.PreviewDragOver.Add(previewDragHandler)
            _protoInput.PreviewDrop.Add(previewDropHandler)
            _protoInput.GotFocus.Add(protoBrowseClickHandler)
            _protoBrowse.Click.Add(protoBrowseClickHandler)
            _dataInput.PreviewDragOver.Add(previewDragHandler)
            _dataInput.PreviewDrop.Add(previewDropHandler) 
            _dataInput.GotFocus.Add(dataBrowseClickHandler)
            _dataBrowse.Click.Add(dataBrowseClickHandler)
            _dataNew.Click.Add(dataNewClickHandler)
        )

    member this.Root = _window.Root

    new() = App(new MainWindow())

let loadWindow() =
    let app = App()
    app.Root

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore