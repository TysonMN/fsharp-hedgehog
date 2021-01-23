open System

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

open Hedgehog

[<CoreJob>]
type Benchmarks () =

    [<Benchmark>]
    member _.GenInts () =
        Property.check (property {
            let! i = Gen.int (Range.constant 0 10000)
            return i >= 0
        })

    [<Benchmark>]
    member _.GenAsciiStrings () =
        Property.check (property {
            let! i = Gen.string (Range.constant 0 100) Gen.ascii
            return i.Length >= 0
        })

    [<Benchmark>]
    member _.BigExampleFromTests () =
        Tests.MinimalTests.``greedy traversal with a predicate yields the perfect minimal shrink`` ()

[<CoreJob>]
type ScaledBenchmarks () =

    [<Params (100, 1000)>] // 10000 is too big at the moment, overflows.
    member val N = 1 with get, set

    [<Benchmark>]
    member this.ForLoopTest () =

        Property.check (property {
            for _ = 0 to this.N do
                ()

            return true
        })

[<EntryPoint>]
let main argv =
    //BenchmarkRunner.Run<Benchmarks> () |> ignore
    //BenchmarkRunner.Run<ScaledBenchmarks> () |> ignore

    let flip f b a = f a b
    let printTree t =
      t
      |> Tree.map (sprintf "%A")
      |> Tree.render
      |> printf "%s\n\n"

    let singleA = Tree.singleton "a"
    let letterChildren =
      [ Node ("b", [ singleA ])
        singleA ]
    let letters = Tree.Node ("c", letterChildren)
    let single0 = Tree.singleton 0
    let numberChildren =
      [ Node (1, [ single0 ])
        single0 ]
    let numbers = Tree.Node (2, numberChildren)
    let createPair a b = (a, b)

    printTree letters
    printTree numbers

    let apply ta tf =
      tf |> flip Tree.bind (fun f -> ta |> flip Tree.bind (fun a -> a |> f |> Tree.singleton))

    createPair
    |> Tree.singleton
    |> apply letters
    |> apply numbers
    |> printTree

    System.Console.ReadKey () |> ignore

    0 // return an integer exit code
